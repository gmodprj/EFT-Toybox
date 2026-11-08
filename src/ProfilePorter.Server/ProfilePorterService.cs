using System.Text.Json.Serialization;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Commerce;

namespace ProfilePorter.Server;

/// <summary>라우트가 돌려주는 답. 거절도 **말로** 돌려준다 — 조용히 실패하지 않는다.</summary>
public sealed record PorterResult(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("fileName")] string? FileName,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("skipped")] IReadOnlyList<string> Skipped);

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
    ConfigStore store,
    MailSendService mailSendService,
    ISptLogger<ProfilePorterService> logger)
{
    public PorterResult ExportStash(
        string? profileId, IReadOnlyList<string>? itemIds, string? name, string? note, bool overwrite, bool rename)
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

        return WriteFile(PorterBundle.KindStash, pmc.Info?.Nickname, profileId!, items, plan, itemIds!, name, note, overwrite, rename);
    }

    public PorterResult ExportLoadout(string? profileId, string? name, string? note, bool overwrite, bool rename)
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

        return WriteFile(PorterBundle.KindLoadout, pmc.Info?.Nickname, profileId!, items, plan, roots, name, note, overwrite, rename);
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

        // 2) 백업. **실패하면 보내지 않는다** — 되돌릴 길 없이 나아가지 않는다(설계 §4.6).
        //    우편도 프로필(`DialogueRecords`)에 쓰이므로 여전히 되돌릴 길을 남긴다.
        if (store.Config.BackupBeforeImport && files.BackupProfile(profileId!) is null)
        {
            return Fail("보내기 전 백업에 실패해 아무것도 쓰지 않았습니다.");
        }

        // 3) **우편으로 보낸다**(설계 §14). 스태시에 꽂지 않는다 —
        //    자리 찾기가 없고, 받을지는 사람이 우편함에서 정한다.
        //    뿌리는 `ParentId` 가 비어 있다(P2 가 그렇게 둔다). 우편 목록이 바라는 모양 그대로다.
        var items = plan.Items.Select(ToSptItem).ToList();
        var days = Math.Clamp(store.Config.MailStorageDays, 1, 365);
        var from = file.FromNickname;
        var what = string.Equals(file.Kind, PorterBundle.KindLoadout, StringComparison.Ordinal)
            ? "입고 있던 한 벌"
            : "스태시의 물건";

        try
        {
            mailSendService.SendSystemMessageToPlayer(
                new MongoId(profileId!),
                $"[Profile Porter] {from} 의 {what} {items.Count}개를 보냈습니다. ({days}일 안에 받으세요)",
                items,
                days * 24 * 60 * 60,
                null);
        }
        catch (Exception e)
        {
            logger.Error($"[ProfilePorter] 우편을 보내지 못했다: {e.Message}");
            return Fail($"우편으로 보내지 못했습니다: {e.Message}");
        }

        logger.Info($"[ProfilePorter] '{fileName}' 의 {plan.Items.Count}개를 "
                    + $"{pmc.Info?.Nickname}({profileId}) 에게 **우편으로** 보냈다({days}일 보관) — 원본은 그대로다.");

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
        string? note,
        bool overwrite,
        bool rename)
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

        var written = files.Write(file, name, overwrite, rename, out var refusal);
        if (written is null)
        {
            // 같은 이름이라 거절한 것은 **실패가 아니라 물음**이다. 창이 사유를 보고 사람에게 묻는다.
            return Fail(refusal ?? "파일을 쓰지 못했습니다.");
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
            // 필드를 아는 척하면 모르는 것이 조용히 떨어진다. 파일과 **같은 설정**을 쓴다:
            // 다르면 담을 때와 적을 때의 모양이 갈린다(빈 필드가 여기서 들어와 거기서 빠진다).
            var other => JsonSerializer.SerializeToElement(other, PorterFiles.Options),
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
