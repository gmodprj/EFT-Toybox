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
/// 헬멧의 부착 칸 필터를 칸 이름별 합집합으로 바꾸고, 부품끼리 서로를 막는 제한을 푼다.
///
/// 게임 기본값은 칸마다 1~3종만 허용해서, 예컨대 Altyn 바이저를 Ops-Core FAST 에 끼울 수 없다.
/// 여기서는 같은 이름의 칸(helmet_ears, mod_nvg, mod_equipment_000 …)을 헬멧 전체에서 모아
/// 그 합집합을 각 칸의 필터로 다시 써 준다. 칸 이름이 같으면 붙는 자리도 같다는 전제다.
///
/// 칸 이름은 대소문자가 섞여 있어(Helmet_top / helmet_top) 소문자로 맞춰 묶는다.
///
/// 칸 필터와 별개로 "A와 B는 동시에 장착할 수 없습니다"(ConflictingItems) 도 정리한다.
/// 헬멧 쪽은 칸 필터를 넓힌 뒤 남는 제한을 푸는 것이고, 총기 쪽은 칸 필터를 넓히는 다른
/// 모드(SlotCategoryRelax·All In Weapon 등)를 쓸 때 같은 메시지가 남는 것을 푸는 것이다.
/// 총기는 칸 필터를 건드리지 않는다 — 충돌 목록만 본다.
/// </summary>
// 두 번째 값은 IOnLoad 실행 순서다. 숫자가 클수록 늦게 돈다(실측: 0 이면 SlotCategoryRelax 보다
// 먼저, 1000 이상이면 나중에 돈다). 칸 필터를 넓히는 모드보다 늦게 돌아야 넓어진 필터까지 따라가
// 충돌을 지울 수 있어서(대상 2178개 → 2183개) 일부러 큰 값을 쓴다.
[Injectable(InjectionType.Singleton, 100000)]
public class HelmetSlotUnion(
    ISptLogger<HelmetSlotUnion> logger,
    TemplateTable templateTable,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>Headwear 분류의 아이템 id. 이 밑에 달린 것만 헬멧으로 본다.</summary>
    private const string HeadwearTpl = "5a341c4086f77401f2541505";

    /// <summary>Weapon 분류의 아이템 id. 이 밑에 달린 것만 총기로 본다.</summary>
    private const string WeaponTpl = "5422acb9af1c889c16000029";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var config = modHelper.GetJsonDataFromFile<Config>(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
            "config.jsonc"
        );

        if (!config.ModEnabled)
        {
            logger.Info("[HelmetSlotUnion] 설정에서 꺼져 있어 아무것도 바꾸지 않았다.");
            return Task.CompletedTask;
        }

        var items = templateTable.Items;
        if (items is null || items.Count == 0)
        {
            logger.Warning("[HelmetSlotUnion] 아이템 표가 비어 있다. 건너뛴다.");
            return Task.CompletedTask;
        }

        var excludedSlots = new HashSet<string>(
            config.ExcludedSlotNames.Select(Normalize),
            StringComparer.Ordinal
        );
        var excludedHelmets = new HashSet<string>(config.ExcludedHelmetIds, StringComparer.OrdinalIgnoreCase);

        var helmets = FindItemsWithSlots(items, HeadwearTpl, excludedHelmets);
        if (helmets.Count == 0)
        {
            logger.Warning("[HelmetSlotUnion] 부착 칸이 있는 헬멧을 찾지 못했다.");
        }
        else
        {
            UnionHelmetSlots(helmets, config, excludedSlots);

            if (config.RemoveAttachmentConflicts || config.RemoveAllConflictsOnAttachments)
            {
                RemoveConflicts(
                    items,
                    helmets,
                    clearAll: config.RemoveAllConflictsOnAttachments,
                    excludedCategories: [],
                    label: "헬멧",
                    debug: config.Debug
                );
            }
        }

        if (config.RemoveWeaponAttachmentConflicts || config.RemoveAllConflictsOnWeaponAttachments)
        {
            RemoveWeaponConflicts(items, config);
        }

        return Task.CompletedTask;
    }

    /// <summary>같은 이름의 칸을 헬멧 전체에서 모아 그 합집합을 각 칸에 돌려준다.</summary>
    private void UnionHelmetSlots(List<TemplateItem> helmets, Config config, HashSet<string> excludedSlots)
    {
        // 1차: 칸 이름별로 쓸 수 있는 파츠를 모은다.
        var union = new Dictionary<string, HashSet<MongoId>>(StringComparer.Ordinal);
        foreach (var helmet in helmets)
        foreach (var slot in Slots(helmet))
        {
            var key = GroupKey(slot.Name, config.MergeEquipmentSlots);
            if (key is null || excludedSlots.Contains(key)) continue;

            if (!union.TryGetValue(key, out var pool))
            {
                pool = [];
                union[key] = pool;
            }

            foreach (var filter in Filters(slot))
            {
                if (filter.Filter is { } existing) pool.UnionWith(existing);
            }
        }

        // 2차: 모은 합집합을 각 칸에 돌려준다.
        var changedSlots = 0;
        var addedEntries = 0;
        foreach (var helmet in helmets)
        foreach (var slot in Slots(helmet))
        {
            var key = GroupKey(slot.Name, config.MergeEquipmentSlots);
            if (key is null || excludedSlots.Contains(key)) continue;
            if (!union.TryGetValue(key, out var pool) || pool.Count == 0) continue;

            foreach (var filter in Filters(slot))
            {
                if (filter.Filter is not { } target) continue;

                var before = target.Count;
                target.UnionWith(pool);
                var added = target.Count - before;
                if (added <= 0) continue;

                changedSlots++;
                addedEntries += added;
                if (config.Debug)
                {
                    logger.Debug(
                        $"[HelmetSlotUnion] {helmet.Id} / {slot.Name}: {before} → {target.Count} (+{added})"
                    );
                }
            }
        }

        logger.Success(
            $"[HelmetSlotUnion] 헬멧 {helmets.Count}종, 칸 묶음 {union.Count}개. "
                + $"{changedSlots}개 칸에 항목 {addedEntries}개를 더했다."
        );
    }

    /// <summary>
    /// 총기·총기 파츠끼리 서로를 막는 ConflictingItems 를 지운다.
    ///
    /// 칸 필터는 건드리지 않는다. 필터를 넓히는 것은 SlotCategoryRelax 같은 다른 모드의 몫이고,
    /// 넓힌 뒤에도 남는 "★ MBUS 가늠쇠과 ★ TR-1 아이템은 동시에 장착할 수 없습니다" 가
    /// 여기서 푸는 제한이다. 헬멧 쪽과 같은 이야기라 같은 모드에 둔다.
    ///
    /// 탄창·장전 손잡이는 기본으로 건드리지 않는다. 그쪽 충돌은 모양이 아니라 구경 이야기라서
    /// (.338 볼트에 7.62 총열 같은 조합을 막는다) 지우면 총이 성립하지 않는다.
    /// </summary>
    private void RemoveWeaponConflicts(Dictionary<MongoId, TemplateItem> items, Config config)
    {
        var excludedWeapons = new HashSet<string>(config.ExcludedWeaponIds, StringComparer.OrdinalIgnoreCase);
        var weapons = FindItemsWithSlots(items, WeaponTpl, excludedWeapons);
        if (weapons.Count == 0)
        {
            logger.Warning("[HelmetSlotUnion] 부착 칸이 있는 총기를 찾지 못했다.");
            return;
        }

        var excludedCategories = new HashSet<MongoId>(
            config.WeaponConflictExcludedCategoryIds.Select(id => new MongoId(id))
        );

        RemoveConflicts(
            items,
            weapons,
            clearAll: config.RemoveAllConflictsOnWeaponAttachments,
            excludedCategories: excludedCategories,
            label: "총기",
            debug: config.Debug
        );
    }

    /// <summary>
    /// "A와 B는 동시에 장착할 수 없습니다" 를 만드는 ConflictingItems 를 정리한다.
    ///
    /// 칸 필터를 넓혀도 이 목록이 남아 있으면 끼울 수 없다. 예컨대 Ops-Core FAST 멀티 히트
    /// 바이저는 Single Clamp 레일 어댑터 마운트를 막는다. 둘 다 헬멧 파츠이므로 지운다.
    ///
    /// 기본값은 범위 안(본체 + 그 파츠)끼리의 항목만 지운다. 헤드셋·안면 보호구·탄약처럼
    /// 범위 밖을 가리키는 항목은 다른 이야기라 그대로 둔다.
    /// excludedCategories 에 든 분류는 어느 쪽에 있든 그 항목을 남긴다.
    /// </summary>
    private void RemoveConflicts(
        Dictionary<MongoId, TemplateItem> items,
        List<TemplateItem> roots,
        bool clearAll,
        HashSet<MongoId> excludedCategories,
        string label,
        bool debug
    )
    {
        var scope = BuildScope(items, roots);

        var touched = 0;
        var removed = 0;
        var kept = 0;
        foreach (var id in scope)
        {
            if (!items.TryGetValue(id, out var item)) continue;
            if (item.Properties?.ConflictingItems is not { Count: > 0 } conflicts) continue;

            var before = conflicts.Count;
            if (excludedCategories.Contains(item.Parent))
            {
                // 이 분류가 건 제한은 그대로 둔다.
                kept += before;
                continue;
            }

            if (clearAll)
            {
                conflicts.Clear();
            }
            else
            {
                conflicts.RemoveWhere(other =>
                    scope.Contains(other)
                    && !(items.TryGetValue(other, out var target) && excludedCategories.Contains(target.Parent))
                );
            }

            var gone = before - conflicts.Count;
            kept += conflicts.Count;
            if (gone <= 0) continue;

            touched++;
            removed += gone;
            if (debug)
            {
                logger.Debug($"[HelmetSlotUnion] {label} 충돌 정리 {id}: {before} → {conflicts.Count} (-{gone})");
            }
        }

        var mode = clearAll ? "통째로" : "범위 안끼리만";
        logger.Success(
            $"[HelmetSlotUnion] {label} 충돌 정리({mode}): 본체 {roots.Count}종, 대상 {scope.Count}개 중 "
                + $"{touched}개에서 {removed}건을 지웠다. 남은 항목 {kept}건."
        );
    }

    /// <summary>본체와, 그 칸에 들어갈 수 있는 파츠, 그 파츠에 또 달리는 파츠까지 모은다.</summary>
    private static HashSet<MongoId> BuildScope(
        Dictionary<MongoId, TemplateItem> items,
        List<TemplateItem> roots
    )
    {
        var scope = new HashSet<MongoId>();
        foreach (var root in roots) scope.Add(root.Id);

        var pending = new Queue<MongoId>();
        foreach (var root in roots)
        foreach (var slot in Slots(root))
        foreach (var filter in Filters(slot))
        {
            if (filter.Filter is not { } ids) continue;
            foreach (var id in ids)
            {
                if (scope.Add(id)) pending.Enqueue(id);
            }
        }

        while (pending.Count > 0)
        {
            if (!items.TryGetValue(pending.Dequeue(), out var part)) continue;
            foreach (var slot in Slots(part))
            foreach (var filter in Filters(slot))
            {
                if (filter.Filter is not { } ids) continue;
                foreach (var id in ids)
                {
                    if (scope.Add(id)) pending.Enqueue(id);
                }
            }
        }

        return scope;
    }

    /// <summary>어떤 분류 밑에 달린 아이템 중 부착 칸이 있는 것.</summary>
    private static List<TemplateItem> FindItemsWithSlots(
        Dictionary<MongoId, TemplateItem> items,
        string rootTpl,
        HashSet<string> excludedIds
    )
    {
        // 부모 → 자식 색인을 만들고 뿌리부터 내려간다. 모드가 중간 분류를 끼워도 따라간다.
        var childrenByParent = new Dictionary<string, List<TemplateItem>>(StringComparer.Ordinal);
        foreach (var item in items.Values)
        {
            var parent = item.Parent.ToString();
            if (string.IsNullOrEmpty(parent)) continue;
            if (!childrenByParent.TryGetValue(parent, out var list))
            {
                list = [];
                childrenByParent[parent] = list;
            }

            list.Add(item);
        }

        var found = new List<TemplateItem>();
        var queue = new Queue<string>();
        queue.Enqueue(rootTpl);
        var seen = new HashSet<string>(StringComparer.Ordinal) { rootTpl };

        while (queue.Count > 0)
        {
            if (!childrenByParent.TryGetValue(queue.Dequeue(), out var children)) continue;
            foreach (var child in children)
            {
                var id = child.Id.ToString();
                if (!seen.Add(id)) continue;
                queue.Enqueue(id);
                if (excludedIds.Contains(id)) continue;
                if (Slots(child).Count > 0) found.Add(child);
            }
        }

        return found;
    }

    /// <summary>Slots 는 IEnumerable 이라 두 번 돌려면 담아 둬야 한다.</summary>
    private static List<Slot> Slots(TemplateItem item) =>
        item.Properties?.Slots is { } slots ? [.. slots] : [];

    private static List<SlotFilter> Filters(Slot slot) =>
        slot.Properties?.Filters is { } filters ? [.. filters] : [];

    private static string Normalize(string? name) => (name ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>칸 이름을 묶음 열쇠로 바꾼다. 이름이 없는 칸은 null 로 걸러 낸다.</summary>
    private static string? GroupKey(string? slotName, bool mergeEquipment)
    {
        var key = Normalize(slotName);
        if (key.Length == 0) return null;
        // mod_equipment_000 → mod_equipment. 번호를 떼어 한 묶음으로 본다.
        if (mergeEquipment && key.StartsWith("mod_equipment", StringComparison.Ordinal)) return "mod_equipment";
        return key;
    }
}
