using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace StackUnify;

/// <summary>
/// 최대 스택 크기를 한 곳에서 정한다.
///
/// 설치본에는 같은 `StackMaxSize` 를 쓰는 모드가 여럿 있어(실측 13개) 어느 값이 남는지
/// 예측할 수 없었다. 이 모드는 <b>모든 모드 뒤</b>에 돌아 설정에 적힌 값으로 못박는다.
/// 중복 작업을 없애는 것이 아니라 <b>최종값의 주인을 하나로 만드는 것</b>이다.
///
/// 맨 뒤에 서는 방법은 `Injectable.TypePriority` 다. SPT 는 그 값으로 오름차순 정렬하고
/// (`DependencyInjectionHandler.InjectAll`), `OnLoadOrder.PostLoad` 가 가장 큰 단계다.
/// 설치된 서버 모드 113개의 선언은 전부 0 이므로 이 모드가 마지막이다.
///
/// 설정에 적지 않은 것은 건드리지 않는다. 기본 설정은 비어 있어 아무 일도 하지 않는다.
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]
public class StackUnify(
    ISptLogger<StackUnify> logger,
    TemplateTable templateTable,
    ModHelper modHelper
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var config = modHelper.GetJsonDataFromFile<Config>(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
            "config.jsonc"
        );

        if (!config.ModEnabled)
        {
            logger.Info("[StackUnify] 설정에서 꺼져 있어 아무것도 바꾸지 않았다.");
            return Task.CompletedTask;
        }

        if (config.StackByCaliber.Count == 0 && config.StackByItemId.Count == 0)
        {
            logger.Info("[StackUnify] 정해 둔 값이 없어 아무것도 바꾸지 않았다. "
                      + "config.jsonc 의 StackByCaliber / StackByItemId 를 채운다.");
            return Task.CompletedTask;
        }

        var items = templateTable.Items;
        if (items is null || items.Count == 0)
        {
            logger.Warning("[StackUnify] 아이템 표가 비어 있다. 건너뛴다.");
            return Task.CompletedTask;
        }

        // 구경 이름은 게임 쪽 표기가 일정하지 않을 수 있어 대소문자를 가리지 않는다.
        var byCaliber = new Dictionary<string, int>(config.StackByCaliber, StringComparer.OrdinalIgnoreCase);
        var byId = new Dictionary<string, int>(config.StackByItemId, StringComparer.OrdinalIgnoreCase);
        var excluded = new HashSet<string>(config.ExcludedItemIds, StringComparer.OrdinalIgnoreCase);

        int set = 0, already = 0, overridden = 0, skipped = 0;
        var overriddenByCaliber = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var item in items.Values)
        {
            var props = item.Properties;
            if (props is null) continue;

            var id = item.Id.ToString();
            if (excluded.Contains(id)) { skipped++; continue; }

            // 아이템 지정이 구경 규칙을 이긴다.
            int want;
            if (byId.TryGetValue(id, out var byIdValue))
            {
                want = byIdValue;
            }
            else
            {
                var caliber = props.Caliber;
                if (string.IsNullOrEmpty(caliber) || !byCaliber.TryGetValue(caliber, out var byCaliberValue)) continue;
                want = byCaliberValue;
            }

            if (want <= 0)
            {
                logger.Warning($"[StackUnify] {id} 에 정한 값이 {want} 다. 1 이상이어야 하므로 건너뛴다.");
                skipped++;
                continue;
            }

            var current = props.StackMaxSize;
            if (current == want) { already++; continue; }

            // 값이 이미 있고 내가 정한 것과 다르면, 그 자리를 누군가 건드려 둔 것이다.
            if (current is not null)
            {
                overridden++;
                var key = props.Caliber ?? "(구경 없음)";
                overriddenByCaliber[key] = overriddenByCaliber.GetValueOrDefault(key) + 1;
            }

            if (config.Debug)
                logger.Info($"[StackUnify] {id} {props.Name ?? item.Name} : {current?.ToString() ?? "없음"} -> {want}");

            if (!config.ReportOnly) props.StackMaxSize = want;
            set++;
        }

        var head = config.ReportOnly ? "[StackUnify] (ReportOnly) 바꿀 것" : "[StackUnify] 바꿈";
        logger.Info($"{head} {set}개, 이미 같음 {already}개, 건너뜀 {skipped}개 "
                  + $"(규칙: 구경 {byCaliber.Count}종 / 아이템 {byId.Count}개)");

        if (config.ReportOverridden && overridden > 0)
        {
            logger.Info($"[StackUnify] 그중 {overridden}개는 다른 모드가 이미 다른 값을 넣어 둔 자리였다. "
                      + "구경별: " + string.Join(", ",
                            overriddenByCaliber.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}")));
        }

        return Task.CompletedTask;
    }
}
