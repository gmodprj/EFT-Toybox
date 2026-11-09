using System.Text.Json.Serialization;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
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
    SaveServer saveServer,
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
            file.Items.Select(i => new PorterItem(i.Id, i.ParentId, i.SlotId, i.TemplateId, i.Upd, i.Location)).ToList());

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
        var unlocked = 0;
        var items = new List<Item>(plan.Items.Count);
        foreach (var planned in plan.Items)
        {
            items.Add(ToSptItem(planned, ref unlocked));
        }
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

        // 4) **디스크에 쓴다.** `MailSendService` 는 프로필의 `DialogueRecords` 를 **메모리에서만**
        //    고친다 — 저장하지 않으면 서버가 꺼질 때 우편이 통째로 날아간다. 꽂아 넣던 때에는
        //    바로 뒤에 `SaveProfileAsync` 가 있었는데, 우편으로 바꾸면서 그 줄까지 지웠다
        //    (2026-11-08 실측: 보낸 뒤 프로필 파일에 우편이 없었다).
        try
        {
            saveServer.SaveProfileAsync(new MongoId(profileId!)).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            // 우편은 이미 메모리에 들어갔다. 저장만 실패한 것이라 **거짓으로 성공을 알리지 않는다.**
            logger.Error($"[ProfilePorter] 우편은 보냈으나 프로필을 저장하지 못했다: {e.Message}");
            return Fail($"우편은 보냈지만 저장하지 못했습니다 — 서버를 끄면 사라집니다: {e.Message}");
        }

        var unlockedNote = unlocked > 0 ? $"  잠금을 푼 것 {unlocked}개." : string.Empty;

        logger.Info($"[ProfilePorter] '{fileName}' 의 {plan.Items.Count}개를 "
                    + $"{pmc.Info?.Nickname}({profileId}) 에게 **우편으로** 보냈다({days}일 보관) — 원본은 그대로다."
                    + unlockedNote);

        // 푼 것은 `skipped`(=말해 주는 자리)에 적는다. 조용히 바꾸지 않는다.
        var said = unlocked > 0
            ? new List<string> { $"잠겨 있던 {unlocked}개의 잠금을 풀어 보냈습니다 — 잠긴 채로는 우편함에서 꺼낼 수 없습니다." }
            : [];

        return new PorterResult(true, null, fileName, plan.Items.Count, said);
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

        // `Location` 도 **같은 식으로 그대로** 담는다. 읽어 온 프로필에서는 `JsonElement`,
        // 서버가 만든 것은 `ItemLocation` 이라(§5-3) 두 모양을 다 받아야 한다 —
        // `as ItemLocation` 만 쓰면 읽기 경로에서 조용히 null 이 되어 §20 의 결함이 돌아온다.
        Location = item.Location switch
        {
            null => null,
            JsonElement element => element,
            var other => JsonSerializer.SerializeToElement(other, PorterFiles.Options),
        },
    };

    /// <summary>
    /// 우편에 담을 모양으로 바꾼다.
    ///
    /// **잠금은 푼다.** `PinLockState.Locked` 인 아이템은 게임이 "옮기거나 쓰거나 고치거나
    /// 버리거나 건넬 수 없다" 로 막아서, 그대로 보내면 **받는 사람이 우편함에서 꺼내지도
    /// 못한다**(2026-11-08 사용자 제보). 잠금은 "그 프로필에서 실수로 건드리지 않겠다" 는
    /// 표시이지 물건의 성질이 아니므로, 남의 우편으로 갈 때 들고 갈 이유가 없다.
    ///
    /// `Pinned` 는 그대로 둔다 — 자동 정렬에서 자리를 지키라는 표시일 뿐 꺼내는 것을 막지 않는다.
    /// 푼 개수는 <paramref name="unlocked"/> 로 세어 **말해 준다.** 조용히 바꾸지 않는다.
    /// </summary>
    private static Item ToSptItem(PorterItem item, ref int unlocked)
    {
        var upd = item.Upd switch
        {
            null => null,
            JsonElement element => element.Deserialize<Upd>(),
            Upd already => already,
            _ => null,
        };

        if (upd?.PinLockState == PinLockState.Locked)
        {
            upd.PinLockState = PinLockState.Free;
            unlocked++;
        }

        return new Item
        {
            Id = new MongoId(item.Id),
            Template = new MongoId(item.TemplateId),
            ParentId = item.ParentId,
            SlotId = item.SlotId,
            Upd = upd,

            // **해석하지 않고 그대로 꽂는다.** `Item.Location` 은 `object` 이고, 프로필을 읽어
            // 온 아이템은 서버 자신도 `JsonElement` 로 들고 있다(§5-3) — 그 모양이 프로필에
            // 다시 적힐 때 `{x, y, r}` 로 그대로 나간다. `ItemLocation` 으로 한 번 돌리면
            // 우리가 뜻을 모르는 `rotation` 같은 필드가 조용히 떨어진다.
            Location = item.Location,
        };
    }

    /// <summary>
    /// 내보낸 파일 하나를 지운다. 창이 **사람에게 묻고 나서** 부른다.
    /// 지우는 것은 그 파일뿐이고, 이미 보낸 우편과 받은 물건은 그대로다.
    /// </summary>
    public PorterResult DeleteFile(string? fileName)
    {
        if (!files.Delete(fileName, out var refusal))
        {
            return Fail(refusal ?? "지우지 못했습니다.");
        }

        return new PorterResult(true, null, fileName, 0, []);
    }

    /// <summary>
    /// 내보낸 파일의 **한 자리**를 세부 보기 화면 모양으로 돌려준다(설계 §4.2).
    ///
    /// 파일 전체를 한 번에 내보내지 않는다 — 1,361개짜리가 있어서, 고른 자리의 자식만 추린다.
    /// 판정은 <see cref="DetailPlanner"/> 가 하고, 이름·크기·격자 칸 수처럼 **DB 가 아는 것**만
    /// 여기서 붙인다.
    /// </summary>
    public object Detail(string? fileName, string? containerId)
    {
        var file = files.Read(fileName);
        if (file is null)
        {
            return new { ok = false, reason = "그 파일을 열지 못했습니다." };
        }

        var items = file.Items
            .Select(i => new PorterItem(i.Id, i.ParentId, i.SlotId, i.TemplateId, i.Upd, i.Location))
            .ToList();
        var bundle = new PorterBundle(file.Kind, file.RootIds, items);
        var byParent = items
            .Where(i => !string.IsNullOrEmpty(i.ParentId))
            .GroupBy(i => i.ParentId!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(i => i.Id).ToList(), StringComparer.Ordinal);

        var notices = new List<string>();
        var atRoot = string.IsNullOrEmpty(containerId);
        var rootIds = file.RootIds.ToHashSet(StringComparer.Ordinal);

        var children = atRoot
            ? items.Where(i => rootIds.Contains(i.Id)).ToList()
            : DetailPlanner.ChildrenOf(bundle, containerId).ToList();

        // **뿌리에서 끊긴 가지도 보여 준다.** 창은 그 id 를 알 길이 없어 지목할 수 없으므로,
        // 여기서 올려 주지 않으면 사람은 그 물건이 애초에 없었다고 읽는다.
        var lost = atRoot ? DetailPlanner.Unreachable(bundle) : [];

        if (!atRoot && items.All(i => !string.Equals(i.Id, containerId, StringComparison.Ordinal)))
        {
            return new { ok = false, reason = "그 파일에 없는 자리입니다." };
        }

        // **tpl 하나당 DB 를 한 번만 묻는다.** 자식마다 `GetItem` 을 두세 번 하면 1,361개짜리
        // 파일에서 한 번 펼칠 때마다 수백~천 번이 된다(설계 §3 의 "몇 초씩 멈추면 실패").
        var facts = new Dictionary<string, TplFacts>(StringComparer.Ordinal);
        TplFacts Facts(string tpl)
        {
            if (facts.TryGetValue(tpl, out var known)) return known;

            var made = new TplFacts(
                profiles.NameOf(tpl), profiles.SizeOf(tpl), profiles.GridsOf(tpl), profiles.SlotsOf(tpl));
            facts[tpl] = made;
            return made;
        }

        var tree = children
            .Concat(lost)
            .Select(c => new
            {
                id = c.Id,
                tpl = c.TemplateId,
                name = Facts(c.TemplateId).Name,
                slotName = SlotNames.Korean(c.SlotId),
                contained = Descendants(c.Id, byParent),
                kind = Facts(c.TemplateId).Grids.Count > 0 || Facts(c.TemplateId).Slots.Count > 0
                    ? "container"
                    : "item",
            })
            .ToList();

        object view;

        if (atRoot && string.Equals(file.Kind, PorterBundle.KindLoadout, StringComparison.Ordinal))
        {
            view = new
            {
                kind = "loadout",
                slots = children.Select(c => Slot(c.SlotId, c, byParent, Facts)).ToList(),
                loose = lost.Select(c => Loose(c, LostReason, byParent, Facts)).ToList(),
            };

            if (lost.Count > 0)
            {
                notices.Add($"뿌리에 매달려 있지 않은 것 {lost.Count}개를 따로 모았습니다.");
            }
        }
        else if (atRoot)
        {
            var placed = children.Where(c => c.Location is not null).ToList();
            var missing = children.Count - placed.Count;
            var rows = placed.Select(c => Row(c, byParent, Facts)).ToList();

            // **고른 것만 담긴 파일이다.** 보낸 사람 스태시의 절대 좌표를 그대로 그리면,
            // 뿌리 하나짜리 파일이 10x80 짜리 **빈 판**으로 보인다(2026-11-09 사용자 제보:
            // "다 안나옴"). 서로의 자리 관계는 지키되 **쓰는 자리로 당겨서** 그린다.
            var shiftX = rows.Count > 0 ? rows.Min(r => r.X) : 0;
            var shiftY = rows.Count > 0 ? rows.Min(r => r.Y) : 0;

            if (shiftX > 0 || shiftY > 0)
            {
                rows = rows.Select(r => r with { X = r.X - shiftX, Y = r.Y - shiftY }).ToList();
            }

            var (cellsH, cellsV) = DetailPlanner.StashCells(
                0, 0, rows.Select(r => (r.X, r.Y, r.W, r.H)));

            // 보낸 프로필의 스태시보다 넓어질 수는 없다 — 넓으면 우리 셈이 틀린 것이다.
            var (profileH, _) = profiles.StashCellsOf(file.FromProfileId);
            if (profileH > 0 && cellsH > profileH)
            {
                cellsH = profileH;
            }

            if (missing > 0)
            {
                notices.Add(missing == children.Count
                    ? $"자리 정보가 없는 것 {missing}개는 아래에 모았습니다 — 2026-11-08 이전에 내보낸 파일입니다. 다시 내보내면 자리까지 들어옵니다."
                    : $"자리 정보가 없는 것 {missing}개는 아래에 모았습니다.");
            }

            if (lost.Count > 0)
            {
                notices.Add($"뿌리에 매달려 있지 않은 것 {lost.Count}개를 따로 모았습니다.");
            }

            AddOverlapNotice(rows.Select(r => (r.X, r.Y, r.W, r.H)), notices);

            view = new
            {
                kind = "stash",
                cellsH,
                cellsV,
                rows,
                loose = children.Where(c => c.Location is null)
                    .Select(c => Loose(c, "자리 정보가 없습니다", byParent, Facts))
                    .Concat(lost.Select(c => Loose(c, LostReason, byParent, Facts)))
                    .ToList(),
            };
        }
        else
        {
            var container = items.First(i => string.Equals(i.Id, containerId, StringComparison.Ordinal));
            var known = Facts(container.TemplateId);
            var place = DetailPlanner.Place(children, known.Grids, known.Slots);

            foreach (var slot in place.Slots.Where(s => !SlotNames.Knows(s.SlotId)))
            {
                logger.Info($"[ProfilePorter] 사전에 없는 슬롯 이름이다: {slot.SlotId}");
            }

            if (place.Loose.Count > 0)
            {
                notices.Add($"자리를 잡지 못한 것 {place.Loose.Count}개는 아래에 모았습니다.");
            }

            view = new
            {
                kind = "container",
                grids = place.Grids.Select(g =>
                {
                    var gridRows = g.Rows.Select(r => Row(r, byParent, Facts)).ToList();
                    AddOverlapNotice(gridRows.Select(r => (r.X, r.Y, r.W, r.H)), notices);

                    // 상자 격자도 **큰 쪽**을 쓴다(설계 §7-5). DB 가 준 칸 수보다 큰 좌표가 있으면
                    // 그대로 그렸을 때 그 물건이 격자 밖이라 **스크롤로도 닿지 않는다.**
                    // 칸 수를 못 읽은 격자(0)도 여기서 테두리로 잡힌다.
                    var (cellsH, cellsV) = DetailPlanner.StashCells(
                        g.CellsH, g.CellsV, gridRows.Select(r => (r.X, r.Y, r.W, r.H)));

                    return new
                    {
                        name = g.Name,
                        gridName = SlotNames.Korean(g.Name),
                        cellsH,
                        cellsV,
                        rows = gridRows,
                    };
                }).ToList(),
                slots = place.Slots
                    .Select(s => Slot(s.SlotId, items.First(i => i.Id == s.Id), byParent, Facts)).ToList(),
                loose = place.Loose
                    .Select(l => Loose(items.First(i => i.Id == l.Id), l.Why, byParent, Facts)).ToList(),
            };
        }

        return new
        {
            ok = true,

            // **파일이 아니라 폴더가 가진 사실이다.** `Read` 는 이름을 채우지 않는다(`List` 만
            // 채운다) — 그대로 내보내면 빈 글자가 나간다(2026-11-09 실측에서 봤다).
            fileName = string.IsNullOrEmpty(file.FileName) ? fileName : file.FileName,
            kind = file.Kind,
            containerId,
            notices,
            tree,
            view,
        };
    }

    /// <summary>
    /// 겹치는 칸이 있으면 **왜 그런지**를 적어 준다. 겹침은 우리가 잘못 그려서가 아니라,
    /// 모드가 아이템 크기를 바꿔 **놓을 때의 크기와 지금 크기가 어긋나서** 생긴다.
    /// 말해 주지 않으면 사람은 창이 깨진 줄로 읽는다(2026-11-09 사용자 제보).
    /// </summary>
    private static void AddOverlapNotice(IEnumerable<(int x, int y, int w, int h)> rows, List<string> notices)
    {
        var overlaps = DetailPlanner.CountOverlaps(rows);
        if (overlaps == 0)
        {
            return;
        }

        notices.Add($"겹쳐 보이는 칸이 {overlaps}개 있습니다 — 모드가 아이템 크기를 바꾼 뒤에도 "
                    + "물건은 놓였던 자리에 그대로 있어서입니다. 물건이 사라진 것은 아닙니다.");
    }

    /// <summary>한 번의 세부 보기에서 **tpl 하나당 한 번만** 모으는 DB 지식.</summary>
    private sealed record TplFacts(
        string Name, (int w, int h) Size, IReadOnlyList<GridSpec> Grids, IReadOnlySet<string> Slots);

    private const string LostReason = "뿌리에 매달려 있지 않습니다";

    private static StashEntry Row(
        PorterItem item, Dictionary<string, List<string>> byParent, Func<string, TplFacts> facts)
    {
        var (x, y, rotated) = ProfileReader.LocationOf(item.Location);
        var (w, h) = facts(item.TemplateId).Size;
        var (cw, ch) = rotated ? (h, w) : (w, h);

        return new StashEntry(
            item.Id,
            item.TemplateId,
            facts(item.TemplateId).Name,
            StackOf(item),
            Descendants(item.Id, byParent),
            x, y, cw, ch, rotated);
    }

    private static object Slot(
        string? slotId, PorterItem item, Dictionary<string, List<string>> byParent, Func<string, TplFacts> facts) => new
    {
        slotId,
        slotName = SlotNames.Korean(slotId),
        id = item.Id,
        tpl = item.TemplateId,
        name = facts(item.TemplateId).Name,
        stack = StackOf(item),
        contained = Descendants(item.Id, byParent),
        summary = Summary(item),
    };

    private static object Loose(
        PorterItem item, string why, Dictionary<string, List<string>> byParent, Func<string, TplFacts> facts) => new
    {
        id = item.Id,
        tpl = item.TemplateId,
        name = facts(item.TemplateId).Name,
        slotName = SlotNames.Korean(item.SlotId),
        stack = StackOf(item),
        contained = Descendants(item.Id, byParent),
        summary = Summary(item),
        why,
    };

    private static int Descendants(string id, Dictionary<string, List<string>> byParent)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal) { id };
        var queue = new Queue<string>();
        queue.Enqueue(id);
        var n = 0;

        while (queue.Count > 0)
        {
            if (!byParent.TryGetValue(queue.Dequeue(), out var kids)) continue;
            foreach (var kid in kids)
            {
                if (!seen.Add(kid)) continue;
                n++;
                queue.Enqueue(kid);
            }
        }

        return n;
    }

    /// <summary>묶음 수. `upd` 를 **읽기만** 한다.</summary>
    private static int StackOf(PorterItem item) =>
        item.Upd is JsonElement { ValueKind: JsonValueKind.Object } upd
        && upd.TryGetProperty("StackObjectsCount", out var stack)
        && stack.TryGetInt32(out var n) && n > 0 ? n : 1;

    /// <summary>
    /// 칸에 적을 짧은 글. **아는 것만 적는다** — 모르는 값은 지어내지 않는다.
    /// 창이 `upd` 스키마를 알게 되지 않도록 여기서 만든다(설계 §7-4).
    /// </summary>
    private static string Summary(PorterItem item)
    {
        if (item.Upd is not JsonElement { ValueKind: JsonValueKind.Object } upd)
        {
            return string.Empty;
        }

        var parts = new List<string>();

        if (upd.TryGetProperty("StackObjectsCount", out var stack)
            && stack.TryGetInt32(out var n) && n > 1)
        {
            parts.Add($"×{n}");
        }

        if (upd.TryGetProperty("Repairable", out var repairable)
            && repairable.ValueKind == JsonValueKind.Object
            && repairable.TryGetProperty("Durability", out var dur)
            && repairable.TryGetProperty("MaxDurability", out var max)
            && dur.TryGetDouble(out var d) && max.TryGetDouble(out var m))
        {
            parts.Add($"내구 {Math.Round(d)}/{Math.Round(m)}");
        }

        if (upd.TryGetProperty("MedKit", out var med)
            && med.ValueKind == JsonValueKind.Object
            && med.TryGetProperty("HpResource", out var hp) && hp.TryGetDouble(out var hpv))
        {
            parts.Add($"잔량 {Math.Round(hpv)}");
        }

        if (upd.TryGetProperty("Resource", out var res)
            && res.ValueKind == JsonValueKind.Object
            && res.TryGetProperty("Value", out var rv) && rv.TryGetDouble(out var rvv))
        {
            parts.Add($"자원 {Math.Round(rvv)}");
        }

        if (upd.TryGetProperty("Key", out var key)
            && key.ValueKind == JsonValueKind.Object
            && key.TryGetProperty("NumberOfUsages", out var used) && used.TryGetInt32(out var usedN))
        {
            parts.Add($"사용 {usedN}회");
        }

        return string.Join(" · ", parts);
    }

    private static PorterResult Fail(string reason) => new(false, reason, null, 0, []);
}
