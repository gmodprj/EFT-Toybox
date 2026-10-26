using System.Reflection;
using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;

namespace WebConfigBridge;

/// <summary>
/// 서버 웹 페이지(https://127.0.0.1:6969/)의 설정 편집기에 다른 서버 모드의 설정 파일을
/// 등록한다. 그 모드들은 자체 편집 수단이 없어 지금까지 파일을 직접 고쳐야 했다.
///
/// 서버가 <c>[Injectable]</c> 과 <see cref="IConfigEditorConfigProvider"/> 로 이 클래스를
/// 찾아낸다(같은 방식을 쓰는 설치된 전례: TraderNamer.ConfigEditorProvider).
/// </summary>
[Injectable]
public class ConfigEditorProvider(ModHelper modHelper) : IConfigEditorConfigProvider
{
    private static readonly JsonSerializerOptions JsoncOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        var registration = RegisterAllinWeaponUnslotted();
        if (registration is not null)
        {
            yield return registration;
        }
    }

    /// <summary>
    /// All In Weapon: Unslotted 의 설정을 등록한다. 그 모드가 없으면 아무것도 등록하지 않는다 —
    /// 이 모드는 남의 설정을 비추는 것이 전부이므로, 대상이 없으면 조용히 빠진다.
    /// </summary>
    private ConfigEditorConfigRegistration? RegisterAllinWeaponUnslotted()
    {
        var path = ResolveSiblingModFile("AllinWeaponUnslotted", Path.Combine("config", "config.jsonc"));
        if (path is null)
        {
            return null;
        }

        var config = ReadJsonc<AllinWeaponUnslottedConfig>(path);
        if (config is null)
        {
            return null;
        }

        return ConfigEditorConfigRegistration.Create(
            "allinweaponunslotted",
            "All In Weapon: Unslotted (저장 후 서버 재시작 필요)",
            config,
            path);
    }

    /// <summary>
    /// 같은 <c>user/mods</c> 안에 있는 다른 모드의 파일 경로. 없으면 null.
    /// </summary>
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

    /// <summary>주석이 있는 JSONC 를 읽는다. 읽지 못하면 null — 남의 파일이므로 던지지 않는다.</summary>
    private static T? ReadJsonc<T>(string path)
        where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsoncOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
