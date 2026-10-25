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
/// 헬멧의 부착 칸 필터를 칸 이름별 합집합으로 바꾼다.
///
/// 게임 기본값은 칸마다 1~3종만 허용해서, 예컨대 Altyn 바이저를 Ops-Core FAST 에 끼울 수 없다.
/// 여기서는 같은 이름의 칸(helmet_ears, mod_nvg, mod_equipment_000 …)을 헬멧 전체에서 모아
/// 그 합집합을 각 칸의 필터로 다시 써 준다. 칸 이름이 같으면 붙는 자리도 같다는 전제다.
///
/// 칸 이름은 대소문자가 섞여 있어(Helmet_top / helmet_top) 소문자로 맞춰 묶는다.
/// </summary>
[Injectable(InjectionType.Singleton, 0)]
public class HelmetSlotUnion(
    ISptLogger<HelmetSlotUnion> logger,
    TemplateTable templateTable,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>Headwear 분류의 아이템 id. 이 밑에 달린 것만 헬멧으로 본다.</summary>
    private const string HeadwearTpl = "5a341c4086f77401f2541505";

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

        var helmets = FindHelmetsWithSlots(items, excludedHelmets);
        if (helmets.Count == 0)
        {
            logger.Warning("[HelmetSlotUnion] 부착 칸이 있는 헬멧을 찾지 못했다.");
            return Task.CompletedTask;
        }

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
        return Task.CompletedTask;
    }

    /// <summary>Headwear 밑에 달린 아이템 중 부착 칸이 있는 것.</summary>
    private static List<TemplateItem> FindHelmetsWithSlots(
        Dictionary<MongoId, TemplateItem> items,
        HashSet<string> excludedHelmets
    )
    {
        // 부모 → 자식 색인을 만들고 Headwear 부터 내려간다. 모드가 중간 분류를 끼워도 따라간다.
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
        queue.Enqueue(HeadwearTpl);
        var seen = new HashSet<string>(StringComparer.Ordinal) { HeadwearTpl };

        while (queue.Count > 0)
        {
            if (!childrenByParent.TryGetValue(queue.Dequeue(), out var children)) continue;
            foreach (var child in children)
            {
                var id = child.Id.ToString();
                if (!seen.Add(id)) continue;
                queue.Enqueue(id);
                if (excludedHelmets.Contains(id)) continue;
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
