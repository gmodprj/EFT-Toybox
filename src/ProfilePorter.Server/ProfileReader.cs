using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Helpers.Items;

namespace ProfilePorter.Server;

/// <summary>
/// 프로필 하나의 간추린 모습. 창이 목록을 그리는 데 쓴다.
///
/// 열쇠 이름을 적어 둔다 — `record` 는 그냥 두면 **PascalCase 로 나가고**, 익명 객체로 만든
/// 다른 응답(`ok`·`items`)은 camelCase 로 나가 한 모드가 두 모양을 섞어 내보내게 된다.
/// </summary>
/// <summary>
/// 스태시 바닥의 한 칸. 창이 **인게임과 같은 격자**로 그리는 데 쓴다 — 그래서 이름·개수뿐
/// 아니라 **자리(X·Y)와 크기(W·H)**까지 싣는다.
///
/// 크기는 `ItemHelper.GetItemSize` 가 준다. 그것은 **접힌 개머리판·부착물까지 본** 실제 차지
/// 칸이다 — tpl 의 Width/Height 만 보면 무기가 격자에서 실제보다 작게 그려진다.
/// 세워 놓은 것(`Rotated`)은 가로·세로를 바꿔서 그린다.
/// </summary>
public sealed record StashEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tpl")] string Tpl,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("stack")] int Stack,
    [property: JsonPropertyName("contained")] int Contained,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("w")] int W,
    [property: JsonPropertyName("h")] int H,
    [property: JsonPropertyName("rotated")] bool Rotated);

/// <summary>스태시 격자 자체의 크기. 서버 DB 가 정한다(§9.3 — 파일로 재면 틀린다).</summary>
public sealed record StashGrid(
    [property: JsonPropertyName("cellsH")] int CellsH,
    [property: JsonPropertyName("cellsV")] int CellsV);

public sealed record ProfileSummary(
    [property: JsonPropertyName("profileId")] string ProfileId,
    [property: JsonPropertyName("nickname")] string Nickname,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("stashFloorCount")] int StashFloorCount);

