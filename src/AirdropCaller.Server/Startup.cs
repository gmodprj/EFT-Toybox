using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// 서버가 켜질 때 설정을 한 번 읽어 요약을 찍는다.
///
/// 하는 일은 **기록뿐**이다. 데이터베이스를 건드리지 않는다. 그런데도 두는 이유는,
/// 이것이 없으면 설정이 깨졌는지를 **레이드 중 첫 보급 요청 때**에야 알게 되기 때문이다.
/// 적어 둔 상자 id 와 품목 수가 로그에 보이면 "내가 고친 설정이 먹었나" 도 바로 답된다.
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]
public class AirdropCallerStartup(ISptLogger<AirdropCallerStartup> logger, ConfigStore store) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var config = store.Config;

        if (store.LoadError is not null)
        {
            // ConfigStore 가 이미 Error 로 적었다. 여기서는 결과만 말한다.
            logger.Warning("[AirdropCaller] 설정을 못 읽어 꺼진 채로 돈다. 보급은 전부 바닐라다.");
            return Task.CompletedTask;
        }

        if (!config.ModEnabled)
        {
            logger.Info("[AirdropCaller] 설정에서 꺼져 있다. 보급은 전부 바닐라다.");
            return Task.CompletedTask;
        }

        if (config.Drops.Count == 0)
        {
            logger.Info("[AirdropCaller] 정해 둔 상자가 없다. config.jsonc 의 Drops 를 채운다.");
            return Task.CompletedTask;
        }

        foreach (var (dropId, spec) in config.Drops)
        {
            var known = Enum.TryParse<AirdropTypeEnum>(spec.Icon, ignoreCase: true, out _);
            var count = spec.Items.Sum(i => Math.Max(1, i.Count));

            if (known)
            {
                logger.Info($"[AirdropCaller] 상자 {dropId} → {spec.Icon}, 품목 {spec.Items.Count}종 {count}개");
            }
            else
            {
                logger.Error($"[AirdropCaller] 상자 {dropId} 의 Icon '{spec.Icon}' 을 모른다 — "
                           + "Common / Supply / Medical / Weapon 중 하나여야 한다. 이 상자는 바닐라로 간다.");
            }
        }

        return Task.CompletedTask;
    }
}
