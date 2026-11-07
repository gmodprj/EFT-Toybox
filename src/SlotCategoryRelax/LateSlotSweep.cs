using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SlotCategoryRelax;

/// <summary>
/// 칸 넓히기를 **맨 끝에서 한 번 더** 한다.
///
/// 왜 — 2026-11-07 실측. 기동 로그의 시각이 그대로 말해 준다(UTC):
///
///   04:25:15  [COTI] Thermal riser created, id 6ad3016b…      ← 라이저가 생긴다(필터 65개)
///   04:25:47  [SlotCategoryRelax] … 칸 4124개에 654007개를 더했다   ← PostLoad, 우리가 넓힌다
///   04:25:53  [COTI] Thermal riser: 89 sights and mounts (…)  ← COTI 가 **나중에** 다시 쓴다
///
/// ECOTI 의 <c>ThermalRiserCompatibility</c> 는 우선순위를 적지 않은 <c>Injectable</c> 이라
/// 기본값 <c>int.MaxValue</c> 로 <c>PostLoad</c>(1000000) 뒤에 돈다. 그리고 라이저의 조준경 목록을
/// **더하는 게 아니라 갈아 끼운다.** 그래서 우리가 넣은 분류가 통째로 사라졌다:
///
///   원본 65개   AssaultScope 8 · Collimator 21 · CompactCollimator 1 · OpticScope 2 · SpecialScope 3 · Mount 30
///   COTI 뒤 89개 CompactCollimator 23 · OpticScope 50 · Mount 16 · **Collimator 0**
///
/// FALKE LE(<c>68a5ab09c44fa287ba0a97b5</c>)는 <c>Collimator</c> 라 목록에서 빠졌고, 게임이
/// "아이템이 호환되지 않습니다" 로 막았다. 호환 강조 모드는 다른 기준으로 칠해 어긋나 보였다.
///
/// 이 쓸기는 같은 <c>int.MaxValue</c> 자리에 서서 **COTI 가 다시 쓴 뒤의 목록**을 기준으로 넓힌다.
/// 같은 자리의 쓰기가 클라이언트에 닿는 것은 HelmetSlotUnion 의 늦은 쓸기에서 이미 실측했다
/// (<c>ConflictingItems</c> 기준. 필터 쪽은 이 모드의 설치 뒤 실측으로 확인한다).
///
/// 두 번 돌아도 <c>Filter</c> 가 집합이라 중복이 생기지 않는다 — 늘어난 항목 수만 로그가 다르다.
///
/// **알아 둘 것**: 이것은 COTI 가 고른 목록 밖의 조준경까지 올라가게 한다. 끼워지기는 해도
/// 열화상이 안 나올 수 있다(COTI 가 들고 있는 메시가 10개다 — <c>meshes: 10 unchanged</c>).
/// 그 경우 평범한 도트가 올라간 라이저가 된다. 원래대로 두려면 <c>LateSweep: false</c>.
/// </summary>
[Injectable(InjectionType.Singleton, int.MaxValue)]
public class LateSlotSweep(
    ISptLogger<LateSlotSweep> logger,
    TemplateTable templateTable,
    ModHelper modHelper,
    // PostLoad 패스와 같은 코드를 쓴다. 둘 다 싱글턴이라 DI 가 같은 인스턴스를 준다.
    SlotCategoryRelax relax
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<Config>(modFolder, "config.jsonc");

        if (!config.ModEnabled || !config.LateSweep)
        {
            return Task.CompletedTask;
        }

        var items = templateTable.Items;
        if (items is null || items.Count == 0)
        {
            return Task.CompletedTask;
        }

        // 분류를 **다시** 찾는다. 앞 패스 뒤에 생긴 아이템과, COTI 처럼 늦게 필터를 다시 쓴
        // 아이템까지 보려면 그 시점의 DB 로 새로 세어야 한다.
        var discovered = SlotCategoryRelax.DiscoverCategories(items, config.IncludeAttachmentSlots);

        var (slots, added) = relax.RunSlotPass(items, config, discovered, "늦은 쓸기(맨 끝): ");

        if (slots == 0)
        {
            logger.Info("[SlotCategoryRelax] 늦은 쓸기(맨 끝): 더할 것이 없었다 — 앞 패스 뒤에 필터를 다시 쓴 모드가 없다는 뜻이다.");
        }

        return Task.CompletedTask;
    }
}
