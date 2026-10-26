using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace WebConfigBridge;

/// <summary>
/// 대상 모드의 설정 파일을 읽고 쓴다. 설정 편집기 등록과 모드 페이지가 함께 쓴다.
/// 남의 파일을 다루므로 실패해도 던지지 않고 null·false 로 알린다.
/// </summary>
[Injectable]
public class ConfigStore(ModHelper modHelper, ISptLogger<ConfigStore> logger)
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>All In Weapon: Unslotted 의 설정 파일. 없으면 null.</summary>
    public string? AllinWeaponUnslottedPath =>
        ResolveSiblingModFile("AllinWeaponUnslotted", Path.Combine("config", "config.jsonc"));

    public AllinWeaponUnslottedConfig? LoadAllinWeaponUnslotted()
    {
        var path = AllinWeaponUnslottedPath;
        if (path is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AllinWeaponUnslottedConfig>(File.ReadAllText(path), ReadOptions);
        }
        catch (Exception ex)
        {
            logger.Warning($"[WebConfigBridge] 설정을 읽지 못했다: {path} — {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 설정을 파일에 쓴다. 처음 쓸 때 원본을 <c>.orig</c> 로 한 번 남긴다 —
    /// 이 쓰기는 원본의 주석을 지우기 때문이다.
    /// </summary>
    public bool SaveAllinWeaponUnslotted(AllinWeaponUnslottedConfig config)
    {
        var path = AllinWeaponUnslottedPath;
        if (path is null)
        {
            return false;
        }

        try
        {
            var backup = path + ".orig";
            if (!File.Exists(backup))
            {
                File.Copy(path, backup);
                logger.Info($"[WebConfigBridge] 원본을 남겼다: {backup}");
            }

            File.WriteAllText(path, JsonSerializer.Serialize(config, WriteOptions));
            logger.Info($"[WebConfigBridge] 설정을 저장했다: {path} (적용은 다음 서버 시작부터)");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[WebConfigBridge] 설정을 저장하지 못했다: {path} — {ex.Message}");
            return false;
        }
    }

    /// <summary>같은 <c>user/mods</c> 안에 있는 다른 모드의 파일 경로. 없으면 null.</summary>
    private string? ResolveSiblingModFile(string modFolderName, string relativePath)
    {
        var ownFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var modsRoot = Path.GetDirectoryName(ownFolder);
        if (string.IsNullOrEmpty(modsRoot))
        {
            return null;
        }

        var full = Path.Combine(modsRoot, modFolderName, relativePath);
        return File.Exists(full) ? full : null;
    }
}
