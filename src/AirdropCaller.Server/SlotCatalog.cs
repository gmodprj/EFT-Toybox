using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// 칸 목록을 물어본다.
///
/// **지금 고른 것을 함께 보낸다.** 겹친 칸(헬멧 → 마운트 → 야시경)은 윗칸에 무엇을
/// 달았느냐에 따라 아랫칸이 달라진다. 모든 경우를 미리 보낼 수는 없으니, 창이 한 칸을
/// 바꿀 때마다 지금 상태로 다시 물어본다.
/// </summary>
public record SlotQueryRequest : IRequestData
{
    [JsonPropertyName("tpl")]
    public string? Tpl { get; set; }

    /// <summary>`Auto` · `All` · `None`. 비우면 `Auto`.</summary>
    [JsonPropertyName("fill")]
    public string? Fill { get; set; }

    /// <summary>칸 경로 → tpl. 빈 문자열은 "비워 둬라".</summary>
    [JsonPropertyName("slots")]
    public Dictionary<string, string>? Slots { get; set; }
}

/// <summary>칸 하나에 달 수 있는 것 하나.</summary>
public record SlotCandidate
{
    [JsonPropertyName("tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>분류 아이콘 파일 이름. 창이 자기 쪽 폴더에서 읽는다.</summary>
    [JsonPropertyName("icon")] public string Icon { get; set; } = string.Empty;

    /// <summary>방어 등급. 방어구가 아니면 0.</summary>
    [JsonPropertyName("armorClass")] public int ArmorClass { get; set; }

    /// <summary>핸드북 값. 자동이 고르는 기준이라 창에도 보여 준다.</summary>
    [JsonPropertyName("price")] public double Price { get; set; }

    /// <summary>
    /// 이미 단 것과 같이 달 수 없다. 창이 회색으로 막는다.
    /// **서버도 마지막에 한 번 더 본다** — `config.jsonc` 로 손으로 적는 길은 창을 안 거친다.
    /// </summary>
    [JsonPropertyName("blocked")] public bool Blocked { get; set; }
}

/// <summary>칸 하나.</summary>
public record SlotNode
{
    /// <summary>`Front_plate` 또는 `mod_mount.mod_nvg`.</summary>
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;

    /// <summary>0 이 품목 바로 밑. 창이 들여쓰기에 쓴다.</summary>
    [JsonPropertyName("depth")] public int Depth { get; set; }

    /// <summary>칸 이름(`Front_plate`). 게임이 쓰는 이름 그대로다.</summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("required")] public bool Required { get; set; }

    /// <summary>`soft`(삽입물) · `plate`(방어판) · `other`(장비).</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = "other";

    /// <summary>자동이 고를 tpl. 비어 있으면 자동도 못 고른다.</summary>
    [JsonPropertyName("auto")] public string Auto { get; set; } = string.Empty;

    /// <summary>지금 달려 있는 tpl. 비어 있으면 빈 칸이다.</summary>
    [JsonPropertyName("chosen")] public string Chosen { get; set; } = string.Empty;

    /// <summary>사용자가 직접 고른 칸인가. 자동이 고른 것과 구별해 보여 준다.</summary>
    [JsonPropertyName("picked")] public bool Picked { get; set; }

    [JsonPropertyName("candidates")] public List<SlotCandidate> Candidates { get; set; } = [];
}

public record SlotListResponse
{
    /// <summary>품목 이름. 창의 제목에 쓴다.</summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>칸이 하나도 없으면 비어 있다. 창은 "칸…" 버튼을 끈다.</summary>
    [JsonPropertyName("slots")] public List<SlotNode> Slots { get; set; } = [];

    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

/// <summary>
/// 칸 목록을 사람이 읽을 수 있게 꾸며 돌려준다.
///
/// 고르는 판단은 **하나도 하지 않는다** — 전부 <see cref="SlotFiller.Describe"/> 가
/// 한 것을 그대로 옮긴다. 창이 "자동: A" 라고 적어 놓고 상자에 B 가 들어 있는 것이
/// 제일 나쁘다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SlotCatalog(SlotFiller slotFiller, ItemSearch itemSearch, ItemHelper itemHelper)
{
    public SlotListResponse Describe(SlotQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Tpl))
        {
            return new SlotListResponse { Reason = "품목을 안 줬다." };
        }

        MongoId tpl;
        try
        {
            tpl = new MongoId(request.Tpl.Trim());
        }
        catch
        {
            return new SlotListResponse { Reason = $"'{request.Tpl}' 는 아이템 id 가 아니다." };
        }

        if (!itemHelper.GetItem(tpl).Key)
        {
            return new SlotListResponse { Reason = $"{tpl} 를 아이템 DB 에서 못 찾았다." };
        }

        var mode = Enum.TryParse<SlotFill>(request.Fill, ignoreCase: true, out var parsed)
            ? parsed
            : SlotFill.Auto;

        var found = slotFiller.Describe(tpl, mode, request.Slots);

        // 이름·아이콘은 한 번에 받아 온다. 칸마다 부르면 후보가 수백 개가 된다.
        var wanted = found.SelectMany(d => d.Pool).Select(t => t.ToString()).Append(tpl.ToString())
                          .Distinct().ToList();
        var known = itemSearch.Search(null, null, wanted.Count + 8, wanted)
                              .Items.ToDictionary(i => i.Tpl, i => i);

        return new SlotListResponse
        {
            Name = known.TryGetValue(tpl.ToString(), out var self) ? self.Name : tpl.ToString(),
            Slots = [.. found.Select(d => ToNode(d, known, found))],
        };
    }

    private SlotNode ToNode(
        SlotDecision decision, IReadOnlyDictionary<string, FoundItem> known, List<SlotDecision> all)
    {
        // 이 칸보다 **먼저** 처리된 칸에 달린 것들과 부딪히는지 본다. 뒤에 오는 칸이
        // 무엇을 달지는 이 칸을 고르고 나서야 정해지므로, 앞쪽만 본다.
        var before = all.TakeWhile(d => d.Path != decision.Path)
                        .Select(d => d.Chosen)
                        .Where(t => t is not null)
                        .Select(t => t!.Value)
                        .ToHashSet();

        var clashing = new HashSet<MongoId>();
        foreach (var placed in before)
        {
            var (ok, template) = itemHelper.GetItem(placed);
            if (ok && template?.Properties?.ConflictingItems is { } conflicts)
            {
                clashing.UnionWith(conflicts);
            }
        }

        return new SlotNode
        {
            Path = decision.Path,
            Depth = decision.Depth,
            Name = decision.Name,
            Required = decision.Required,
            Kind = decision.Kind,
            Auto = decision.Auto?.ToString() ?? string.Empty,
            Chosen = decision.Chosen?.ToString() ?? string.Empty,
            Picked = decision.Picked,
            Candidates =
            [
                .. decision.Pool
                    .Select(t => ToCandidate(t, known, clashing))
                    // 자동이 고르는 기준과 같은 순서로 보여 준다 — 비싼 것이 위다.
                    .OrderByDescending(c => c.Price)
                    .ThenBy(c => c.Name, StringComparer.Ordinal),
            ],
        };
    }

    private SlotCandidate ToCandidate(
        MongoId tpl, IReadOnlyDictionary<string, FoundItem> known, HashSet<MongoId> clashing)
    {
        var key = tpl.ToString();
        var (ok, template) = itemHelper.GetItem(tpl);
        var armor = ok && template?.Properties?.ArmorClass is { } raw ? ToInt(raw) : 0;

        return new SlotCandidate
        {
            Tpl = key,
            Name = known.TryGetValue(key, out var info) ? info.Name : key,
            Icon = known.TryGetValue(key, out var icon) ? icon.CategoryIcon : string.Empty,
            ArmorClass = armor,
            Price = slotFiller.PriceOf(tpl),
            Blocked = clashing.Contains(tpl),
        };
    }

    private static int ToInt(object raw)
    {
        try
        {
            return Convert.ToInt32(raw);
        }
        catch
        {
            return 0;
        }
    }
}
