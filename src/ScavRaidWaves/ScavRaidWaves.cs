using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace ScavRaidWaves;

/// <summary>
/// 스캐브로 들어간 레이드에 PMC 가 나오게 한다.
///
/// <b>왜 안 나오는가.</b> PMC 는 웨이브로 들어온다(`pmc.json` 의 `customPmcWaves`).
/// 그 시각표는 레이드 시작 30초부터 1800초(30분) 사이에 몰려 있다 — 커스텀을 예로 들면
/// `30, 30, 30, 30, 360, 360, 720, 720, 1080, 1080, 1440, 1440, 1800, 1800`.
///
/// 그런데 스캐브 레이드는 `location.json` 의 `scavRaidTimeSettings` 에 따라
/// <b>이미 진행 중인 레이드에 끼어드는 것</b>으로 처리된다. 맵마다
/// `reducedChancePercent`(커스텀·인터체인지·연구소는 100) 확률로 시간이 40~70% 깎이고,
/// `adjustWaves` 가 켜져 있어 봇 웨이브도 같이 조정된다. 서버가 몇 초 지점에서 시작한
/// 것으로 칠지는 `RaidChanges.SimulatedRaidStartSeconds` 에 담기고,
/// `RaidTimeAdjustmentService.MakeAdjustmentsToMap` 이 그 값으로 웨이브를 손본다.
///
/// PMC 웨이브가 전부 앞쪽에 몰려 있으니, 뒤늦게 끼어든 스캐브는 그것들을 통째로 놓친다.
///
/// <b>이 모드가 하는 일.</b> 그 두 값만 설정대로 덮어쓴다. 기본 설정은
/// 모든 맵의 `ReducedChancePercent` 를 0 으로 둔다 — 스캐브 레이드도 시간을 깎지 않으므로
/// PMC 웨이브가 전부 산다. 대신 스캐브 레이드가 정규 레이드와 같은 길이가 된다.
/// 짧은 레이드를 유지하고 싶으면 `ReducedChancePercent` 를 null 로 두고
/// `AdjustWaves` 를 false 로 바꾼다.
///
/// <b>적용 지점.</b> `LocationConfig` 는 서비스에 그대로 주입되는 단일 인스턴스다
/// (`RaidTimeAdjustmentService` 의 생성자가 받는 것과 같은 것). 여기서 고치면 그 서비스가
/// 레이드마다 읽을 때 반영된다. 다른 모드가 같은 값을 건드릴 수 있으므로
/// <see cref="OnLoadOrder.PostLoad"/> 에 둬서 늦게 쓴다.
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]
public class ScavRaidWaves(
    ISptLogger<ScavRaidWaves> logger,
    LocationConfig locationConfig,
    ModHelper modHelper
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        Config config;
        try
        {
            config = modHelper.GetJsonDataFromFile<Config>(
                modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
                "config.jsonc"
            );
        }
        catch (Exception e)
        {
            // 설정이 깨졌다고 서버를 죽일 이유가 없다. 아무것도 바꾸지 않고 넘어간다.
            logger.Error($"[ScavRaidWaves] config.jsonc 를 읽지 못해 아무것도 바꾸지 않는다: {e.Message}");
            return Task.CompletedTask;
        }

        if (!config.ModEnabled)
        {
            logger.Info("[ScavRaidWaves] 설정에서 꺼져 있어 아무것도 바꾸지 않았다.");
            return Task.CompletedTask;
        }

        var maps = locationConfig.ScavRaidTimeSettings?.Maps;
        if (maps is null || maps.Count == 0)
        {
            logger.Warning("[ScavRaidWaves] scavRaidTimeSettings 에 맵이 없다. 건너뛴다.");
            return Task.CompletedTask;
        }

        var changedChance = 0;
        var changedWaves = 0;
        var skipped = 0;

        foreach (var (mapId, settings) in maps)
        {
            if (settings is null)
            {
                skipped++;
                continue;
            }

            config.Maps.TryGetValue(mapId, out var mapOverride);

            var chance = mapOverride?.ReducedChancePercent ?? config.Default.ReducedChancePercent;
            var adjust = mapOverride?.AdjustWaves ?? config.Default.AdjustWaves;

            if (chance.HasValue && !NearlyEqual(settings.ReducedChancePercent, chance.Value))
            {
                if (config.Debug || config.ReportOnly)
                {
                    logger.Info($"[ScavRaidWaves] {mapId}: 시간을 깎을 확률 "
                              + $"{settings.ReducedChancePercent} -> {chance.Value}");
                }

                if (!config.ReportOnly)
                {
                    settings.ReducedChancePercent = chance.Value;
                }

                changedChance++;
            }

            if (adjust.HasValue && settings.AdjustWaves != adjust.Value)
            {
                if (config.Debug || config.ReportOnly)
                {
                    logger.Info($"[ScavRaidWaves] {mapId}: 웨이브 조정 "
                              + $"{settings.AdjustWaves} -> {adjust.Value}");
                }

                if (!config.ReportOnly)
                {
                    settings.AdjustWaves = adjust.Value;
                }

                changedWaves++;
            }
        }

        var what = config.ReportOnly ? "바꿀 것" : "바꾼 것";
        logger.Info($"[ScavRaidWaves] 맵 {maps.Count}개 중 {what} — "
                  + $"시간 확률 {changedChance}개, 웨이브 조정 {changedWaves}개"
                  + (skipped > 0 ? $" (설정이 비어 건너뛴 맵 {skipped}개)" : string.Empty));

        if (changedChance == 0 && changedWaves == 0)
        {
            logger.Info("[ScavRaidWaves] 바꿀 것이 없었다. config.jsonc 의 Default / Maps 를 확인한다.");
        }

        return Task.CompletedTask;
    }

    /// <summary>double 비교. 설정에서 읽은 값과 서버 값이 같은지만 보면 되므로 이 정도로 충분하다.</summary>
    private static bool NearlyEqual(double a, double b) => Math.Abs(a - b) < 0.000001;
}
