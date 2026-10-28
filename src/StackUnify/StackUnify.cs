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
/// 순서는 `Injectable.TypePriority` 로 정해진다. SPT 는 그 값으로 오름차순 정렬한다
/// (`DependencyInjectionHandler.InjectAll`).
///
/// <b>늦으면 늦을수록 좋은 것이 아니다.</b> 클라이언트에 보낼 아이템 목록이 굳는 지점이
/// 중간에 있어서, 그보다 뒤에 쓰면 DB 는 바뀌어도 클라이언트는 못 받는다. 실측으로 잡았다.
///
/// | 우선순위 | 모드가 본 값 | 모드가 쓴 값 | 클라이언트가 받은 값 |
/// | --- | --- | --- | --- |
/// | `OnLoadOrder.PostLoad` (1,000,000) | 150 | 137 | **137 (38개 중 37개)** |
/// | `int.MaxValue` (2,147,483,647) | 900 | 137 | 150 (하나도 안 옴) |
///
/// `int.MaxValue` 에서 본 값이 900 이라는 것이 열쇠다 — 목록이 굳은 뒤에도 DB 는 계속
/// 바뀌고 있다. 그래서 **굳기 전의 가장 늦은 자리**인 `PostLoad` 를 쓴다.
///
/// `Injectable` 의 생성자 기본값은 `typePriority = int.MaxValue` 다(인수 없이 붙이면 그 값).
/// 설치된 서버 모드의 선언 603개 중 304개가 거기 있다 — 즉 **그 모드들은 DB 를 고쳐도
/// 클라이언트 목록에는 닿지 않는 자리에서 돈다.**
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
            logger.Error($"[StackUnify] config.jsonc 를 읽지 못해 아무것도 바꾸지 않는다: {e.Message}");
            return Task.CompletedTask;
        }

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

            // 클래스 정의 노드는 아이템이 아니다. 여기에 값을 쓰면 그 아래 모든 아이템의
            // 상속이 틀어진다 — 실제로 Ammo 노드에 150 을 써서 게임 쪽이 깨진 적이 있다.
            if (string.Equals(item.Type, "Node", StringComparison.OrdinalIgnoreCase)) { skipped++; continue; }

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

            // 값이 없던 것에는 넣지 않는다. 없다는 것도 뜻이 있고, 없던 자리에 값을 만들면
            // 게임이 그 아이템을 다르게 다룰 수 있다.
            if (current is null) { skipped++; continue; }

            if (current == want) { already++; continue; }

            // 내가 정한 것과 다르면 그 자리를 누군가 건드려 둔 것이다.
            overridden++;
            {
                var key = props.Caliber ?? "(구경 없음)";
                overriddenByCaliber[key] = overriddenByCaliber.GetValueOrDefault(key) + 1;
            }

            if (config.Debug)
                logger.Info($"[StackUnify] {id} {props.Name ?? item.Name} : {current} -> {want}");

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
