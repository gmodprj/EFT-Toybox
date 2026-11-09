using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProfilePorter.App;

public sealed record ProfileRow
{
    [JsonPropertyName("profileId")] public string ProfileId { get; init; } = string.Empty;
    [JsonPropertyName("nickname")] public string Nickname { get; init; } = string.Empty;
    [JsonPropertyName("level")] public int Level { get; init; }
    [JsonPropertyName("stashFloorCount")] public int StashFloorCount { get; init; }

    public override string ToString() => $"{Nickname}  (Lv{Level}, 바닥 {StashFloorCount})";
}

public sealed record StashRow
{
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("tpl")] public string Tpl { get; init; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("stack")] public int Stack { get; init; }
    [JsonPropertyName("contained")] public int Contained { get; init; }
    [JsonPropertyName("x")] public int X { get; init; }
    [JsonPropertyName("y")] public int Y { get; init; }
    [JsonPropertyName("w")] public int W { get; init; }
    [JsonPropertyName("h")] public int H { get; init; }
    [JsonPropertyName("rotated")] public bool Rotated { get; init; }

    /// <summary>격자 칸 안에 적는 글. 좁으므로 이름과 묶음 수만 둔다.</summary>
    public string GridLabel => Stack > 1 ? $"{Name}{Environment.NewLine}×{Stack}" : Name;

    /// <summary>목록 한 줄. **안에 든 것의 수를 보여 준다** — 그것이 실제로 옮겨지는 양이다.</summary>
    public string Display =>
        (Stack > 1 ? $"{Name} ×{Stack}" : Name) + (Contained > 0 ? $"   (안에 {Contained}개)" : string.Empty);
}

public sealed record PorterFileRow
{
    [JsonPropertyName("fileName")] public string FileName { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; init; } = string.Empty;
    [JsonPropertyName("createdUtc")] public string CreatedUtc { get; init; } = string.Empty;
    [JsonPropertyName("fromNickname")] public string FromNickname { get; init; } = string.Empty;
    [JsonPropertyName("note")] public string? Note { get; init; }
    [JsonPropertyName("rootIds")] public List<string> RootIds { get; init; } = [];
    [JsonPropertyName("items")] public List<JsonElement> Items { get; init; } = [];

    /// <summary>
    /// 서버가 그 파일을 **열 수 있었는가**. 못 읽는 파일도 목록에 오른다 — 그래야 지울 수 있다.
    /// </summary>
    [JsonPropertyName("readable")] public bool Readable { get; init; } = true;

    public string Display =>
        Readable
            ? $"{FileName}   [{(Kind == "loadout" ? "한 벌" : "스태시")}]  {FromNickname} 에서 {Items.Count}개"
              + (string.IsNullOrWhiteSpace(Note) ? string.Empty : $"   — {Note}")
            : $"{FileName}   (못 읽음 — 지우기만 됩니다)";
}

public sealed record ProfilesResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; init; }
    [JsonPropertyName("profiles")] public List<ProfileRow> Profiles { get; init; } = [];
}

/// <summary>스태시 격자의 칸 수. **서버가 DB 에서 읽어 준다** — 창이 지어내지 않는다.</summary>
public sealed record StashGridSize
{
    [JsonPropertyName("cellsH")] public int CellsH { get; init; }
    [JsonPropertyName("cellsV")] public int CellsV { get; init; }
}

public sealed record StashResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; init; }
    [JsonPropertyName("reason")] public string? Reason { get; init; }
    [JsonPropertyName("grid")] public StashGridSize? Grid { get; init; }
    [JsonPropertyName("items")] public List<StashRow> Items { get; init; } = [];
}

public sealed record FilesResponse
{
    [JsonPropertyName("ok")] public bool Ok { get; init; }
    [JsonPropertyName("folder")] public string? Folder { get; init; }
    [JsonPropertyName("files")] public List<PorterFileRow> Files { get; init; } = [];
}

public sealed record PorterResultRow
{
    [JsonPropertyName("ok")] public bool Ok { get; init; }
    [JsonPropertyName("reason")] public string? Reason { get; init; }
    [JsonPropertyName("fileName")] public string? FileName { get; init; }
    [JsonPropertyName("count")] public int Count { get; init; }
    [JsonPropertyName("skipped")] public List<string> Skipped { get; init; } = [];
}

