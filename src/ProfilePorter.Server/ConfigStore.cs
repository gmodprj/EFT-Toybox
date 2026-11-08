using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Utils;

namespace ProfilePorter.Server;

/// <summary>
/// `config.jsonc` 를 한 번만 읽어 들고 있는다. AirdropCaller 의 같은 이름 클래스와 같은 모양이다 —
/// 설정이 깨졌으면 **서버를 켜자마자** 알게 하려는 것이고, 깨졌다고 서버를 죽이지는 않는다.
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
            // 설정이 깨졌으면 **꺼진 채로** 돈다. 프로필에 쓰는 모드가 추측으로 켜져 있으면 안 된다.
            LoadError = e.Message;
            Config = new Config { ModEnabled = false };
            logger.Error($"[ProfilePorter] config.jsonc 를 읽지 못해 꺼진 채로 돈다: {e.Message}");
        }
    }
}
