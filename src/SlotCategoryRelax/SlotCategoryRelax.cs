using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SlotCategoryRelax;

/// <summary>
/// 분류별로 골라 칸 필터를 넓힌다.
///
/// All In Weapon: Unslotted 는 칸에 들어가던 부품들의 부모 분류를 모아 그 분류 전부를 넣는다.
/// 켜고 끄는 것이 전부라 "조준경은 아무 총에나, 핸드가드는 제 총 것만" 같은 구분이 안 된다.
/// 여기서는 같은 방식을 쓰되 분류마다 켜고 끈다.
///
/// 규칙: 어떤 칸이 켜진 분류의 부품을 **하나라도** 받고 있으면, 그 칸에 같은 분류의 부품을 모두 넣는다.
/// 칸이 원래 받던 다른 분류는 그대로 둔다 — 넓히기만 하고 좁히지 않는다.
///
/// 어떤 분류가 있는지는 설치본마다 다르다(모드가 분류를 더하기도 한다). 그래서 매번 DB 에서
/// 찾아 <c>discovered-categories.json</c> 로 내놓는다. 설정 화면은 그 목록을 보고 스위치를 만든다.
/// </summary>
[Injectable(InjectionType.Singleton, 0)]
public class SlotCategoryRelax(
    ISptLogger<SlotCategoryRelax> logger,
    TemplateTable templateTable,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>설정 화면이 읽을 분류 목록 파일 이름.</summary>
    public const string DiscoveredFileName = "discovered-categories.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<Config>(modFolder, "config.jsonc");

        var items = templateTable.Items;
        if (items is null || items.Count == 0)
        {
            logger.Warning("[SlotCategoryRelax] 아이템 표가 비어 있다. 건너뛴다.");
            return Task.CompletedTask;
        }

        // 칸 필터에 실제로 나타나는 분류를 전부 찾는다. 설정이 꺼져 있어도 목록은 내놓는다 —
        // 설정 화면이 이것을 보고 스위치를 만들기 때문이다.
        var discovered = DiscoverCategories(items, config.IncludeAttachmentSlots);
        WriteDiscovered(modFolder, discovered, items);

        if (!config.ModEnabled)
        {
            logger.Info($"[SlotCategoryRelax] 꺼져 있어 아무것도 바꾸지 않았다. 분류 {discovered.Count}종을 찾아 목록만 내놓는다.");
            return Task.CompletedTask;
        }

        // 켜진 분류의 노드 id 를 모은다.
        // 약실은 칸(Slots)이 아니라 Chambers 라 분류 스위치와 상관없이 따로 본다.
        if (config.RelaxChambers)
        {
            RelaxChamberFilters(items, config);
        }

        var openIds = new HashSet<MongoId>();
        var openNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (id, name) in discovered)
        {
            if (config.Categories.TryGetValue(name, out var on) && on)
            {
                openIds.Add(id);
                openNames.Add(name);
            }
        }

        var unknown = config.Categories
            .Where(kv => kv.Value && !discovered.Values.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
            .Select(kv => kv.Key)
            .ToList();
        if (unknown.Count > 0)
        {
            logger.Warning($"[SlotCategoryRelax] 이 설치본에 없는 분류 {unknown.Count}개: {string.Join(", ", unknown)}");
        }

        if (openIds.Count == 0)
        {
            logger.Info($"[SlotCategoryRelax] 켠 분류가 없어 아무것도 바꾸지 않았다. 고를 수 있는 분류 {discovered.Count}종.");
            return Task.CompletedTask;
        }

        var byCategory = GroupByCategory(items, openIds);
        var excludedSlots = new HashSet<string>(
            config.ExcludedSlotNames.Select(n => n.ToLowerInvariant()),
            StringComparer.Ordinal);

        var touchedSlots = 0;
        var addedEntries = 0;

        foreach (var (_, template) in items)
        {
            if (!OwnsRelevantSlots(items, template, config.IncludeAttachmentSlots))
            {
                continue;
            }

            foreach (var slot in SlotsOf(template))
            {
                if (slot.Name is null || excludedSlots.Contains(slot.Name.ToLowerInvariant()))
                {
                    continue;
                }

                var filter = slot.Properties?.Filters?.FirstOrDefault()?.Filter;
                if (filter is null || filter.Count == 0)
                {
                    continue;
                }

                // 이 칸이 이미 받고 있는 분류 중 켜진 것만 넓힌다.
                var present = new HashSet<MongoId>();
                foreach (var tpl in filter)
                {
                    if (items.TryGetValue(tpl, out var entry) && openIds.Contains(entry.Parent))
                    {
                        present.Add(entry.Parent);
                    }
                }

                if (present.Count == 0)
                {
                    continue;
                }

                // Filter 는 HashSet 이라 그대로 더하면 된다. 넓히기만 하고 좁히지 않는다.
                var before = filter.Count;

                foreach (var category in present)
                {
                    foreach (var candidate in byCategory[category])
                    {
                        filter.Add(candidate);
                    }
                }

                if (filter.Count == before)
                {
                    continue;
                }

                touchedSlots++;
                addedEntries += filter.Count - before;

                if (config.Debug)
                {
                    logger.Info($"[SlotCategoryRelax] {template.Name}/{slot.Name}: {before} → {filter.Count}");
                }
            }
        }

        logger.Info(
            $"[SlotCategoryRelax] 고를 수 있는 분류 {discovered.Count}종 중 {openNames.Count}종을 켰다"
            + $"({string.Join(", ", openNames)}). 칸 {touchedSlots}개에 항목 {addedEntries}개를 더했다.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// 약실(<c>Chambers</c>)이 받는 탄을 넓힌다 — All In Weapon 의 <c>FckChambers</c> 자리다.
    ///
    /// 약실은 부품 칸(<c>Slots</c>)이 아니라 <c>Chambers</c> 에 들어 있어 분류 스위치가 닿지 않는다.
    /// 그래서 따로 돈다. 규칙은 같다 — 그 약실이 이미 받고 있는 분류(거의 다 <c>Ammo</c>)의 아이템을
    /// 전부 넣는다. 넓히기만 하고 좁히지 않는다.
    ///
    /// 약실이 받는 분류를 그대로 쓰므로 RPG 처럼 <c>Rocket</c> 만 받던 약실에는 로켓만 들어간다.
    /// 반대로 유탄발사기·신호권총도 <c>Ammo</c> 를 받으므로 소총탄이 들어간다 — "모든 총기의 약실에
    /// 모든 탄종" 이라는 뜻 그대로다. 빼고 싶으면 <c>ExcludedChamberWeaponIds</c> 에 적는다.
    ///
    /// 탄창은 건드리지 않는다. 다른 구경의 탄은 약실에 한 발씩 물리는 것까지다.
    /// </summary>
    private void RelaxChamberFilters(Dictionary<MongoId, TemplateItem> items, Config config)
    {
        var excluded = new HashSet<string>(config.ExcludedChamberWeaponIds, StringComparer.OrdinalIgnoreCase);
        var excludedCalibers = new HashSet<string>(config.ExcludedChamberCalibers, StringComparer.OrdinalIgnoreCase);
        var skipped = 0;

        // 분류 → 그 분류의 아이템. 약실에 실제로 나타난 분류만 뒤늦게 채운다.
        var byCategory = new Dictionary<MongoId, List<MongoId>>();

        var touchedChambers = 0;
        var addedEntries = 0;
        var weapons = 0;

        foreach (var (_, template) in items)
        {
            // Chambers 는 IEnumerable 이라 세려면 담아 둬야 한다.
            var chambers = template.Properties?.Chambers is { } list ? list.ToList() : [];
            if (chambers.Count == 0)
            {
                continue;
            }

            if (excluded.Contains(template.Id.ToString()))
            {
                skipped++;
                continue;
            }

            // 유탄발사기·신호권총처럼 소총탄이 들어갈 자리가 아닌 것은 구경으로 거른다.
            if (template.Properties?.AmmoCaliber is { } caliber && excludedCalibers.Contains(caliber))
            {
                skipped++;
                if (config.Debug)
                {
                    logger.Info($"[SlotCategoryRelax] 약실 건너뜀 {template.Name} — 구경 {caliber}");
                }

                continue;
            }

            weapons++;
            foreach (var chamber in chambers)
            {
                var filter = chamber.Properties?.Filters?.FirstOrDefault()?.Filter;
                if (filter is null || filter.Count == 0)
                {
                    continue;
                }

                var present = new HashSet<MongoId>();
                foreach (var tpl in filter)
                {
                    if (items.TryGetValue(tpl, out var entry))
                    {
                        present.Add(entry.Parent);
                    }
                }

                var before = filter.Count;
                foreach (var category in present)
                {
                    if (!byCategory.TryGetValue(category, out var pool))
                    {
                        pool = [];
                        foreach (var (id, candidate) in items)
                        {
                            if (candidate.Parent == category)
                            {
                                pool.Add(id);
                            }
                        }

                        byCategory[category] = pool;
                    }

                    foreach (var candidate in pool)
                    {
                        filter.Add(candidate);
                    }
                }

                if (filter.Count == before)
                {
                    continue;
                }

                touchedChambers++;
                addedEntries += filter.Count - before;

                if (config.Debug)
                {
                    logger.Info($"[SlotCategoryRelax] {template.Name}/약실 {chamber.Name}: {before} → {filter.Count}");
                }
            }
        }

        logger.Info(
            $"[SlotCategoryRelax] 약실: 총기 {weapons}종의 약실 {touchedChambers}개에 탄 {addedEntries}개를 더했다"
            + $"(분류 {byCategory.Count}종). 설정으로 건너뛴 총기 {skipped}종.");
    }

    /// <summary>
    /// 칸 필터에 나타나는 분류 노드를 찾는다. id → 이름.
    ///
    /// 무기와 무기 부품의 칸만 본다. 그러지 않으면 보안 컨테이너·장비 칸까지 훑어
    /// Ammo·KeyMechanical·Money 같은 분류가 목록에 올라오고, 그것을 켜면 보관 제한이 무너진다.
    /// 더하는 쪽도 Mod 계열만 쓴다 — 조상 경로가 그 둘을 깔끔하게 가른다
    /// (Handguard &lt; MasterMod &lt; Mod, Ammo &lt; StackableItem).
    /// </summary>
    private static Dictionary<MongoId, string> DiscoverCategories(
        Dictionary<MongoId, TemplateItem> items,
        bool includeAttachmentSlots)
    {
        var found = new Dictionary<MongoId, string>();

        foreach (var (_, template) in items)
        {
            if (!OwnsRelevantSlots(items, template, includeAttachmentSlots))
            {
                continue;
            }

            foreach (var slot in SlotsOf(template))
            {
                var filter = slot.Properties?.Filters?.FirstOrDefault()?.Filter;
                if (filter is null)
                {
                    continue;
                }

                foreach (var tpl in filter)
                {
                    if (!items.TryGetValue(tpl, out var entry))
                    {
                        continue;
                    }

                    var parent = entry.Parent;
                    if (found.ContainsKey(parent))
                    {
                        continue;
                    }

                    // 무기 부품 계열만 고를 수 있게 한다.
                    if (items.TryGetValue(parent, out var node)
                        && node.Name is not null
                        && HasAncestor(items, node, "Mod"))
                    {
                        found[parent] = node.Name;
                    }
                }
            }
        }

        return found;
    }

    /// <summary>켠 분류마다 그 분류에 속한 아이템을 모은다.</summary>
    private static Dictionary<MongoId, List<MongoId>> GroupByCategory(
        Dictionary<MongoId, TemplateItem> items,
        HashSet<MongoId> openIds)
    {
        var byCategory = new Dictionary<MongoId, List<MongoId>>();

        foreach (var id in openIds)
        {
            byCategory[id] = [];
        }

        foreach (var (id, template) in items)
        {
            if (byCategory.TryGetValue(template.Parent, out var list))
            {
                list.Add(id);
            }
        }

        return byCategory;
    }

    /// <summary>
    /// 찾은 분류를 설정 화면이 읽을 수 있게 파일로 내놓는다.
    /// 분류마다 그 분류에 속한 아이템 수를 함께 적어 고를 때 참고가 되게 한다.
    /// </summary>
    private void WriteDiscovered(
        string modFolder,
        Dictionary<MongoId, string> discovered,
        Dictionary<MongoId, TemplateItem> items)
    {
        try
        {
            var counts = new Dictionary<MongoId, int>();
            foreach (var (_, template) in items)
            {
                if (discovered.ContainsKey(template.Parent))
                {
                    counts[template.Parent] = counts.GetValueOrDefault(template.Parent) + 1;
                }
            }

            var payload = discovered
                .Select(kv => new { name = kv.Value, items = counts.GetValueOrDefault(kv.Key) })
                .OrderBy(e => e.name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            File.WriteAllText(
                System.IO.Path.Combine(modFolder, DiscoveredFileName),
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            logger.Warning($"[SlotCategoryRelax] 분류 목록을 내놓지 못했다 — {ex.Message}");
        }
    }

    private static IEnumerable<Slot> SlotsOf(TemplateItem template) =>
        template.Properties?.Slots ?? [];

    /// <summary>
    /// 이 아이템의 칸을 건드려도 되는가. 무기는 언제나, 무기 부품은 설정에 따라.
    /// 헬멧·조끼·보관함의 칸은 여기서 걸러진다.
    /// </summary>
    private static bool OwnsRelevantSlots(
        Dictionary<MongoId, TemplateItem> items,
        TemplateItem template,
        bool includeAttachmentSlots)
    {
        if (HasAncestor(items, template, "Weapon"))
        {
            return true;
        }

        return includeAttachmentSlots && HasAncestor(items, template, "Mod");
    }

    /// <summary>조상 노드 중에 그 이름이 있는가. 자기 자신은 세지 않는다.</summary>
    private static bool HasAncestor(
        Dictionary<MongoId, TemplateItem> items,
        TemplateItem template,
        string ancestorName)
    {
        var parent = template.Parent;
        for (var depth = 0; depth < 16 && parent != default; depth++)
        {
            if (!items.TryGetValue(parent, out var node))
            {
                return false;
            }

            if (string.Equals(node.Name, ancestorName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            parent = node.Parent;
        }

        return false;
    }
}
