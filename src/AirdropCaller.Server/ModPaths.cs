using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace AirdropCaller.Server;

/// <summary>
/// 이 모드 폴더의 절대 경로. `ModHelper` 를 여기저기서 다시 부르지 않으려고 한 번만 구한다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ModPaths(ModHelper modHelper)
{
    public string ModFolder { get; } =
        modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
}