/// <summary>
/// SPT 서버의 `/profile-porter/*` 를 부른다.
///
/// 왕복은 AirdropCaller 의 `ServerClient` 와 같다 — **https + 자체 서명 인증서**,
/// `Cookie: PHPSESSID=&lt;세션 id&gt;`, 그리고 **요청·응답 양쪽 zlib**. SPT 는 응답을
/// `Content-Encoding` 없이 압축해 보내므로 `HttpClient` 의 자동 해제가 걸리지 않는다.
///
/// 보내는 열쇠 이름은 **camelCase** 다. 서버 쪽 요청 레코드에 `[JsonPropertyName]` 이
/// 달려 있어야 받아진다 — 2026-11-08 기동 확인에서 그것이 빠져 "그 프로필을 찾지
/// 못했습니다" 라는 엉뚱한 사유로 거절당했다.
/// </summary>
public sealed class ServerClient : IDisposable
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ServerClient(string baseUrl, string sessionId)
    {
        var uri = new Uri(baseUrl);
        var handler = new HttpClientHandler();
        if (uri.IsLoopback)
        {
            // 127.0.0.1 에 한해서만 자체 서명 인증서를 받아들인다.
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        _http = new HttpClient(handler)
        {
            BaseAddress = uri,
            Timeout = TimeSpan.FromSeconds(30),
        };
        _http.DefaultRequestHeaders.Add("Cookie", "PHPSESSID=" + sessionId);
    }

    public Task<ProfilesResponse?> ProfilesAsync(CancellationToken ct) =>
        PostAsync<ProfilesResponse>("/profile-porter/profiles", null, ct);

    public Task<StashResponse?> StashAsync(string profileId, CancellationToken ct) =>
        PostAsync<StashResponse>("/profile-porter/stash", new { profileId }, ct);

    public Task<FilesResponse?> FilesAsync(CancellationToken ct) =>
        PostAsync<FilesResponse>("/profile-porter/files", null, ct);

    /// <summary>이름을 비우면 서버가 **프로필명-종류-날짜_시각**으로 짓는다.</summary>
    public Task<PorterResultRow?> ExportStashAsync(
        string profileId, IReadOnlyList<string> itemIds, string? name, string? note,
        bool overwrite, bool rename, CancellationToken ct) =>
        PostAsync<PorterResultRow>(
            "/profile-porter/export",
            new { profileId, kind = "stash", itemIds, name, note, overwrite, rename }, ct);

    public Task<PorterResultRow?> ExportLoadoutAsync(
        string profileId, string? name, string? note,
        bool overwrite, bool rename, CancellationToken ct) =>
        PostAsync<PorterResultRow>(
            "/profile-porter/export",
            new { profileId, kind = "loadout", name, note, overwrite, rename }, ct);

    /// <summary>같은 이름이라 거절당했는지. 서버가 이 말로 알려 준다.</summary>
    public const string NameTakenReason = "같은 이름의 파일이 이미 있습니다.";

    /// <summary>내보낸 파일 하나를 지운다. **창이 사람에게 묻고 나서** 부른다.</summary>
    public Task<PorterResultRow?> DeleteFileAsync(string fileName, CancellationToken ct) =>
        PostAsync<PorterResultRow>("/profile-porter/file-delete", new { fileName }, ct);

    public Task<PorterResultRow?> ImportAsync(string profileId, string fileName, CancellationToken ct) =>
        PostAsync<PorterResultRow>("/profile-porter/import", new { profileId, fileName }, ct);

    private async Task<T?> PostAsync<T>(string path, object? body, CancellationToken ct)
    {
        using var content = Compress(body);
        using var response = await _http.PostAsync(path, content, ct);
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

    private static ByteArrayContent Compress(object? body)
    {
        var json = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body ?? new { }));
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(json, 0, json.Length);
        }

        var content = new ByteArrayContent(output.ToArray());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        return content;
    }

    /// <summary>zlib 머리글: 첫 바이트가 0x78 이고 두 바이트를 묶은 값이 31 로 나누어떨어진다.</summary>
    private static bool LooksLikeZlib(byte[] bytes) =>
        bytes.Length >= 2 && bytes[0] == 0x78 && ((bytes[0] << 8) | bytes[1]) % 31 == 0;

    public void Dispose() => _http.Dispose();
}
