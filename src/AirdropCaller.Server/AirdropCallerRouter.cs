using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// 바닐라의 보급 전리품 라우트(`POST /client/airdrop/loot`)에 **뒤에서** 끼어든다.
///
/// `HttpRouter.HandleRouteAsync` 는 같은 URL 을 잡는 라우터에서 **멈추지 않는다.**
/// 전부 차례로 돌리면서 앞선 결과를 다음 라우터에 `output` 으로 넘기고, 마지막 결과가
/// 응답이 된다. 그래서 바닐라보다 **뒤에** 등록되면 그 답을 받아 보고 바꿀 수 있다.
///
/// 순서는 `Injectable.TypePriority` 오름차순이다(`DependencyInjectionHandler`).
/// 바닐라 `LocationStaticRouter` 는 400,000 이고 `Injectable` 의 기본값은
/// `int.MaxValue` 라, 인수 없이 붙이는 것만으로 우리가 뒤에 선다.
///
/// 우리 id 가 아니면 `output` 을 **그대로** 돌려준다 — 바닐라 보급기와 플레어 보급은
/// 건드리지 않는다.
/// </summary>
/// <remarks>
/// Singleton 이다. 기본값(Transient)이면 전리품 요청마다 새로 만들어져 `config.jsonc` 를
/// 다시 읽는다. 한 번만 읽는 대신 **설정을 고치면 서버를 다시 켜야 한다** — 이 저장소의
/// 다른 서버 모드와 같은 규칙이다.
/// 등록 순서는 수명과 무관하게 `TypePriority` 로 정해지므로 바닐라 뒤에 서는 것은 그대로다.
/// </remarks>
[Injectable(InjectionType.Singleton)]
public class AirdropCallerRouter : StaticRouter
{
    public AirdropCallerRouter(
        JsonUtil jsonUtil,
        ISptLogger<AirdropCallerRouter> logger,
        ModHelper modHelper,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder)
        : base(jsonUtil, BuildRoutes(logger, modHelper, httpResponseUtil, crateBuilder))
    {
    }

    private static List<RouteAction> BuildRoutes(
        ISptLogger<AirdropCallerRouter> logger,
        ModHelper modHelper,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder)
    {
        var config = LoadConfig(logger, modHelper);

        return
        [
            new RouteAction<GetAirdropLootRequest>(
                "/client/airdrop/loot",
                (url, info, sessionId, output, ct) =>
                    ValueTask.FromResult(Handle(logger, httpResponseUtil, crateBuilder, config, info, output))),
        ];
    }

    private static string Handle(
        ISptLogger<AirdropCallerRouter> logger,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder,
        Config config,
        GetAirdropLootRequest info,
        string? vanillaOutput)
    {
        // 우리 것이 아니면 바닐라가 만든 답을 그대로 흘려보낸다.
        var passthrough = vanillaOutput ?? string.Empty;

        if (!config.ModEnabled)
        {
            return passthrough;
        }

        var containerId = info.ContainerId.ToString();
        if (string.IsNullOrEmpty(containerId) || !config.Drops.TryGetValue(containerId, out var spec))
        {
            if (config.Debug)
            {
                logger.Info($"[AirdropCaller] {containerId} 는 우리 것이 아니다 — 바닐라 답을 그대로 보낸다.");
            }

            return passthrough;
        }

        var built = crateBuilder.Build(containerId, spec);
        if (built is null)
        {
            // 조립이 안 되면 상자를 비우느니 바닐라 무작위가 낫다.
            logger.Error($"[AirdropCaller] {containerId} 조립에 실패해 바닐라 답을 그대로 보낸다.");
            return passthrough;
        }

        logger.Info($"[AirdropCaller] {containerId} → {spec.Icon} 상자에 {built.Placed}개를 담았다"
                    + (built.Skipped.Count > 0 ? $" (못 담은 것 {built.Skipped.Count}개: {string.Join(", ", built.Skipped)})" : "."));

        return httpResponseUtil.GetBody(built.Response);
    }

    private static Config LoadConfig(ISptLogger<AirdropCallerRouter> logger, ModHelper modHelper)
    {
        try
        {
            return modHelper.GetJsonDataFromFile<Config>(
                modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
                "config.jsonc");
        }
        catch (Exception e)
        {
            // 설정이 깨졌다고 서버를 죽일 이유가 없다. 아무것도 가로채지 않는다.
            logger.Error($"[AirdropCaller] config.jsonc 를 읽지 못해 아무것도 가로채지 않는다: {e.Message}");
            return new Config { ModEnabled = false };
        }
    }
}
