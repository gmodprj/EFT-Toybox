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

    /// <summary>이 품목 안에 넣을 것. 격자를 가진 컨테이너일 때만 뜻이 있다.</summary>
    [JsonPropertyName("Items")] public List<ItemSpec>? Items { get; set; }

    /// <summary>
    /// 딸린 칸을 어디까지 채울지 — `Auto` · `All` · `None`. 비우면 `Auto`.
    /// 서버의 <c>SlotFill</c> 과 짝이다. 글자 그대로 오간다(숫자가 아니다).
    /// </summary>
    [JsonPropertyName("Fill")] public string? Fill { get; set; }

    /// <summary>
    /// 칸마다 직접 고른 것. 칸 경로 → tpl. 빈 문자열은 "비워 둬라".
    /// 겹친 칸은 점으로 잇는다(`mod_mount.mod_nvg`).
    /// </summary>
    [JsonPropertyName("Slots")] public Dictionary<string, string>? Slots { get; set; }
}

/// <summary>칸 하나에 달 수 있는 것 하나. 서버 `SlotCandidate` 와 짝이다.</summary>
public class SlotCandidate
{
    [JsonPropertyName("tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("icon")] public string Icon { get; set; } = string.Empty;
    [JsonPropertyName("armorClass")] public int ArmorClass { get; set; }
    [JsonPropertyName("price")] public double Price { get; set; }

    /// <summary>이미 단 것과 같이 못 단다. 고를 수 없게 막는다.</summary>
    [JsonPropertyName("blocked")] public bool Blocked { get; set; }
}

/// <summary>칸 하나. 서버 `SlotNode` 와 짝이다.</summary>
public class SlotNode
{
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
    [JsonPropertyName("depth")] public int Depth { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("required")] public bool Required { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = "other";
    [JsonPropertyName("auto")] public string Auto { get; set; } = string.Empty;
    [JsonPropertyName("chosen")] public string Chosen { get; set; } = string.Empty;
    [JsonPropertyName("picked")] public bool Picked { get; set; }
    [JsonPropertyName("candidates")] public List<SlotCandidate> Candidates { get; set; } = [];
}

public record SlotListResponse
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("slots")] public List<SlotNode> Slots { get; set; } = [];
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

/// <summary>
/// `record` 가 아니라 `class` 다. 창의 목록에 **그대로 얹히는 것**은 전부 그렇다 —
/// 이유는 <see cref="FoundItem"/> 에 적어 뒀다.
/// </summary>
public class Preset
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("icon")] public string Icon { get; set; } = "Common";
    [JsonPropertyName("items")] public List<ItemSpec> Items { get; set; } = [];

    /// <summary>큰 상자로 보낼지. 겉모습은 <see cref="Icon"/> 이 정하고 격자만 커진다.</summary>
    [JsonPropertyName("bigCrate")] public bool BigCrate { get; set; }
}

public record PresetListResponse
{
    [JsonPropertyName("presets")] public List<Preset> Presets { get; set; } = [];
}

/// <summary>
/// **`record` 로 두면 안 된다.** 한 번 `record` 로 뒀다가 다음을 겪었다.
///
/// `record` 는 값으로 같은지 본다 — 모든 필드를 견준다. 그런데 이 물건은 목록에 얹힌
/// **뒤에** <see cref="ItemIcon"/> 이 채워진다(그림은 뒤따라 온다). 그래서 WPF 가
/// "선택된 것" 으로 적어 둔 그 순간의 값과, 그림이 들어온 뒤의 값이 **서로 달라진다.**
///
/// 목록 내용을 갈아 끼울 때 `Selector` 는 적어 둔 값으로 옛 선택을 찾아 지우는데,
/// 값이 달라져 못 찾는다. 지워지지 않은 선택이 그대로 남고, 새로 고른 것이 뒤에
/// 덧붙는다. 단일 선택 목록인데 선택이 **여섯 개까지 쌓인 것**을 실측했다.
/// 그 상태에서 `SelectedItem` 은 **가장 오래된 것**을 돌려준다 — 무엇을 눌러도
/// 맨 처음에 골랐던 아이템이 품목에 들어가던 까닭이다.
///
/// 목록에 얹히는 물건은 **자기 자신으로 구별돼야** 한다. `class` 는 참조로 견주니
/// 안의 값이 아무리 바뀌어도 같은 것이 같은 것으로 남는다.
/// </summary>
public class FoundItem : System.ComponentModel.INotifyPropertyChanged
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

    /// <summary>안에 다른 품목을 담을 수 있는가(격자가 있는가).</summary>
    [JsonPropertyName("canContain")] public bool CanContain { get; set; }

    /// <summary>담을 수 있으면 그 칸 수. 보급 상자는 120칸이다.</summary>
    [JsonPropertyName("cells")] public int Cells { get; set; }

    /// <summary>분류 아이콘 파일 이름. 아이템마다의 아이콘은 없다(게임 번들 안).</summary>
    [JsonPropertyName("categoryIcon")] public string CategoryIcon { get; set; } = string.Empty;

    /// <summary>즐겨찾기 표시. 서버가 주는 값이 아니라 창이 채운다.</summary>
    [JsonIgnore] public string Star { get; set; } = string.Empty;

    private System.Windows.Media.ImageSource? _itemIcon;

    /// <summary>
    /// 아이템 그림. 서버가 주는 값이 아니라 창이 뒤늦게 채운다(받아 오는 데 시간이 걸린다).
    /// 그래서 알림이 필요하다 — 목록을 다시 그리지 않고 그림만 나타나게 한다.
    /// </summary>
    [JsonIgnore]
    public System.Windows.Media.ImageSource? ItemIcon
    {
        get => _itemIcon;
        set
        {
            _itemIcon = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ItemIcon)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public string Size => $"{Width}×{Height}"
                          + (MaxStack > 1 ? $" ·{MaxStack}" : string.Empty)
                          + (CanContain ? $" ·담을 수 있음 {Cells}칸" : string.Empty);

    public string Display => $"{Name}   [{Size}]";
}

