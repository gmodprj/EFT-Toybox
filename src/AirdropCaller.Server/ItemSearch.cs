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
            var names = new Dictionary<string, string>();
            foreach (var category in handbook.Categories ?? [])
            {
                var id = category.Id.ToString();
                names[id] = locale.TryGetValue(id, out var n) && !string.IsNullOrWhiteSpace(n) ? n : id;
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
                Name = _categoryNames!.GetValueOrDefault(id, id),
                Count = count,
            });
        }

        list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));
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

            var name = value ?? string.Empty;
            if (needle.Length > 0
                && name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0
                && tpl.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var found = Describe(tpl, name, locale);
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

    private FoundItem? Describe(string tpl, string name, IReadOnlyDictionary<string, string> locale)
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
            ShortName = locale.TryGetValue(tpl + " ShortName", out var s) ? s : string.Empty,
            Category = categoryId.Length > 0 ? _categoryNames!.GetValueOrDefault(categoryId, categoryId) : "(분류 없음)",
            Width = props.Width ?? 1,
            Height = props.Height ?? 1,
            MaxStack = props.StackMaxSize ?? 1,
        };
    }
}
