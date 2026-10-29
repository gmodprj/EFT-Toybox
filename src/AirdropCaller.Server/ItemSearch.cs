using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Locales;

namespace AirdropCaller.Server;

public record SearchRequest : IRequestData
{
    /// <summary>찾을 말. 한국어 이름·짧은 이름·tpl id 어느 쪽으로도 찾는다. 비우면 분류만으로 거른다.</summary>
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    /// <summary>이 분류(핸드북 카테고리 id)만. 비우면 전부.</summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 200;
}

public record FoundItem
{
    [JsonPropertyName("tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("shortName")] public string ShortName { get; set; } = string.Empty;

    /// <summary>핸드북 분류 이름. 외부 창이 이것으로 묶어 보여 준다.</summary>
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 이름에 붙어 있던 색(`#RRGGBB`). 없으면 빈 값.
    ///
    /// 다른 모드가 `&lt;color=#2694da&gt;` 로 등급을 색으로 표시한다. 태그를 글자에서
    /// 떼되 **그 정보까지 버리지는 않는다** — 창이 이 색으로 이름을 칠한다.
    /// </summary>
    [JsonPropertyName("color")] public string Color { get; set; } = string.Empty;

    /// <summary>격자에서 차지하는 칸.</summary>
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }

    /// <summary>한 칸에 묶을 수 있는 최대 개수. 1 이면 묶이지 않는 물건이다.</summary>
    [JsonPropertyName("maxStack")] public int MaxStack { get; set; } = 1;
}

public record SearchResponse
{
    [JsonPropertyName("items")] public List<FoundItem> Items { get; set; } = [];

