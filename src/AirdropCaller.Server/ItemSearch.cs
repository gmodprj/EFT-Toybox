using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Locales;

namespace AirdropCaller.Server;

public record SearchRequest : IRequestData
{
    /// <summary>찾을 말. 한국어 이름·짧은 이름·tpl id 어느 쪽으로도 찾는다.</summary>
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 50;
}

public record FoundItem
{
    [JsonPropertyName("tpl")]
    public string Tpl { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortName")]
    public string ShortName { get; set; } = string.Empty;

    /// <summary>격자에서 차지하는 칸. 외부 창이 "상자에 들어갈까" 를 가늠하는 데 쓴다.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>한 칸에 묶을 수 있는 최대 개수. 1 이면 묶이지 않는 물건이다.</summary>
    [JsonPropertyName("maxStack")]
    public int MaxStack { get; set; } = 1;
}

public record SearchResponse
{
    [JsonPropertyName("items")]
    public List<FoundItem> Items { get; set; } = [];

    /// <summary>자른 뒤에도 더 있었는지. 외부 창이 "더 좁혀 보라" 를 띄운다.</summary>
    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}

/// <summary>
/// 한국어 이름으로 아이템을 찾는다.
///
/// 이름은 로케일 사전(`kr.json`)에 `"<tpl> Name"` / `"<tpl> ShortName"` 으로 들어 있다.
/// 크기와 최대 스택은 아이템 템플릿에서 읽는다 — **DB 가 유일한 진실이다.**
/// 설정 파일(`StackUnify` 등)을 보지 않는 이유는 2단계 설계에 적어 둔 것과 같다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ItemSearch(LocaleService localeService, ItemHelper itemHelper)
{
    private const string NameSuffix = " Name";

    public SearchResponse Search(string? query, int limit)
    {
        var response = new SearchResponse();
        if (string.IsNullOrWhiteSpace(query))
        {
            return response;
        }

        var needle = query.Trim();
        var locale = localeService.GetLocaleDb("kr");
        var capped = Math.Clamp(limit <= 0 ? 50 : limit, 1, 200);

        foreach (var (key, value) in locale)
        {
            if (!key.EndsWith(NameSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            var tpl = key.Substring(0, key.Length - NameSuffix.Length);
            var name = value ?? string.Empty;

            // id 를 그대로 붙여 넣는 경우도 받아 준다 — 설정 파일에서 옮겨 올 때 편하다.
            var hit = name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0
                      || tpl.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
            if (!hit)
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

        // 짧은 이름이 먼저 오게. "탄" 을 찾으면 "탄창" 보다 "탄" 이 위에 있는 쪽이 쓸 만하다.
        response.Items.Sort((a, b) => a.Name.Length.CompareTo(b.Name.Length));
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

        var props = template.Properties;
        return new FoundItem
        {
            Tpl = tpl,
            Name = name,
            ShortName = locale.TryGetValue(tpl + " ShortName", out var s) ? s : string.Empty,
            Width = props.Width ?? 1,
            Height = props.Height ?? 1,
            MaxStack = props.StackMaxSize ?? 1,
        };
    }
}
