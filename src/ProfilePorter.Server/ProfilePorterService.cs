using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils.Cloners;

namespace ProfilePorter.Server;

/// <summary>라우트가 돌려주는 답. 거절도 **말로** 돌려준다 — 조용히 실패하지 않는다.</summary>
public sealed record PorterResult(bool Ok, string? Reason, string? FileName, int Count, IReadOnlyList<string> Skipped);

/// <summary>
/// 내보내기와 들여오기를 조립한다.
///
/// 지키는 순서가 둘이다.
/// 1. **내보내기는 아무것도 쓰지 않는다**(안티목표 1). 프로필을 읽어 파일 하나를 만들 뿐이다.
/// 2. **들여오기는 백업 → 복제본에서 다 해 보기 → 실물에 쓰기 → 저장** 이다. 터질 수 있는 것은
///    실물을 건드리기 전에 끝낸다(STM 의 전송이 "저널을 열기 전에 끝낸다" 로 쓰는 것과 같은 규율).
///
/// 저널은 쓰지 않는다 — 설계 §2.1 이 그 이유를 적었다(복제 도구라 저널의 불변식이 성립하지 않는다).
/// 대신 **한 프로필만 고친다**. 반만 끝난 상태가 두 파일에 걸치지 않는다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ProfilePorterService(
    ProfileReader profiles,
    PorterFiles files,
    StashPlacer placer,
    ConfigStore store,
    SaveServer saveServer,
    ICloner cloner,
    ISptLogger<ProfilePorterService> logger)
{
    public PorterResult ExportStash(string? profileId, IReadOnlyList<string>? itemIds, string? name, string? note)
    {
        var pmc = profiles.Pmc(profileId);
        if (pmc?.Inventory is null)
        {
            return Fail("그 프로필을 찾지 못했습니다.");
        }

        var items = ProfileReader.ToPorterItems(pmc.Inventory.Items);
        var plan = ExportSelection.PlanStash(
            items, pmc.Inventory.Stash?.ToString(), itemIds ?? [], store.Config.BlockedTemplateIds);

        if (!plan.Accepted)
        {
            return new PorterResult(false, plan.Reason, null, 0, plan.Skipped);
        }

        return WriteFile(PorterBundle.KindStash, pmc.Info?.Nickname, profileId!, items, plan, itemIds!, name, note);
    }

    public PorterResult ExportLoadout(string? profileId, string? name, string? note)
    {
        var pmc = profiles.Pmc(profileId);
        if (pmc?.Inventory is null)
        {
            return Fail("그 프로필을 찾지 못했습니다.");
        }

        var items = ProfileReader.ToPorterItems(pmc.Inventory.Items);
        var plan = ExportSelection.PlanLoadout(
            items, pmc.Inventory.Equipment?.ToString(), store.Config.BlockedTemplateIds);

        if (!plan.Accepted)
        {
            return new PorterResult(false, plan.Reason, null, 0, plan.Skipped);
        }

        // 한 벌의 뿌리는 장비 루트에 바로 달린 것들이다.
        var equipRoot = pmc.Inventory.Equipment?.ToString();
        var roots = items
            .Where(i => string.Equals(i.ParentId, equipRoot, StringComparison.Ordinal)
                        && plan.ItemIds.Contains(i.Id, StringComparer.Ordinal))
            .Select(i => i.Id)
            .ToList();

        return WriteFile(PorterBundle.KindLoadout, pmc.Info?.Nickname, profileId!, items, plan, roots, name, note);
    }

    /// <summary>
    /// 파일 하나를 그 프로필의 스태시로 들여온다. **되돌릴 수 없는 쓰기**이므로 백업이 먼저다.
    /// </summary>
    public PorterResult Import(string? profileId, string? fileName)
    {
        var file = files.Read(fileName);
        if (file is null)
        {
            return Fail("그 파일을 읽지 못했습니다.");
        }

        if (!string.Equals(file.Format, PorterFile.CurrentFormat, StringComparison.Ordinal))
        {
            return Fail($"모르는 파일 형식입니다('{file.Format}').");
        }

        var pmc = profiles.Pmc(profileId);
        if (pmc?.Inventory is null)
        {
            return Fail("받는 프로필을 찾지 못했습니다.");
        }

        var stashRootId = pmc.Inventory.Stash?.ToString();
        if (string.IsNullOrEmpty(stashRootId))
        {
            return Fail("받는 프로필의 스태시를 찾지 못했습니다.");
        }

        // 1) 순수 판정 — id 를 새로 매기고 꾸러미가 온전한지 본다. 아직 아무것도 안 썼다.
        var bundle = new PorterBundle(
            file.Kind,
            file.RootIds,
            file.Items.Select(i => new PorterItem(i.Id, i.ParentId, i.SlotId, i.TemplateId, i.Upd)).ToList());

        var plan = ImportPlanner.Plan(bundle, () => new MongoId().ToString());
        if (!plan.Accepted)
        {
            return new PorterResult(false, plan.Reason, fileName, 0, []);
        }

        // 2) **복제본 위에서** 자리까지 찾아 본다. 여기서 실패하면 실물은 손대지 않은 채 끝난다.
        var live = pmc.Inventory.Items ?? [];
        var trial = cloner.Clone(live) ?? [];
        trial.AddRange(plan.Items.Select(ToSptItem));

        if (!placer.TryPlaceAll(trial, stashRootId, plan.RootIds, out var failure))
        {
            return new PorterResult(false, failure ?? "자리를 찾지 못했습니다.", fileName, 0, []);
        }

        // 3) 백업. **실패하면 들여오지 않는다** — 되돌릴 길 없이 나아가지 않는다(설계 §4.6).
        if (store.Config.BackupBeforeImport && files.BackupProfile(profileId!) is null)
        {
            return Fail("들여오기 전 백업에 실패해 아무것도 쓰지 않았습니다.");
        }

        // 4) 실물에 쓴다. 자리는 복제본에서 이미 정해졌으므로 그 결과를 그대로 옮긴다.
        var placed = trial.Where(i => plan.Items.Any(p => string.Equals(p.Id, i.Id.ToString(), StringComparison.Ordinal)));
        live.AddRange(placed);
        pmc.Inventory.Items = live;

        // 여기까지 왔다는 것은 `profiles.Pmc` 가 이 id 로 프로필을 찾았다는 뜻이다.
        saveServer.SaveProfileAsync(new MongoId(profileId!)).GetAwaiter().GetResult();

        logger.Info($"[ProfilePorter] '{fileName}' 의 {plan.Items.Count}개를 "
                    + $"{pmc.Info?.Nickname}({profileId}) 스태시에 들여왔다 — 원본은 그대로다.");

        return new PorterResult(true, null, fileName, plan.Items.Count, []);
    }

    private PorterResult WriteFile(
        string kind,
        string? nickname,
        string profileId,
        List<PorterItem> items,
        ExportPlan plan,
        IReadOnlyList<string> rootIds,
        string? name,
        string? note)
    {
        var picked = plan.ItemIds.ToHashSet(StringComparer.Ordinal);
        var file = new PorterFile
        {
            Kind = kind,
            CreatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            FromProfileId = profileId,
            FromNickname = nickname ?? "(이름 없음)",
            Note = note,
            RootIds = rootIds.Where(picked.Contains).ToList(),
            Items = items.Where(i => picked.Contains(i.Id)).Select(ToFileItem).ToList(),
        };

        var written = files.Write(file, name);
        if (written is null)
        {
            return Fail("파일을 쓰지 못했습니다.");
        }

        logger.Info($"[ProfilePorter] {file.FromNickname} 에서 {file.Items.Count}개를 "
                    + $"'{written}.json' 으로 내보냈다 — 프로필은 고치지 않았다.");

        return new PorterResult(true, null, written, file.Items.Count, plan.Skipped);
    }

    private static PorterFileItem ToFileItem(PorterItem item) => new()
    {
        Id = item.Id,
        TemplateId = item.TemplateId,
        ParentId = item.ParentId,
        SlotId = item.SlotId,
        Upd = item.Upd switch
        {
            null => null,
            JsonElement element => element,
            // 프로필에서 온 `Upd` 는 SPT 타입이다. 해석하지 않고 JSON 으로 한 번 돌려 담는다 —
            // 필드를 아는 척하면 모르는 것이 조용히 떨어진다.
            var other => JsonSerializer.SerializeToElement(other),
        },
    };

    private static Item ToSptItem(PorterItem item) => new()
    {
        Id = new MongoId(item.Id),
        Template = new MongoId(item.TemplateId),
        ParentId = item.ParentId,
        SlotId = item.SlotId,
        Upd = item.Upd switch
        {
            null => null,
            JsonElement element => element.Deserialize<Upd>(),
            Upd already => already,
            _ => null,
        },
    };

    private static PorterResult Fail(string reason) => new(false, reason, null, 0, []);
}
