using PrestigeStashRelax;
using Xunit;

namespace PrestigeStashRelax.Tests;

/// <summary>
/// <see cref="StashPlan.Build"/> 의 판정. 검사마다 **다른 사유**를 본다 — 하나를 지웠을 때
/// 다른 검사가 그것을 덮어 주면 시험이 녹색인데도 구멍이 남는다.
/// </summary>
public class StashPlanTests
{
    /// <summary>P1 의 실측값을 줄여 쓴 원본. 가로 8 · 세로 3 · 분류 둘.</summary>
    private static StashShape P1() => new(8, 3, [CategoryCatalog.Weapon, "item_a"]);

    /// <summary>P6 의 실측값. 세로만 다르다 — 단계차가 지켜지는지 보는 데 쓴다.</summary>
    private static StashShape P6() => new(8, 8, [CategoryCatalog.Weapon, "item_a"]);

    [Fact]
    public void 설정이_비면_원본_그대로다()
    {
        var result = StashPlan.Build(P1(), new Config());

        Assert.Equal(8, result.XCellCount);
        Assert.Equal(3, result.YCellCount);
        Assert.Equal([CategoryCatalog.Weapon, "item_a"], result.IncludedItems);
    }

    [Fact]
    public void 두_번_돌려도_같다()
    {
        var config = new Config { YCellAdd = 4, AllowBags = true };

        var once = StashPlan.Build(P1(), config);
        var twice = StashPlan.Build(P1(), config);

        Assert.Equal(once.YCellCount, twice.YCellCount);
        Assert.Equal(once.IncludedItems, twice.IncludedItems);
    }

    [Fact]
    public void 세로_더하기는_단계차를_지킨다()
    {
        var config = new Config { YCellAdd = 5 };

        var p1 = StashPlan.Build(P1(), config);
        var p6 = StashPlan.Build(P6(), config);

        Assert.Equal(8, p1.YCellCount);
        Assert.Equal(13, p6.YCellCount);
        Assert.Equal(5, p6.YCellCount - p1.YCellCount);   // 원본 차이 8-3 이 그대로
    }

    [Fact]
    public void 세로_고정이_더하기를_이긴다()
    {
        var result = StashPlan.Build(P1(), new Config { YCellAdd = 5, YCellCount = 10 });

        Assert.Equal(10, result.YCellCount);
    }

    [Fact]
    public void 가로는_0이면_원본이고_값이_있으면_덮는다()
    {
        Assert.Equal(8, StashPlan.Build(P1(), new Config { XCellCount = 0 }).XCellCount);
        Assert.Equal(12, StashPlan.Build(P1(), new Config { XCellCount = 12 }).XCellCount);
    }

    [Fact]
    public void 칸_수는_아래끝과_위끝_사이로_잘린다()
    {
        var low = StashPlan.Build(P1(), new Config { XCellCount = 1, YCellAdd = -99 });
        Assert.Equal(StashPlan.MinCells, low.YCellCount);

        var high = StashPlan.Build(P1(), new Config { YCellCount = 9999 });
        Assert.Equal(StashPlan.MaxCellsUntilMeasured, high.YCellCount);
    }

    [Fact]
    public void 가방_허용은_배낭과_케이스_둘을_더한다()
    {
        var result = StashPlan.Build(P1(), new Config { AllowBags = true });

        Assert.Contains(CategoryCatalog.Backpack, result.IncludedItems);
        Assert.Contains(CategoryCatalog.SimpleContainer, result.IncludedItems);
    }

    [Fact]
    public void 조끼와_보안_컨테이너는_가방과_따로_켜진다()
    {
        var bagsOnly = StashPlan.Build(P1(), new Config { AllowBags = true });
        Assert.DoesNotContain(CategoryCatalog.Vest, bagsOnly.IncludedItems);
        Assert.DoesNotContain(CategoryCatalog.MobContainer, bagsOnly.IncludedItems);

        var vest = StashPlan.Build(P1(), new Config { AllowVest = true });
        Assert.Contains(CategoryCatalog.Vest, vest.IncludedItems);

        var secure = StashPlan.Build(P1(), new Config { AllowSecureContainer = true });
        Assert.Contains(CategoryCatalog.MobContainer, secure.IncludedItems);
    }

    [Fact]
    public void 탄_허용은_탄_탄박스_탄창_셋을_더한다()
    {
        var result = StashPlan.Build(P1(), new Config { AllowAmmo = true });

        Assert.Contains(CategoryCatalog.Ammo, result.IncludedItems);
        Assert.Contains(CategoryCatalog.AmmoBox, result.IncludedItems);
        Assert.Contains(CategoryCatalog.Magazine, result.IncludedItems);
    }

    [Fact]
    public void 전부_허용은_뿌리_분류를_더한다()
    {
        var result = StashPlan.Build(P1(), new Config { AllowEverything = true });

        Assert.Contains(CategoryCatalog.RootItem, result.IncludedItems);
    }

    [Fact]
    public void 필터_갈아치우기는_원본을_버린다()
    {
        var result = StashPlan.Build(P1(), new Config { ReplaceFilter = true, AllowBags = true });

        Assert.Equal([CategoryCatalog.Backpack, CategoryCatalog.SimpleContainer], result.IncludedItems);
    }

    [Fact]
    public void 이미_있는_분류는_두_번_들어가지_않는다()
    {
        // P1() 의 원본에 Weapon 이 이미 있다 — P3 이상이 그런 상태다.
        var result = StashPlan.Build(P1(), new Config { AllowWeaponsEverywhere = true });

        Assert.Single(result.IncludedItems, i => i == CategoryCatalog.Weapon);
    }

    [Fact]
    public void 직접_적은_id_가_들어간다()
    {
        var result = StashPlan.Build(P1(), new Config { ExtraIds = ["my_id"] });

        Assert.Contains("my_id", result.IncludedItems);
    }

    [Fact]
    public void 빈_id_는_버린다()
    {
        var result = StashPlan.Build(P1(), new Config { ExtraIds = ["", "  ", "ok"] });

        Assert.Equal([CategoryCatalog.Weapon, "item_a", "ok"], result.IncludedItems);
    }

    [Fact]
    public void 원본_리스트를_건드리지_않는다()
    {
        var origin = P1();

        StashPlan.Build(origin, new Config { AllowBags = true, ReplaceFilter = true });

        Assert.Equal([CategoryCatalog.Weapon, "item_a"], origin.IncludedItems);
        Assert.Equal(3, origin.YCellCount);
    }
}
