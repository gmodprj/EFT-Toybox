using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace PrestigeStashRelax;

/// <summary>
/// 프레스티지 **이전 창고**(전송 창고)의 크기와 담을 수 있는 품목을 넓힌다.
///
/// 왜 — 2026-11-15 실측(doc/specs/2026-11-15-prestige-stash-relax-design.md). 제한은
/// <c>templateTable.Prestige</c> 의 <c>elements[i].transferConfigs.stashConfig</c> 하나이고,
/// 현재 값은 P1 8×3 에서 P6 8×8 까지(가로는 8 고정, 세로만 1씩)다. 품목은 화이트리스트이고
/// <c>excludedItems</c> 는 여섯 단계 모두 비어 있다. P3 부터는 무기·부품·방어 장비·헤드셋이
/// 이미 열려 있으나, **가방·배낭·케이스·조끼·돈·탄·열쇠는 어느 단계에도 없다.**
///
/// 서버는 전송 요청을 **검사하지 않는다** — <c>PrestigeHelper.ProcessPendingPrestige</c> 는
/// 클라이언트가 보낸 목록을 <c>GetItemWithChildren</c> 로 자식째 집어 우편으로 보낸다. 즉
/// 제한은 클라이언트 화면이 이 데이터를 보고 스스로 거는 것이고, 데이터를 바꾸면 그대로 풀린다.
/// 가방을 허용하면 **안에 든 것이 함께 넘어간다** — 그것이 이 모드의 요점이다.
///
/// <c>PrestigeController.GetPrestige</c> 가 <c>templateTable.Prestige</c> 를 그대로 돌려주고,
/// 클라이언트는 <c>/client/prestige/list</c> 를 한 세션에 여러 번(11-15 로그에서 6번) 다시
/// 물어본다. 그래서 메모리의 표만 고쳐도 다음 조회부터 반영된다.
///
/// 자리는 <c>int.MaxValue</c> — 프레스티지 데이터를 싣는 모드(2540 Content Backport)보다
/// **뒤여야** 한다. 그것이 맞는지는 기동 로그의 단계 수로 확인한다(4 면 우리가 먼저 돈 것이고,
/// 6 이면 제자리다).
/// </summary>
[Injectable(InjectionType.Singleton, int.MaxValue)]
public class PrestigeStashRelax(
    ISptLogger<PrestigeStashRelax> logger,
    TemplateTable templateTable,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>현재 값과 적용 결과를 적어 두는 파일 이름. 로그를 긁지 않고 숫자를 보는 자리다.</summary>
    public const string ReportFileName = "prestige-stash-report.json";

    /// <summary>
    /// 단계별 원본. 열쇠는 <b>리스트 색인이 아니라 단계 id</b> 다 — 다른 모드가
    /// <c>Elements</c> 를 새 리스트로 갈아끼우면 색인으로 들고 있던 참조가 떠 버린다
    /// (3332 Prestige Perks 가 그럴 수 있다).
    /// </summary>
    private readonly Dictionary<string, StashShape> _origin = new(StringComparer.Ordinal);

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<Config>(modFolder, "config.jsonc");

        if (!config.ModEnabled)
        {
            logger.Info("[PrestigeStashRelax] 꺼져 있어 아무것도 보지 않았다.");
            return Task.CompletedTask;
        }

        var elements = templateTable.Prestige?.Elements;
        if (elements is null || elements.Count == 0)
        {
            logger.Warning("[PrestigeStashRelax] 프레스티지 표가 비어 있다. 건너뛴다.");
            return Task.CompletedTask;
        }

        // 적용 순서의 실측값. 2540 이 P5·P6 을 넣었으면 6 이다.
        logger.Info($"[PrestigeStashRelax] 프레스티지 단계 {elements.Count}개를 보았다.");

        var before = new List<StageReport>();
        var after = new List<StageReport>();

        foreach (var element in elements)
        {
            var stash = element.TransferConfigs?.StashConfig;
            if (stash is null)
            {
                logger.Warning($"[PrestigeStashRelax] {element.Id} 에 stashConfig 가 없다. 건너뛴다.");
                continue;
            }

            var id = element.Id ?? string.Empty;
            var current = Read(stash);

            // 원본은 **처음 본 그 값**만 기억한다. 두 번째 기동이나 두 번째 적용에서 덮지 않는다.
            if (!_origin.ContainsKey(id))
            {
                _origin[id] = current;
            }

            var origin = _origin[id];
            before.Add(new StageReport(id, origin.XCellCount, origin.YCellCount, origin.IncludedItems.Count));

            if (!config.Apply)
            {
                continue;
            }

            var planned = StashPlan.Build(origin, config);
            Write(stash, planned);
            after.Add(new StageReport(id, planned.XCellCount, planned.YCellCount, planned.IncludedItems.Count));

            if (config.Debug)
            {
                logger.Info(
                    $"[PrestigeStashRelax] {id}: {origin.XCellCount}x{origin.YCellCount}"
                    + $"({origin.IncludedItems.Count}) → {planned.XCellCount}x{planned.YCellCount}"
                    + $"({planned.IncludedItems.Count})");
            }
        }

        if (config.Apply)
        {
            logger.Info($"[PrestigeStashRelax] {after.Count}단계에 적용했다.");
        }
        else
        {
            logger.Info("[PrestigeStashRelax] 재기만 했다 — 고치려면 config.jsonc 의 Apply 를 true 로.");
        }

        if (config.Report)
        {
            WriteReport(modFolder, before, after, config);
        }

        return Task.CompletedTask;
    }

    private static StashShape Read(StashPrestigeConfig stash) =>
        new(
            stash.XCellCount ?? 0,
            stash.YCellCount ?? 0,
            stash.Filters?.IncludedItems?.Select(i => i.ToString()!).ToList() ?? []);

    private static void Write(StashPrestigeConfig stash, StashShape shape)
    {
        stash.XCellCount = shape.XCellCount;
        stash.YCellCount = shape.YCellCount;

        stash.Filters ??= new StashPrestigeFilters();
        stash.Filters.IncludedItems = shape.IncludedItems.Select(i => new MongoId(i)).ToList();
    }

    private void WriteReport(string modFolder, List<StageReport> before, List<StageReport> after, Config config)
    {
        try
        {
            // `Path` 는 SPT 의 Tables.Path 와 이름이 겹친다 — 전체 이름으로 적는다.
            var path = System.IO.Path.Combine(modFolder, ReportFileName);
            var payload = new
            {
                measuredAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"),
                stageCount = before.Count,
                applied = config.Apply,
                replaceFilter = config.ReplaceFilter,
                before,
                after,
            };

            File.WriteAllText(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
            logger.Info($"[PrestigeStashRelax] 적어 두었다: {path}");
        }
        catch (Exception ex)
        {
            logger.Warning($"[PrestigeStashRelax] 보고서를 못 적었다: {ex.Message}");
        }
    }

    /// <summary>보고서 한 줄.</summary>
    public sealed record StageReport(string Id, int X, int Y, int IncludedCount);
}
