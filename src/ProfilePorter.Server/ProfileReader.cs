using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Locales;

namespace ProfilePorter.Server;

/// <summary>
/// 프로필 하나의 간추린 모습. 창이 목록을 그리는 데 쓴다.
///
/// 열쇠 이름을 적어 둔다 — `record` 는 그냥 두면 **PascalCase 로 나가고**, 익명 객체로 만든
/// 다른 응답(`ok`·`items`)은 camelCase 로 나가 한 모드가 두 모양을 섞어 내보내게 된다.
/// </summary>
public sealed record StashEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tpl")] string Tpl,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("stack")] int Stack,
    [property: JsonPropertyName("contained")] int Contained);

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
            floor.Add(new StashEntry(
                item.Id.ToString(),
                tpl,
                ResolveName(locale, tpl),
                (int)(item.Upd?.StackObjectsCount ?? 1),
                CountDescendants(item.Id.ToString(), byParent)));
        }

        return floor;
    }

    /// <summary>짧은 이름 → 긴 이름 → 그래도 없으면 tpl. 빈 줄을 그리지 않는다.</summary>
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

    /// <summary>판정이 쓰는 모양으로 줄인다. <c>Upd</c> 는 **들여다보지 않고 그대로** 들고 간다.</summary>
    public static List<PorterItem> ToPorterItems(IEnumerable<Item>? items) =>
        (items ?? [])
        .Where(i => i is not null)
        .Select(i => new PorterItem(
            i.Id.ToString(),
            i.ParentId,
            i.SlotId,
            i.Template.ToString(),
            i.Upd))
        .ToList();

    private static bool Same(string? a, string? b) =>
        a is not null && b is not null && string.Equals(a, b, StringComparison.Ordinal);
}
