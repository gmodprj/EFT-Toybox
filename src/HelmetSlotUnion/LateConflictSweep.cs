using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace HelmetSlotUnion;

/// <summary>
/// 같은 쓸기를 한 번 더, 맨 끝에서 한다.
///
/// `PostLoad` 에서 ConflictingItems 를 전부 비웠는데도 클라이언트가 받은 목록에는 884건이
/// 남아 있었다(ECOTI 의 서멀 라이저 390, 안면 보호구 369 …). 그 뒤에 다시 쓰는 모드가 있다는 뜻이다
/// — `Injectable` 에 우선순위를 적지 않으면 기본값이 `int.MaxValue` 라 PostLoad 보다 늦게 돈다.
///
/// 그래서 `int.MaxValue` 자리에 한 번 더 둔다. 이 자리의 쓰기가 클라이언트에 닿는지는 StackUnify
/// 때 "닿지 않는다" 로 재어 둔 적이 있으므로(스택 크기 기준), 효과는 실측으로 확인한다.
/// 닿지 않더라도 앞의 PostLoad 패스가 한 일은 그대로 남는다.
/// </summary>
[Injectable(InjectionType.Singleton, int.MaxValue)]
public class LateConflictSweep(
    ISptLogger<LateConflictSweep> logger,
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

        if (!config.ModEnabled || !config.RemoveRemainingConflicts) return Task.CompletedTask;

        var items = templateTable.Items;
        if (items is null || items.Count == 0) return Task.CompletedTask;

        var keepCategories = new HashSet<MongoId>(
            config.WeaponConflictExcludedCategoryIds.Select(id => new MongoId(id))
        );

        var touched = 0;
        var removed = 0;
        var kept = 0;
        foreach (var (id, item) in items)
        {
            if (item.Properties?.ConflictingItems is not { Count: > 0 } conflicts) continue;

            if (keepCategories.Contains(item.Parent))
            {
                kept += conflicts.Count;
                continue;
            }

            var before = conflicts.Count;
            conflicts.Clear();
            touched++;
            removed += before;
            if (config.Debug)
            {
                logger.Debug($"[HelmetSlotUnion] 늦은 쓸기 {id}: {before} → 0");
            }
        }

        logger.Success(
            $"[HelmetSlotUnion] 늦은 쓸기(맨 끝): 아이템 {touched}개에서 {removed}건을 더 지웠다. "
                + $"구경 제한으로 남긴 항목 {kept}건."
        );

        return Task.CompletedTask;
    }
}