/// <summary>
/// 프로필을 **읽기만** 한다. 설계의 안티목표 1("내보내는 쪽 프로필이 바뀐다")이 여기 걸린다 —
/// 이 클래스에는 쓰는 메서드가 없다.
///
/// SPT 의 <see cref="Item"/> 을 판정이 쓰는 <see cref="PorterItem"/> 으로 줄인다. 판정(§P2)은
/// SPT 타입을 모르고, 그래야 게임 없이 시험할 수 있다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ProfileReader(
    SaveServer saveServer,
    LocaleService localeService,
    ItemHelper itemHelper,
    ISptLogger<ProfileReader> logger)
{
    /// <summary>이 서버가 들고 있는 프로필 전부. 못 읽는 것은 건너뛰고 적는다.</summary>
    public List<ProfileSummary> List()
    {
        var summaries = new List<ProfileSummary>();

        foreach (var (id, profile) in saveServer.GetProfiles())
        {
            var pmc = profile?.CharacterData?.PmcData;
            if (pmc?.Inventory is null)
            {
                logger.Warning($"[ProfilePorter] 프로필 {id} 를 읽지 못해 목록에서 뺐다.");
                continue;
            }

            var floor = (pmc.Inventory.Items ?? [])
                .Count(i => Same(i.ParentId, pmc.Inventory.Stash?.ToString())
                            && Same(i.SlotId, ExportSelection.StashSlot));

            summaries.Add(new ProfileSummary(
                id.ToString(),
                pmc.Info?.Nickname ?? "(이름 없음)",
                pmc.Info?.Level ?? 0,
                floor));
        }

        return summaries;
    }

    /// <summary>그 프로필의 PMC. 없으면 null — 던지지 않는다(밖에서 오는 id 로 불린다).</summary>
    public PmcData? Pmc(string? profileId)
    {
        if (!LooksLikeProfileId(profileId))
        {
            return null;
        }

        // `MongoId` 는 `TryParse` 가 없고 생김새가 틀리면 던진다. 밖에서 오는 글자라 먼저 본다.
        return saveServer.GetProfile(new MongoId(profileId))?.CharacterData?.PmcData;
    }

    /// <summary>24자리 소문자 hex 인지. 프로필 파일 이름이 쓰는 모양과 같다.</summary>
    public static bool LooksLikeProfileId(string? value) =>
        value is { Length: 24 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    /// <summary>
    /// 스태시 바닥에 놓인 것들. **이름을 붙여서** 돌려준다 — 창이 tpl 만 보여 주면 사람이
    /// 무엇을 고르는지 알 수 없다. 이름은 서버가 붙인다(STM 의 꾸러미 견적과 같은 자리).
    /// </summary>
    public List<StashEntry> StashFloor(PmcData pmc)
    {
        var items = pmc.Inventory?.Items ?? [];
        var stash = pmc.Inventory?.Stash?.ToString();
        var locale = localeService.GetLocaleDb();

        // 자손 수를 세려고 부모→자식 색인을 한 번만 만든다.
        var byParent = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.ParentId)) continue;
            if (!byParent.TryGetValue(item.ParentId!, out var list))
            {
                list = [];
                byParent[item.ParentId!] = list;
            }

            list.Add(item.Id.ToString());
        }

        var floor = new List<StashEntry>();
        foreach (var item in items)
        {
            if (!Same(item.ParentId, stash) || !Same(item.SlotId, ExportSelection.StashSlot))
            {
                continue;
            }

            var tpl = item.Template.ToString();

            // 크기는 **자손까지 본 실제 차지 칸**이다. 못 재면 1x1 로 둔다 — 격자에서 자리를
            // 조금 적게 차지해 보일 뿐, 그 때문에 무엇이 사라지지는 않는다.
            var size = SafeSize(item.Template);
            var (x, y, rotated) = ReadLocation(item.Location);
            var (w, h) = rotated ? (size.h, size.w) : (size.w, size.h);

            floor.Add(new StashEntry(
                item.Id.ToString(),
                tpl,
                ResolveName(locale, tpl),
                (int)(item.Upd?.StackObjectsCount ?? 1),
                CountDescendants(item.Id.ToString(), byParent),
                x,
                y,
                w,
                h,
                rotated));
        }

        return floor;
    }

    /// <summary>그 프로필 스태시 격자의 칸 수. **서버 DB 에서 읽는다**(설계 §9.3).</summary>
    public StashGrid StashSize(PmcData pmc)
    {
        var items = pmc.Inventory?.Items ?? [];
        var stash = items.FirstOrDefault(i =>
            string.Equals(i.Id.ToString(), pmc.Inventory?.Stash?.ToString(), StringComparison.Ordinal));

        if (stash is null)
        {
            return new StashGrid(0, 0);
        }

        var found = itemHelper.GetItem(stash.Template);
        var grid = (found.Key ? found.Value?.Properties?.Grids : null)
            ?.FirstOrDefault(g => g?.Properties?.CellsH is > 0 && g.Properties.CellsV is > 0);

        return new StashGrid(grid?.Properties?.CellsH ?? 0, grid?.Properties?.CellsV ?? 0);
    }

    /// <summary>
    /// 격자 안의 자리를 읽는다.
    ///
    /// **`Item.Location` 은 `object` 다.** 칸에 놓인 것은 `{x, y, r}` 이고 슬롯에 꽂힌 것은
    /// 정수 색인이라, SPT 가 한 타입으로 묶지 않았다. 프로필을 **읽어** 온 것은 역직렬화가
    /// `JsonElement` 로 남기고, 서버가 **만든** 것은 `ItemLocation` 이다 — 둘 다 받는다.
    ///
    /// `as ItemLocation` 만 쓰면 읽기 경로에서 조용히 null 이 되어 **모든 아이템이 (0,0)** 에
    /// 겹쳐 그려진다(2026-11-08 실측: 110개가 448칸 겹침).
    /// </summary>
    private static (int x, int y, bool rotated) ReadLocation(object? location)
    {
        switch (location)
        {
            case ItemLocation typed:
                return ((int)(typed.X ?? 0), (int)(typed.Y ?? 0), typed.R == ItemRotation.Vertical);

            case JsonElement { ValueKind: JsonValueKind.Object } element:
            {
                var x = element.TryGetProperty("x", out var xv) && xv.TryGetInt32(out var xi) ? xi : 0;
                var y = element.TryGetProperty("y", out var yv) && yv.TryGetInt32(out var yi) ? yi : 0;

                // 회전은 `r` 이 "Vertical" 이거나 1 로 온다. 둘 다 본다.
                var rotated = false;
                if (element.TryGetProperty("r", out var rv))
                {
                    rotated = rv.ValueKind switch
                    {
                        JsonValueKind.String => string.Equals(rv.GetString(), "Vertical", StringComparison.OrdinalIgnoreCase),
                        JsonValueKind.Number => rv.TryGetInt32(out var ri) && ri == 1,
                        _ => false,
                    };
                }

                return (x, y, rotated);
            }

            // 슬롯 색인(정수)이거나 자리가 없는 것. 스태시 바닥에서는 오지 않아야 한다.
            default:
                return (0, 0, false);
        }
    }

    /// <summary>
    /// 격자에서 차지하는 칸. **tpl 의 기본 크기**다.
    ///
    /// `ItemHelper.GetItemSize` 를 쓰면 안 된다 — 그것은 "이 트리를 격자에 **펼쳐 놓으면**
    /// 얼마나 필요한가" 를 재는 함수라, **컨테이너 안에 든 것까지 더한다.** 실측(2026-11-08):
    /// 같은 Item Case 가 안에 383개면 **38x4**, 77개면 11x5, 161개면 5x3 으로 제각각 나왔고
    /// 화면이 통째로 깨졌다. 스태시에서 상자는 안이 비었든 찼든 **같은 칸**을 차지한다.
    ///
    /// 못 재면 1x1 로 둔다. 밖에서 오는 자료로 불리는 자리라 던지게 두지 않는다.
    /// </summary>
    private (int w, int h) SafeSize(MongoId tpl)
    {
        try
        {
            var found = itemHelper.GetItem(tpl);
            var props = found.Key ? found.Value?.Properties : null;
            return (props?.Width ?? 1, props?.Height ?? 1);
        }
        catch (Exception e)
        {
            logger.Warning($"[ProfilePorter] {tpl} 의 크기를 재지 못해 1x1 로 둔다: {e.Message}");
            return (1, 1);
        }
    }

    /// <summary>짧은 이름 → 긴 이름 → 그래도 없으면 tpl. 빈 줄을 그리지 않는다.</summary>
    /// <summary>
    /// 그 tpl 이 가진 **격자들**. 상자 하나에 격자가 여럿이다(리그의 작은 주머니들) — 격자마다
    /// 칸 수가 다르다. 이름은 DB 의 `_name` 이고 **프로필의 `slotId` 와 같은 글자**다.
    /// </summary>
    public IReadOnlyList<GridSpec> GridsOf(string? tpl)
    {
        if (string.IsNullOrWhiteSpace(tpl)) return [];

        try
        {
            var found = itemHelper.GetItem(new MongoId(tpl));
            var grids = found.Key ? found.Value?.Properties?.Grids : null;

            return (grids ?? [])
                .Where(g => !string.IsNullOrEmpty(g?.Name))
                .Select(g => new GridSpec(
                    g!.Name!,
                    (int)(g.Properties?.CellsH ?? 0),
                    (int)(g.Properties?.CellsV ?? 0)))
                .ToList();
        }
        catch (Exception e)
        {
            logger.Warning($"[ProfilePorter] {tpl} 의 격자를 읽지 못했다: {e.Message}");
            return [];
        }
    }

    /// <summary>그 tpl 이 가진 **슬롯 이름들**. 플레이트·부착물이 꽂히는 자리다.</summary>
    public IReadOnlySet<string> SlotsOf(string? tpl)
    {
        if (string.IsNullOrWhiteSpace(tpl)) return new HashSet<string>(StringComparer.Ordinal);

        try
        {
            var found = itemHelper.GetItem(new MongoId(tpl));
            var slots = found.Key ? found.Value?.Properties?.Slots : null;

            return (slots ?? [])
                .Where(s => !string.IsNullOrEmpty(s?.Name))
                .Select(s => s!.Name!)
                .ToHashSet(StringComparer.Ordinal);
        }
        catch (Exception e)
        {
            logger.Warning($"[ProfilePorter] {tpl} 의 슬롯을 읽지 못했다: {e.Message}");
            return new HashSet<string>(StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// 격자에서 차지하는 칸 — **tpl 기본 크기**다(설계 §5-4).
    ///
    /// `MongoId` 는 생김새가 틀리면 **던진다.** 손으로 고친 파일의 `_tpl` 하나가 라우트 전체를
    /// 500 으로 만들지 않도록 try 안에서 만든다(`GridsOf`·`SlotsOf` 와 같은 규율).
    /// </summary>
    public (int w, int h) SizeOf(string? tpl)
    {
        if (string.IsNullOrWhiteSpace(tpl)) return (1, 1);

        try
        {
            return SafeSize(new MongoId(tpl));
        }
        catch (Exception e)
        {
            logger.Warning($"[ProfilePorter] {tpl} 는 아이템 id 모양이 아니라 1x1 로 둔다: {e.Message}");
            return (1, 1);
        }
    }

    /// <summary>아이템 이름. 사전에 없으면 tpl 을 그대로 돌려준다.</summary>
    public string NameOf(string tpl) => ResolveName(localeService.GetLocaleDb(), tpl);

    /// <summary>그 프로필의 스태시 격자 칸 수. 프로필이 없으면 (0, 0) — 부르는 쪽이 테두리를 쓴다.</summary>
    public (int cellsH, int cellsV) StashCellsOf(string? profileId)
    {
        var pmc = Pmc(profileId);
        if (pmc is null) return (0, 0);

        var grid = StashSize(pmc);
        return (grid.CellsH, grid.CellsV);
    }

    /// <summary>격자 안의 자리를 읽는다 — 서버 밖(파일에서 온 `JsonElement`)도 받는다.</summary>
    public static (int x, int y, bool rotated) LocationOf(object? location) => ReadLocation(location);

    private static string ResolveName(IDictionary<string, string>? locale, string tpl)
    {
        if (locale is not null && locale.TryGetValue($"{tpl} ShortName", out var shortName)
            && !string.IsNullOrWhiteSpace(shortName))
        {
            return shortName;
        }

        if (locale is not null && locale.TryGetValue($"{tpl} Name", out var name)
            && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        // 사전에 없으면 **그 사실이 보이게** 둔다.
        return tpl;
    }

    /// <summary>그 아이템 **안에** 든 것의 수(자기 자신은 빼고). 고리를 만나면 멈춘다.</summary>
    private static int CountDescendants(string id, Dictionary<string, List<string>> byParent)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal) { id };
        var queue = new Queue<string>();
        queue.Enqueue(id);
        var n = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!byParent.TryGetValue(current, out var children)) continue;
            foreach (var child in children)
            {
                if (!seen.Add(child)) continue;
                n++;
                queue.Enqueue(child);
            }
        }

        return n;
    }

    /// <summary>
    /// 판정이 쓰는 모양으로 줄인다. <c>Upd</c> 와 <c>Location</c> 은 **들여다보지 않고 그대로**
    /// 들고 간다 — 여기서 빠뜨리면 내보낸 파일에서도 빠진다(설계 §20 의 결함이 바로 이 줄에
    /// `Location` 이 없던 것이다).
    /// </summary>
    public static List<PorterItem> ToPorterItems(IEnumerable<Item>? items) =>
        (items ?? [])
        .Where(i => i is not null)
        .Select(i => new PorterItem(
            i.Id.ToString(),
            i.ParentId,
            i.SlotId,
            i.Template.ToString(),
            i.Upd,
            i.Location))
        .ToList();

    private static bool Same(string? a, string? b) =>
        a is not null && b is not null && string.Equals(a, b, StringComparison.Ordinal);
}
