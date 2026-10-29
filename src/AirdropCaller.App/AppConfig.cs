using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AirdropCaller.App;

/// <summary>
/// exe 옆 `airdrop-caller.config.json`. `give-ui.config.json` 과 같은 자리·같은 모양이다.
/// </summary>
public class AppConfig
{
    [JsonPropertyName("sptUrl")]
    public string SptUrl { get; set; } = "https://127.0.0.1:6969";

    /// <summary>SPT 설치 폴더. 프로필에서 세션 id 를 읽는 데 쓴다.</summary>
    [JsonPropertyName("sptDir")]
    public string SptDir { get; set; } = @"E:\Spt";

    /// <summary>
    /// 프로필을 못 찾았을 때 손으로 적어 넣는 자리. 비워 두면 프로필 폴더에서 찾는다.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("statePollSeconds")]
    public double StatePollSeconds { get; set; } = 2;

    /// <summary>
    /// 즐겨찾기한 아이템 tpl. 창의 분류 목록 맨 위에 "★ 즐겨찾기" 로 모아 보여 준다.
    /// 서버에 두지 않는다 — 이것은 **이 창을 쓰는 사람의 취향**이지 프로필 자료가 아니다.
    /// </summary>
    [JsonPropertyName("favorites")]
    public List<string> Favorites { get; set; } = [];

    /// <summary>
    /// 아이템 아이콘을 tarkov.dev 에서 받아 올지. **밖으로 나가는 통신이다** —
    /// 아이템 id 가 그쪽으로 가고 그림을 받아 온다. 끄면 분류 아이콘만 쓴다.
    /// 받은 것은 <see cref="IconCacheFolder"/> 에 쌓여 다시 받지 않는다.
    /// </summary>
    [JsonPropertyName("downloadItemIcons")]
    public bool DownloadItemIcons { get; set; } = true;

    /// <summary>비우면 exe 옆 `icon-cache` 폴더.</summary>
    [JsonPropertyName("iconCacheFolder")]
    public string IconCacheFolder { get; set; } = string.Empty;

    public string ResolveIconCacheFolder()
    {
        if (!string.IsNullOrWhiteSpace(IconCacheFolder))
        {
            return IconCacheFolder;
        }

        var dir = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        return Path.Combine(dir, "icon-cache");
    }

    /// <summary>
    /// 기본은 **꺼짐**이다. 켜 두면 다른 창을 이 창 위에 올릴 수 없어 가린 채로
    /// 일하게 된다. 레이드 중에만 켜고 싶은 것이므로 창의 체크상자로 켠다.
    /// </summary>
    [JsonPropertyName("alwaysOnTop")]
    public bool AlwaysOnTop { get; set; }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string PathNextToExe()
    {
        var dir = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        return Path.Combine(dir, "airdrop-caller.config.json");
    }

    /// <summary>창에서 바꾼 것(항상 위 등)을 그 자리에서 적어 둔다. 실패해도 조용히 넘어간다.</summary>
    public void Save()
    {
        try
        {
            File.WriteAllText(PathNextToExe(), JsonSerializer.Serialize(this, Options));
        }
        catch
        {
            // 설정을 못 적는다고 창을 멈출 이유가 없다. 다음에 다시 고르면 된다.
        }
    }

    public static AppConfig LoadOrCreate(out string? error)
    {
        error = null;
        var path = PathNextToExe();
        try
        {
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path), Options) ?? new AppConfig();
            }

            var fresh = new AppConfig();
            File.WriteAllText(path, JsonSerializer.Serialize(fresh, Options));
            return fresh;
        }
        catch (Exception e)
        {
            // 설정을 못 읽었다고 창을 안 띄울 이유는 없다. 기본값으로 뜨고 이유를 보여 준다.
            error = $"설정을 못 읽어 기본값으로 돈다: {e.Message}";
            return new AppConfig();
        }
    }

    /// <summary>
    /// 세션 id 를 찾는다. SPT 는 **프로필 파일 이름이 곧 세션 id** 다
    /// (`user\profiles\&lt;id&gt;.json`). 게임 클라이언트도 그 값을
    /// `Cookie: PHPSESSID=&lt;id&gt;` 로 보낸다.
    /// </summary>
    public string? ResolveSessionId(out string how)
    {
        if (!string.IsNullOrWhiteSpace(SessionId))
        {
            how = "설정에 적힌 값";
            return SessionId.Trim();
        }

        var dir = Path.Combine(SptDir, "SPT_Runtime", "user", "profiles");
        how = dir;

        try
        {
            var files = new DirectoryInfo(dir).GetFiles("*.json");
            if (files.Length == 0)
            {
                return null;
            }

            // 여러 개면 가장 최근에 쓰인 것. 지금 쓰는 프로필일 가능성이 높다.
            Array.Sort(files, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
            how = $"{files[0].Name} (프로필 {files.Length}개 중 가장 최근)";
            return Path.GetFileNameWithoutExtension(files[0].Name);
        }
        catch (Exception e)
        {
            how = $"{dir} — {e.Message}";
            return null;
        }
    }
}
