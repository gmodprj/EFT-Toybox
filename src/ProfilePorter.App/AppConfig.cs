using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProfilePorter.App;

/// <summary>
/// exe 옆 `profile-porter.config.json`. AirdropCaller 의 설정과 같은 자리·같은 모양이다.
/// </summary>
public class AppConfig
{
    [JsonPropertyName("sptUrl")]
    public string SptUrl { get; set; } = "https://127.0.0.1:6969";

    /// <summary>SPT 설치 폴더. 프로필에서 세션 id 를 읽는 데 쓴다.</summary>
    [JsonPropertyName("sptDir")]
    public string SptDir { get; set; } = @"E:\Spt";

    /// <summary>비워 두면 프로필 폴더에서 가장 최근 것을 찾는다.</summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 아이템 아이콘을 tarkov.dev 에서 받아 올지. **밖으로 나가는 통신이다** — 아이템 id 가
    /// 그쪽으로 가고 그림을 받아 온다. 끄면 격자에 이름만 그린다.
    /// </summary>
    [JsonPropertyName("downloadItemIcons")]
    public bool DownloadItemIcons { get; set; } = true;

    /// <summary>
    /// 받은 아이콘을 쌓아 둘 곳. 비우면 `&lt;sptDir&gt;\icon-cache` — **AirdropCaller 와 같은
    /// 자리**라 이미 받아 둔 것을 다시 받지 않는다.
    /// </summary>
    [JsonPropertyName("iconCacheFolder")]
    public string IconCacheFolder { get; set; } = string.Empty;

    public string ResolveIconCacheFolder() =>
        string.IsNullOrWhiteSpace(IconCacheFolder)
            ? System.IO.Path.Combine(SptDir, "icon-cache")
            : IconCacheFolder;

    private const string FileName = "profile-porter.config.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Path => System.IO.Path.Combine(AppContext.BaseDirectory, FileName);

    public static AppConfig Load(out string? error)
    {
        error = null;
        try
        {
            if (!File.Exists(Path))
            {
                var fresh = new AppConfig();
                File.WriteAllText(Path, JsonSerializer.Serialize(fresh, Options));
                return fresh;
            }

            var loaded = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(Path), Options) ?? new AppConfig();

            // **읽은 뒤 다시 적는다.** 설정 항목이 늘어도 옛 파일에는 그 줄이 없어, 사람이
            // 무엇을 끌 수 있는지 알 길이 없다. 고쳐 둔 값은 그대로 실려 돌아간다.
            File.WriteAllText(Path, JsonSerializer.Serialize(loaded, Options));
            return loaded;
        }
        catch (Exception e)
        {
            // 설정이 깨졌다고 창을 못 띄울 이유는 없다. 기본값으로 열고 사유를 상태줄에 적는다.
            error = e.Message;
            return new AppConfig();
        }
    }

    /// <summary>
    /// 세션 id 를 찾는다. SPT 는 **프로필 파일 이름이 곧 세션 id** 다
    /// (`user\profiles\&lt;id&gt;.json`). 게임 클라이언트도 그 값을 쿠키로 보낸다.
    ///
    /// 어느 프로필을 쓰든 라우트는 같은 답을 준다 — 이 창은 세션으로 **무엇을 할지 고르지
    /// 않는다**. 보내는 쪽·받는 쪽은 화면에서 따로 고른다.
    /// </summary>
    public string? ResolveSessionId(out string how)
    {
        if (!string.IsNullOrWhiteSpace(SessionId))
        {
            how = "설정에 적힌 값";
            return SessionId.Trim();
        }

        var dir = System.IO.Path.Combine(SptDir, "SPT_Runtime", "user", "profiles");
        how = dir;

        try
        {
            var files = new DirectoryInfo(dir).GetFiles("*.json");
            if (files.Length == 0)
            {
                return null;
            }

            Array.Sort(files, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
            how = $"{files[0].Name} (프로필 {files.Length}개 중 가장 최근)";
            return System.IO.Path.GetFileNameWithoutExtension(files[0].Name);
        }
        catch (Exception e)
        {
            how = $"{dir} — {e.Message}";
            return null;
        }
    }
}
