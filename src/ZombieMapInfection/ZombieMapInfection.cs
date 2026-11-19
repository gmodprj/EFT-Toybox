using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace ZombieMapInfection;

/// <summary>
/// 할로윈 좀비 이벤트의 맵별 감염률을 설정대로 바꾼다 — 기본은 세관·인터체인지를 50 으로.
///
/// <b>왜 필요한가.</b> SPT 에 든 2024 할로윈 좀비는 <c>seasonalevents.json</c> 의
/// <c>zombieSettings.mapInfectionAmount</c> 가 0 보다 큰 맵에만 나온다. 기본값에서 세관(bigmap)과
/// 인터체인지는 0 이다. PPES 는 이 값을 그대로 쓰고(2026 발병 부분만 따로 덮는다) 맵별로 정하는
/// 설정이 없다. SPT 파일을 직접 고치면 SPT 를 다시 깔 때 날아가므로 메모리에서 고친다.
///
/// <b>적용 지점.</b> 순서가 전부다(역컴파일 실측, SPT 4.1.6 · PPES 1.7.7):
/// <list type="number">
/// <item>199998 — 여기. <c>SeasonalEventConfig</c>(주입되는 단일 인스턴스)의 값을 고친다.</item>
/// <item>199999 — PPES <c>SeasonalApplier</c> 가 기본값을 읽어 간다(웹 미리보기의 감염률도 여기서 온다).</item>
/// <item>200000 — SPT <c>GameCallbacks</c> → <c>PostDbLoadService.PerformPostDbLoadActions</c> →
///   <c>SeasonalEventService.EnableSeasonalEvents</c> → <c>ConfigureZombies</c> 가 감염률 0 초과 맵에
///   <c>halloweenzombies</c> 보스(감염자)를 넣고 맵의 <c>InfectionPercentage</c> 를 정한다.</item>
/// </list>
/// 그래서 <see cref="OnLoadOrder.PostLoad"/> 처럼 늦게 두면 <b>이미 지나가서 아무 효과가 없다.</b>
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.GameCallbacks - 2)]
public class ZombieMapInfection(
    ISptLogger<ZombieMapInfection> logger,
    SeasonalEventConfig seasonalConfig,
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
            logger.Error($"[ZombieMapInfection] config.jsonc 를 읽지 못해 아무것도 바꾸지 않는다: {e.Message}");
            return Task.CompletedTask;
        }

        if (!config.ModEnabled || config.Infection.Count == 0)
        {
            logger.Info("[ZombieMapInfection] 꺼져 있거나 바꿀 맵이 없어 아무것도 바꾸지 않았다.");
            return Task.CompletedTask;
        }

        var halloween = (seasonalConfig.Events ?? [])
            .Where(e => e.Type == SeasonalEventType.Halloween && e.Settings?.ZombieSettings?.MapInfectionAmount is not null)
            .ToList();
        if (halloween.Count == 0)
        {
            logger.Warning("[ZombieMapInfection] seasonalevents.json 에 할로윈 zombieSettings 가 없다. 건너뛴다.");
            return Task.CompletedTask;
        }

        foreach (var ev in halloween)
        {
            var amounts = ev.Settings!.ZombieSettings!.MapInfectionAmount!;
            var changes = new List<string>();
            foreach (var (wanted, value) in config.Infection)
            {
                // 기존 열쇠의 철자(bigmap·Interchange 처럼 섞여 있다)를 그대로 살린다.
                var key = amounts.Keys.FirstOrDefault(k => string.Equals(k, wanted, StringComparison.OrdinalIgnoreCase));
                if (key is null)
                {
                    logger.Warning($"[ZombieMapInfection] '{wanted}' 는 감염률 목록에 없는 맵 열쇠다. 건너뛴다 "
                                 + $"(있는 열쇠: {string.Join(", ", amounts.Keys)}).");
                    continue;
                }

                var clamped = Math.Clamp(value, 0, 100);
                if (Math.Abs(amounts[key] - clamped) < 0.000001)
                {
                    continue;
                }

                changes.Add($"{key} {amounts[key]} -> {clamped}");
                amounts[key] = clamped;
            }

            logger.Info(changes.Count == 0
                ? $"[ZombieMapInfection] {ev.Name}: 바꿀 것이 없었다(이미 설정과 같다)."
                : $"[ZombieMapInfection] {ev.Name} 감염률: {string.Join(", ", changes)}");
        }

        return Task.CompletedTask;
    }
}
