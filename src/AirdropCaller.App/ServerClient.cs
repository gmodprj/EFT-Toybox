using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AirdropCaller.App;

// ── 서버와 주고받는 모양. AirdropCaller.Server 의 Contracts.cs 와 짝이다. ──

public record ItemSpec
{
    [JsonPropertyName("Tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("Count")] public int Count { get; set; } = 1;
    [JsonPropertyName("StackSize")] public int? StackSize { get; set; }
}

public record Preset
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("icon")] public string Icon { get; set; } = "Common";
    [JsonPropertyName("items")] public List<ItemSpec> Items { get; set; } = [];
}

public record PresetListResponse
{
    [JsonPropertyName("presets")] public List<Preset> Presets { get; set; } = [];
}

public record FoundItem
{
    [JsonPropertyName("tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("shortName")] public string ShortName { get; set; } = string.Empty;
    [JsonPropertyName("width")] public int Width { get; set; } = 1;
    [JsonPropertyName("height")] public int Height { get; set; } = 1;
    [JsonPropertyName("maxStack")] public int MaxStack { get; set; } = 1;

    public string Display => $"{Name}   [{Width}×{Height}{(MaxStack > 1 ? $", 최대 {MaxStack}" : "")}]";
}

public record SearchResponse
{
    [JsonPropertyName("items")] public List<FoundItem> Items { get; set; } = [];
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

public record StateResponse
{
    [JsonPropertyName("gameConnected")] public bool GameConnected { get; set; }
    [JsonPropertyName("inRaid")] public bool InRaid { get; set; }
    [JsonPropertyName("map")] public string? Map { get; set; }
    [JsonPropertyName("canSummon")] public bool CanSummon { get; set; }
    [JsonPropertyName("crateInFlight")] public bool CrateInFlight { get; set; }
    [JsonPropertyName("availablePoints")] public int AvailablePoints { get; set; }
    [JsonPropertyName("hasPending")] public bool HasPending { get; set; }
    [JsonPropertyName("cooldownSeconds")] public int CooldownSeconds { get; set; }
    [JsonPropertyName("canQueue")] public bool CanQueue { get; set; }
    [JsonPropertyName("blockedBy")] public string? BlockedBy { get; set; }
}

public record QueueResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("dropId")] public string? DropId { get; set; }
    [JsonPropertyName("placed")] public int Placed { get; set; }
    [JsonPropertyName("skipped")] public List<string> Skipped { get; set; } = [];
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public record SimpleResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

/// <summary>
/// SPT 서버의 `/airdrop-caller/*` 를 부른다.
///
/// 인증은 게임 클라이언트와 **똑같이** 한다 — `Cookie: PHPSESSID=&lt;세션 id&gt;`
/// (`SPT.Common.Http.Client` 가 그렇게 보낸다). 라우트를 인증에서 빼지 않는다:
/// 전리품을 만들어 내는 자리다.
///
/// 서버는 자체 서명 인증서를 쓴다. **127.0.0.1 에 한해서만** 검증을 건너뛴다.
/// </summary>
public sealed class ServerClient : IDisposable
{
    private readonly HttpClient _http;

    public ServerClient(string baseUrl, string sessionId)
    {
        var uri = new Uri(baseUrl);
        var loopback = uri.IsLoopback;

        var handler = new HttpClientHandler();
        if (loopback)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        _http = new HttpClient(handler)
        {
            BaseAddress = uri,
            Timeout = TimeSpan.FromSeconds(10),
        };
        _http.DefaultRequestHeaders.Add("Cookie", "PHPSESSID=" + sessionId);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private async Task<T?> PostAsync<T>(string path, object? body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(path, body ?? new { }, Options, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(Options, ct);
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(Options, ct);
    }

    public Task<StateResponse?> GetStateAsync(CancellationToken ct) =>
        GetAsync<StateResponse>("/airdrop-caller/state", ct);

    public Task<PresetListResponse?> ListPresetsAsync(CancellationToken ct) =>
        GetAsync<PresetListResponse>("/airdrop-caller/presets", ct);

    public Task<SimpleResponse?> SavePresetAsync(Preset preset, CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/presets/save", new { preset }, ct);

    public Task<SimpleResponse?> DeletePresetAsync(string id, CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/presets/delete", new { id }, ct);

    public Task<SearchResponse?> SearchAsync(string query, int limit, CancellationToken ct) =>
        PostAsync<SearchResponse>("/airdrop-caller/items", new { query, limit }, ct);

    public Task<QueueResponse?> QueueAsync(string? presetId, string icon, List<ItemSpec> items, CancellationToken ct) =>
        PostAsync<QueueResponse>("/airdrop-caller/queue", new { presetId, icon, items }, ct);

    public Task<SimpleResponse?> CancelAsync(CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/cancel", null, ct);

    public void Dispose() => _http.Dispose();
}
