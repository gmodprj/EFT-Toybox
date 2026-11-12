using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SlotFilterSweep;

/// <summary>
/// 칸 필터에 적혀 있지만 **데이터베이스에 없는 부품 id** 를 걷어낸다.
///
/// 왜 — 2026-11-12 실측(doc/REVIEW-MODS-AUDIT-2026-11-12.md). 총기 1,038종의 칸 6,846개 가운데
/// 21칸이 **없는 부품 13종**을 가리키고 있었다(바닐라는 0회). 서버도 기동마다
/// `Could not add &lt;id&gt; to cache, it does not exist in the item database!` 를 554줄 적는다.
/// 출처는 남의 모드가 적어 둔 묵은 참조다 — WTT-Armory 9종, EukyreECOT 2종, 그리고 출처를
/// 못 찾은 2종이 변형 생성 모드를 타고 9배로 불어나 있었다.
///
/// 하는 일은 **빼기뿐이다.** 없는 부품을 만들어 주지 않고, 살아 있는 id 는 건드리지 않는다.
/// 남의 모드 폴더도 고치지 않는다 — 메모리의 표만 손보므로 그 모드를 업데이트해도 날아갈 것이 없다.
///
/// 자리는 <c>int.MaxValue</c> — 아이템을 만드는 모드(PostLoad)보다 뒤여야 한다. 그보다 늦게
/// 만드는 모드가 있으면 <c>KeepIds</c> 로 지킨다.
///
/// **기본값은 세기만 하는 것이다**(<c>Apply: false</c>). 남이 적어 둔 목록을 고치는 일이라
/// 숫자를 먼저 보고 켜는 순서가 맞다.
/// </summary>
[Injectable(InjectionType.Singleton, int.MaxValue)]
public class SlotFilterSweep(
    ISptLogger<SlotFilterSweep> logger,
    TemplateTable templateTable,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>찾은 것을 적어 두는 파일 이름. 로그를 긁지 않고 숫자를 보는 자리다.</summary>
    public const string ReportFileName = "dead-refs.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<Config>(modFolder, "config.jsonc");

        if (!config.ModEnabled)
        {
            logger.Info("[SlotFilterSweep] 꺼져 있어 아무것도 보지 않았다.");
            return Task.CompletedTask;
        }

        var items = templateTable.Items;
        if (items is null || items.Count == 0)
        {
            logger.Warning("[SlotFilterSweep] 아이템 표가 비어 있다. 건너뛴다.");
            return Task.CompletedTask;
        }

        var known = new HashSet<string>(items.Keys.Select(k => k.ToString()!), StringComparer.Ordinal);
        var keep = new HashSet<string>(config.KeepIds, StringComparer.Ordinal);

        var filtersSeen = 0;
        var deadRefs = 0;
        var removed = 0;
        var byId = new Dictionary<string, int>(StringComparer.Ordinal);
        var places = new List<Place>();

        foreach (var (templateId, template) in items)
        {
            foreach (var (kind, slot) in SlotsToCheck(template, config))
            {
                var filter = slot.Properties?.Filters?.FirstOrDefault()?.Filter;
                if (filter is null || filter.Count == 0)
                {
                    continue;
                }

                filtersSeen++;

                var dead = DeadRefScan.DeadIn(filter.Select(f => f.ToString()!), known, keep);
                if (dead.Count == 0)
                {
                    continue;
                }

                deadRefs += dead.Count;
                foreach (var id in dead)
                {
                    byId[id] = byId.TryGetValue(id, out var n) ? n + 1 : 1;
                }

                places.Add(new Place(
                    templateId.ToString()!,
                    template.Name ?? string.Empty,
                    kind,
                    slot.Name ?? string.Empty,
                    dead));

                if (config.Debug)
                {
                    logger.Info($"[SlotFilterSweep] {template.Name} [{kind}:{slot.Name}] 죽은 참조 {dead.Count}개: "
                                + string.Join(", ", dead));
                }

                if (!config.Apply)
                {
                    continue;
                }

                // 빼기만 한다. 살아 있는 것은 그대로 둔다.
                foreach (var id in dead)
                {
                    if (filter.Remove(new MongoId(id)))
                    {
                        removed++;
                    }
                }
            }
        }

        logger.Info("[SlotFilterSweep] " + DeadRefScan.Describe(filtersSeen, deadRefs, byId.Count, config.Apply));

        foreach (var (id, count) in byId.OrderByDescending(kv => kv.Value).Take(Math.Max(0, config.TopIds)))
        {
            logger.Info($"[SlotFilterSweep]   {id} ← {count}칸");
        }

        if (config.Apply && removed != deadRefs)
        {
            // 조용히 지나가지 않는다 — 센 것과 지운 것이 다르면 둘 중 하나가 틀렸다.
            logger.Warning($"[SlotFilterSweep] 센 것 {deadRefs}개 중 {removed}개만 지워졌다. 남은 것은 다음 기동에 다시 보인다.");
        }

        if (config.WriteReport)
        {
            WriteReport(modFolder, config, filtersSeen, deadRefs, byId, places);
        }

        return Task.CompletedTask;
    }

    /// <summary>설정이 켠 자리만 돌려준다. 꼬리표는 로그와 보고서에 그대로 쓴다.</summary>
    private static IEnumerable<(string Kind, Slot Slot)> SlotsToCheck(TemplateItem template, Config config)
    {
        if (config.IncludeSlots)
        {
            foreach (var slot in template.Properties?.Slots ?? [])
            {
                yield return ("Slots", slot);
            }
        }

        if (config.IncludeChambers && template.Properties?.Chambers is { } chambers)
        {
            foreach (var chamber in chambers)
            {
                yield return ("Chambers", chamber);
            }
        }

        if (config.IncludeCartridges && template.Properties?.Cartridges is { } cartridges)
        {
            foreach (var cartridge in cartridges)
            {
                yield return ("Cartridges", cartridge);
            }
        }
    }

    private void WriteReport(
        string modFolder,
        Config config,
        int filtersSeen,
        int deadRefs,
        Dictionary<string, int> byId,
        List<Place> places)
    {
        try
        {
            // SPT 에도 Path 라는 타입이 있어 이름이 겹친다 — 파일 경로 쪽을 분명히 적는다.
            var path = System.IO.Path.Combine(modFolder, ReportFileName);
            var report = new
            {
                takenAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                applied = config.Apply,
                filtersSeen,
                deadRefs,
                distinctIds = byId.Count,
                byId = byId.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value),
                places,
            };

            // 바깥은 camelCase 인데 안쪽 record 만 PascalCase 로 나가 읽는 쪽이 헷갈렸다. 하나로 맞춘다.
            File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        }
        catch (Exception e)
        {
            // 보고서를 못 썼다고 서버 기동을 막지는 않는다. 다만 조용히 넘어가지도 않는다.
            logger.Warning($"[SlotFilterSweep] 보고서를 쓰지 못했다: {e.Message}");
        }
    }

    /// <summary>죽은 참조가 발견된 자리 하나.</summary>
    private sealed record Place(string ItemId, string ItemName, string Kind, string SlotName, List<string> DeadIds);
}