/// <summary>분류 칸에 그대로 얹힌다. 그래서 `class` 다 — <see cref="FoundItem"/> 참고.</summary>
public class CategoryInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("icon")] public string Icon { get; set; } = string.Empty;

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

    /// <summary>품목에 붙은 것(방어판·삽입물·헬멧 부속)의 수. 격자 칸은 안 쓴다.</summary>
    [JsonPropertyName("attached")] public int Attached { get; set; }
    [JsonPropertyName("skipped")] public List<string> Skipped { get; set; } = [];

    /// <summary>담기는 했는데 적어 낸 대로는 아닌 것(묶음을 깎았다 등).</summary>
    [JsonPropertyName("adjusted")] public List<string> Adjusted { get; set; } = [];
    [JsonPropertyName("reason")] public string? Reason { get; set; }

    /// <summary>
    /// 마른 조립의 답. 참이면 예약된 것이 없고 **아무것도 떨어지지 않는다** —
    /// 창이 "예약했다" 로 잘못 말하지 않게 서버가 적어 돌려준다.
    /// </summary>
    [JsonPropertyName("dryRun")] public bool DryRun { get; set; }
}

/// <summary>지금 입고 있는 것을 뽑은 결과. 서버 `ExtractResponse` 와 짝이다(설계 §18).</summary>
public record ExtractResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }

    /// <summary>뽑은 것. **서버가 저장하지 않는다** — 창이 받아 보여 주고 사람이 이름을 붙인다.</summary>
    [JsonPropertyName("preset")] public Preset? Preset { get; set; }

    /// <summary>뺀 것과 그 이유(보안 컨테이너 등). 조용히 버리지 않는다.</summary>
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

    /// <summary>tpl 묶음으로 한 번에 찾는다. 프리셋을 불러올 때 쓴다.</summary>
    public Task<SearchResponse?> LookupAsync(List<string> tpls, CancellationToken ct) =>
        PostAsync<SearchResponse>("/airdrop-caller/items", new { tpls }, ct);

    public Task<CategoryListResponse?> ListCategoriesAsync(CancellationToken ct) =>
        GetAsync<CategoryListResponse>("/airdrop-caller/categories", ct);

    /// <summary>
    /// 품목의 칸 목록. **지금 고른 것을 함께 보낸다** — 겹친 칸은 윗칸에 무엇을
    /// 달았느냐에 따라 아랫칸이 달라져서, 한 칸을 바꿀 때마다 다시 물어봐야 한다.
    /// </summary>
    public Task<SlotListResponse?> SlotsAsync(
        string tpl, string? fill, Dictionary<string, string>? slots, CancellationToken ct) =>
        PostAsync<SlotListResponse>("/airdrop-caller/slots", new { tpl, fill, slots }, ct);

    /// <param name="dryRun">
    /// 참이면 **조립만** 한다 — 예약하지 않고 쿨다운도 쓰지 않으므로 게임이 꺼져 있어도
    /// 담기는지 미리 볼 수 있다. 답의 <c>DryRun</c> 이 참으로 돌아온다.
    /// </param>
    public Task<QueueResponse?> QueueAsync(
        string? presetId, string icon, List<ItemSpec> items, bool bigCrate, CancellationToken ct,
        bool dryRun = false) =>
        PostAsync<QueueResponse>("/airdrop-caller/queue", new { presetId, icon, items, bigCrate, dryRun }, ct);

    /// <summary>지금 입고 있는 것을 프리셋으로 뽑는다(설계 §18). 저장은 하지 않는다.</summary>
    public Task<ExtractResponse?> ExtractAsync(string? name, CancellationToken ct) =>
        PostAsync<ExtractResponse>("/airdrop-caller/extract", new { name }, ct);

    public Task<SimpleResponse?> CancelAsync(CancellationToken ct) =>
        PostAsync<SimpleResponse>("/airdrop-caller/cancel", null, ct);

    public void Dispose() => _http.Dispose();
}