    /// <summary>자른 뒤에도 더 있었는지. 외부 창이 "더 좁혀 보라" 를 띄운다.</summary>
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

public record CategoryInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>이 분류에 담긴 아이템 수. 0 인 분류는 돌려주지 않는다.</summary>
    [JsonPropertyName("count")] public int Count { get; set; }
}

public record CategoryListResponse
{
    [JsonPropertyName("categories")] public List<CategoryInfo> Categories { get; set; } = [];
}

/// <summary>
/// 아이템을 찾는다. 이름으로도 찾고 **분류로 훑기도** 한다.
///
/// 이름은 로케일 사전(`kr.json`)의 `"&lt;tpl&gt; Name"` 에 있다. 분류는 핸드북
/// (`templates/handbook.json`)의 `Items[].ParentId` → `Categories[].Id` 이고, 그 이름도
/// 로케일에서 읽는다. **이 설치본의 `kr.json` 은 분류 이름을 번역해 두지 않아 영어로
/// 나온다** — 묶어 보는 데는 지장이 없어 그대로 쓴다.
///
/// 크기와 최대 스택은 아이템 템플릿에서 읽는다. DB 가 유일한 진실이다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ItemSearch(LocaleService localeService, ItemHelper itemHelper, TemplateTable templateTable)
{
    private const string NameSuffix = " Name";

    private Dictionary<string, string>? _categoryOfItem;
    private Dictionary<string, string>? _categoryNames;
    private readonly object _lock = new();

    /// <summary>
    /// tpl → 분류 id, 분류 id → 이름. 한 번만 만든다. 핸드북은 2만 줄쯤 되어
    /// 요청마다 훑을 것이 아니다.
    /// </summary>
    private void EnsureCategories(IReadOnlyDictionary<string, string> locale)
    {
        lock (_lock)
        {
            if (_categoryOfItem is not null)
            {
                return;
            }

            var handbook = templateTable.Handbook;

            // 이름은 kr → en → 우리 표 → 자리표시자 순으로 찾는다. **순서가 중요하다** —
            // 자리표시자를 먼저 넣어 두면 우리 표가 영영 안 쓰인다(한 번 그렇게 틀렸다).
            var fallback = localeService.GetLocaleDb("en");
            var names = new Dictionary<string, string>();
            foreach (var category in handbook.Categories ?? [])
            {
                var id = category.Id.ToString();
                var name = Named(locale, id) ?? Named(fallback, id) ?? KnownNames.GetValueOrDefault(id);
                if (name is not null)
                {
                    names[id] = name;
                }
            }

            var ofItem = new Dictionary<string, string>();
            foreach (var item in handbook.Items ?? [])
            {
                ofItem[item.Id.ToString()] = item.ParentId.ToString();
            }

            _categoryNames = names;
            _categoryOfItem = ofItem;
        }
    }

    /// <summary>
    /// 로케일에서 쓸 만한 이름을 꺼낸다.
    ///
    /// 값이 비어 있는 것만 거르면 안 된다 — **id 를 값으로 들고 있는 항목이 있다**
    /// (실측: `5795f317245977243854e041` 의 kr·en 값이 그 id 자신이다). 그런 값은
    /// 이름이 아니므로 없는 것으로 친다.
    /// </summary>
    private static string? Named(IReadOnlyDictionary<string, string> locale, string id)
    {
        if (!locale.TryGetValue(id, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var looksLikeId = value.Length == 24 && value.All(c => char.IsAsciiHexDigitLower(c));
        return looksLikeId ? null : value;
    }

    /// <summary>
    /// 분류 id 의 표시 이름.
    ///
    /// **핸드북 `Items[].ParentId` 중에는 `Categories` 에 없는 id 가 있다**
    /// (실측: `5795f317245977243854e041`, 아이템 1개). 그래서 분류 표를 만들 때만이
    /// 아니라 **아이템에서 거슬러 올라올 때도** 같은 규칙으로 이름을 붙여야 한다 —
    /// 한 곳만 고쳤다가 24자리 id 가 그대로 목록에 남았다.
    /// </summary>
    private string NameOfCategory(string id) =>
        _categoryNames!.TryGetValue(id, out var known) ? known
        : KnownNames.TryGetValue(id, out var ours) ? ours
        : $"(이름 없는 분류 {id[..Math.Min(6, id.Length)]})";

    /// <summary>
    /// 로케일에도 핸드북에도 이름이 없는데 **큰** 분류는 우리가 이름을 붙인다.
    /// 아래 것은 아이템 454개짜리다(아라미드·강철 삽입물 등) — id 로 두면 못 찾는다.
    /// </summary>
    private static readonly Dictionary<string, string> KnownNames = new()
    {
        ["6564b96a189fe36f356d177c"] = "방어판·삽입물",
    };

    public CategoryListResponse ListCategories()
    {
        var locale = localeService.GetLocaleDb("kr");
        EnsureCategories(locale);

        var counts = new Dictionary<string, int>();
        foreach (var (_, categoryId) in _categoryOfItem!)
        {
            counts[categoryId] = counts.GetValueOrDefault(categoryId) + 1;
        }

        var list = new List<CategoryInfo>();
        foreach (var (id, count) in counts)
        {
            list.Add(new CategoryInfo
            {
                Id = id,
                Name = NameOfCategory(id),
                Count = count,
            });
        }

        // 이름이 있는 것부터, 그 안에서는 가나다·abc 순. 이름 없는 것은 맨 뒤로 보낸다 —
        // 목록 맨 위가 "(이름 없는 분류 …)" 인 것은 처음 여는 사람에게 쓸모가 없다.
        list.Sort((a, b) =>
        {
            var aUnnamed = a.Name.StartsWith('(');
            var bUnnamed = b.Name.StartsWith('(');
            if (aUnnamed != bUnnamed)
            {
                return aUnnamed ? 1 : -1;
            }

            return string.Compare(a.Name, b.Name, StringComparison.CurrentCulture);
        });

        return new CategoryListResponse { Categories = list };
    }

    public SearchResponse Search(string? query, string? category, int limit)
    {
        var response = new SearchResponse();
        var needle = query?.Trim() ?? string.Empty;
        var wantCategory = category?.Trim() ?? string.Empty;

        // 둘 다 비면 2만 개를 쏟게 된다. 적어도 하나는 있어야 한다.
        if (needle.Length == 0 && wantCategory.Length == 0)
        {
            return response;
        }

        var locale = localeService.GetLocaleDb("kr");
        EnsureCategories(locale);

        var capped = Math.Clamp(limit <= 0 ? 200 : limit, 1, 1000);

        foreach (var (key, value) in locale)
        {
            if (!key.EndsWith(NameSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            var tpl = key.Substring(0, key.Length - NameSuffix.Length);

            if (wantCategory.Length > 0
                && _categoryOfItem!.GetValueOrDefault(tpl, string.Empty) != wantCategory)
            {
                continue;
            }

            // 태그를 먼저 뗀다. 안 그러면 "color" 나 "2694da" 로도 걸리고,
            // 이름 안의 글자를 찾을 때 태그가 끼어든다.
            var raw = value ?? string.Empty;
            var name = StripTags(raw);
            if (needle.Length > 0
                && name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0
                && tpl.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var found = Describe(tpl, name, ExtractColor(raw), locale);
            if (found is null)
            {
                continue;
            }

            if (response.Items.Count >= capped)
            {
                response.Truncated = true;
                break;
            }

            response.Items.Add(found);
        }

        // 분류로 묶어 보게 분류 → 이름 순으로 정렬해 보낸다.
        response.Items.Sort((a, b) =>
        {
            var byCategory = string.Compare(a.Category, b.Category, StringComparison.CurrentCulture);
            return byCategory != 0 ? byCategory : string.Compare(a.Name, b.Name, StringComparison.CurrentCulture);
        });

        return response;
    }

    /// <summary>
    /// 이름에 섞인 꾸밈 태그를 뗀다.
    ///
    /// `kr.json` 파일 자체에는 태그가 없다(실측: 이름 5297개 중 0개). **다른 모드가
    /// 런타임에 이름을 덧칠한다** — `&lt;b&gt;&lt;color=#2694da&gt;Kalashnikov AK-74N…`
    /// 처럼. 게임 안에서는 그 태그가 색으로 보이지만 우리 창은 글자 그대로 찍어
    /// 읽을 수가 없었다. 붙은 별표(`★`) 같은 것은 그대로 둔다 — 그건 글자다.
    /// </summary>
    /// <summary>
    /// 이름에 박힌 `&lt;color=#RRGGBB&gt;` 에서 색만 꺼낸다. 없으면 빈 값.
    /// 등급을 색으로 나타내는 모드가 붙여 둔 것이라 **버리면 구분이 사라진다.**
    /// </summary>
    private static string ExtractColor(string text)
    {
        const string marker = "color=#";
        var at = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (at < 0)
        {
            return string.Empty;
        }

        var start = at + marker.Length;
        var end = start;
        while (end < text.Length && Uri.IsHexDigit(text[end]) && end - start < 8)
        {
            end++;
        }

        var hex = text[start..end];
        return hex.Length is 6 or 8 ? "#" + hex : string.Empty;
    }

    private static string StripTags(string text)
    {
        if (text.IndexOf('<') < 0)
        {
            return text;
        }

        var sb = new System.Text.StringBuilder(text.Length);
        var depth = 0;
        foreach (var c in text)
        {
            if (c == '<')
            {
                depth++;
            }
            else if (c == '>')
            {
                if (depth > 0)
                {
                    depth--;
                }
            }
            else if (depth == 0)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Trim();
    }

    private FoundItem? Describe(string tpl, string name, string color, IReadOnlyDictionary<string, string> locale)
    {
        MongoId id;
        try
        {
            id = new MongoId(tpl);
        }
        catch
        {
            // 로케일에는 아이템이 아닌 키도 많다(상인·퀘스트 등). 조용히 넘어간다.
            return null;
        }

        var (ok, template) = itemHelper.GetItem(id);
        if (!ok || template?.Properties is null)
        {
            return null;
        }

        var categoryId = _categoryOfItem!.GetValueOrDefault(tpl, string.Empty);
        var props = template.Properties;
        return new FoundItem
        {
            Tpl = tpl,
            Name = name,
            Color = color,
            ShortName = locale.TryGetValue(tpl + " ShortName", out var shortName) ? StripTags(shortName) : string.Empty,
            Category = categoryId.Length > 0 ? NameOfCategory(categoryId) : "(분류 없음)",
            Width = props.Width ?? 1,
            Height = props.Height ?? 1,
            MaxStack = props.StackMaxSize ?? 1,
        };
    }
}
