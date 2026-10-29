using System.IO;
using System.IO.Compression;
using System.Net.Http;

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
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;

    /// <summary>이름에 붙어 있던 등급 색(`#RRGGBB`). 없으면 빈 값.</summary>
    [JsonPropertyName("color")] public string Color { get; set; } = string.Empty;

    [JsonPropertyName("width")] public int Width { get; set; } = 1;
    [JsonPropertyName("height")] public int Height { get; set; } = 1;
    [JsonPropertyName("maxStack")] public int MaxStack { get; set; } = 1;

    /// <summary>즐겨찾기 표시. 서버가 주는 값이 아니라 창이 채운다.</summary>
    [JsonIgnore] public string Star { get; set; } = string.Empty;

    public string Size => $"{Width}×{Height}" + (MaxStack > 1 ? $" ·{MaxStack}" : string.Empty);

    public string Display => $"{Name}   [{Size}]";
}

public record CategoryInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }

    public string Display => Id.Length == 0 ? Name : $"{Name}  ({Count})";
}

public record CategoryListResponse
{
    [JsonPropertyName("categories")] public List<CategoryInfo> Categories { get; set; } = [];
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

    /// <summary>
    /// SPT 는 응답 본문을 zlib 으로 **압축해서 보내면서 `Content-Encoding` 을 달지 않는다.**
    /// 그래서 `HttpClient` 의 자동 해제가 걸리지 않는다. 게임 클라이언트
    /// (`SPT.Common.Http.Client`)도 바이트로 받아 `Zlib.IsCompressed` 로 보고 직접 푼다 —
    /// 같은 방식을 따른다.
    ///
    /// 보내는 쪽은 평범한 JSON 이어도 받아 준다(실측).
    /// </summary>
    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        if (bytes.Length == 0)
        {
            return default;
        }

        if (LooksLikeZlib(bytes))
        {
            using var input = new MemoryStream(bytes);
            using var zlib = new ZLibStream(input, CompressionMode.Decompress);
            return await JsonSerializer.DeserializeAsync<T>(zlib, Options, ct);
        }

        return JsonSerializer.Deserialize<T>(bytes, Options);
    }

    /// <summary>
    /// zlib 머리글: 첫 바이트가 0x78 이고 두 바이트를 묶은 값이 31 로 나누어떨어진다.
    /// JSON 은 `{` 나 `[` 로 시작하므로 섞일 일이 없다.
    /// </summary>
    private static bool LooksLikeZlib(byte[] bytes) =>
        bytes.Length >= 2 && bytes[0] == 0x78 && ((bytes[0] << 8) | bytes[1]) % 31 == 0;

    /// <summary>
    /// **요청 본문도 zlib 으로 압축해야 한다.** 평범한 JSON 을 보내면 서버가 200 을
    /// 돌려주지만 본문을 못 읽어 빈 응답이 온다(실측). 게임 클라이언트도
    /// `Zlib.Compress` 로 보낸다 — 같은 방식을 따른다.
    /// </summary>
    private static ByteArrayContent Compress(object body)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(body, Options);
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(json, 0, json.Length);
        }

        var content = new ByteArrayContent(output.ToArray());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        return content;
    }

    private async Task<T?> PostAsync<T>(string path, object? body, CancellationToken ct)
    {
        using var content = Compress(body ?? new { });
        using var response = await _http.PostAsync(path, content, ct);
        return await ReadAsync<T>(response, ct);
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await _http.GetAsync(path, ct);
        return await ReadAsync<T>(response, ct);
    }

    public Task<StateResponse?> GetStateAsync(CancellationToken ct) =>
        GetAsync<StateResponse>("/airdrop-caller/state", ct);

    public Task<PresetListResponse?> ListPresetsAsync(CancellationToken ct) =>
        GetAsync<PresetListResponse>("/airdrop-caller/presets", ct);

    public Task<SimpleResponse?> SavePresetAsync(Preset preset, CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/presets/save", new { preset }, ct);

    public Task<SimpleResponse?> DeletePresetAsync(string id, CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/presets/delete", new { id }, ct);

    public Task<SearchResponse?> SearchAsync(string query, string? category, int limit, CancellationToken ct) =>
        PostAsync<SearchResponse>("/airdrop-caller/items", new { query, category, limit }, ct);

    public Task<CategoryListResponse?> ListCategoriesAsync(CancellationToken ct) =>
        GetAsync<CategoryListResponse>("/airdrop-caller/categories", ct);

    public Task<QueueResponse?> QueueAsync(string? presetId, string icon, List<ItemSpec> items, CancellationToken ct) =>
        PostAsync<QueueResponse>("/airdrop-caller/queue", new { presetId, icon, items }, ct);

    public Task<SimpleResponse?> CancelAsync(CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/cancel", null, ct);

    public void Dispose() => _http.Dispose();
}
