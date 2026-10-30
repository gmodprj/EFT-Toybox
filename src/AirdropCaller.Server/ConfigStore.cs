using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// `config.jsonc` 를 한 번만 읽어 들고 있는다.
///
/// 라우터가 직접 읽게 두면 **첫 전리품 요청 때** 처음 읽히고, 설정이 깨져 있어도
/// 레이드 한가운데에서야 안다. 여기서 읽고 <see cref="AirdropCallerStartup"/> 가
/// 서버 시작 때 요약을 찍으므로, 문제가 있으면 **켜자마자** 보인다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ConfigStore
{
    public Config Config { get; }

    /// <summary>설정을 못 읽었으면 이유. 읽었으면 null.</summary>
    public string? LoadError { get; }

    public ConfigStore(ISptLogger<ConfigStore> logger, ModHelper modHelper)
    {
        try
        {
            Config = modHelper.GetJsonDataFromFile<Config>(
                modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
                "config.jsonc");
        }
        catch (Exception e)
        {
            // 설정이 깨졌다고 서버를 죽일 이유가 없다. 아무것도 가로채지 않는다.
            LoadError = e.Message;
            Config = new Config { ModEnabled = false };
            logger.Error($"[AirdropCaller] config.jsonc 를 읽지 못해 아무것도 가로채지 않는다: {e.Message}");
        }
    }
}
