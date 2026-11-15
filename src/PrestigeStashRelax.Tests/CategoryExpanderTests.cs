using PrestigeStashRelax;
using Xunit;

namespace PrestigeStashRelax.Tests;

/// <summary>
/// <see cref="CategoryExpander.Expand"/> 의 판정. 검사마다 다른 사유를 본다.
/// </summary>
public class CategoryExpanderTests
{
    /// <summary>
    /// 작은 표: 뿌리 Item 아래 Backpack 과 Vest, 그 아래 구체 아이템들.
    /// 모드가 넣은 배낭(mod_bag)이 바닐라 Backpack 아래 있는 경우를 함께 둔다.
    /// </summary>
    private static Dictionary<string, string> Table() => new(StringComparer.Ordinal)
    {
        [CategoryCatalog.Backpack] = CategoryCatalog.RootItem,
        [CategoryCatalog.Vest] = CategoryCatalog.RootItem,
        ["bag_a"] = CategoryCatalog.Backpack,
        ["bag_b"] = CategoryCatalog.Backpack,
        ["mod_bag"] = "bag_a",                      // 배낭을 복제한 모드 아이템
        ["vest_a"] = CategoryCatalog.Vest,
        ["gun"] = CategoryCatalog.RootItem,
    };

    [Fact]
    public void 분류_아래_아이템을_모은다()
    {
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.Backpack]);

        Assert.Contains("bag_a", result);
        Assert.Contains("bag_b", result);
    }

    [Fact]
    public void 손자까지_내려간다()
    {
        // mod_bag 의 부모는 bag_a 이고 Backpack 은 그 위다 — 한 단계만 보면 놓친다.
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.Backpack]);

        Assert.Contains("mod_bag", result);
    }

    [Fact]
    public void 다른_분류는_끌려오지_않는다()
    {
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.Backpack]);

        Assert.DoesNotContain("vest_a", result);
        Assert.DoesNotContain("gun", result);
    }

    [Fact]
    public void 분류_자신은_넣지_않는다()
    {
        // 분류 id 는 아이템이 아니라 창고에 들어올 일이 없다. 그것은 호출하는 쪽이 따로 남긴다.
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.Backpack]);

        Assert.DoesNotContain(CategoryCatalog.Backpack, result);
    }

    [Fact]
    public void 분류_여럿을_한꺼번에_펼친다()
    {
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.Backpack, CategoryCatalog.Vest]);

        Assert.Contains("bag_a", result);
        Assert.Contains("vest_a", result);
    }

    [Fact]
    public void 뿌리는_펼치지_않는다()
    {
        // 전부가 자손이라 1만 개를 적게 된다. 그 경우는 다른 방법이 필요하다.
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.RootItem]);

        Assert.Empty(result);
    }

    [Fact]
    public void 뿌리가_섞여_있어도_나머지는_펼친다()
    {
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.RootItem, CategoryCatalog.Backpack]);

        Assert.Contains("bag_a", result);
        Assert.DoesNotContain("gun", result);
    }

    [Fact]
    public void 아이템_id_를_넣으면_아무것도_안_나온다()
    {
        // 구체 아이템에는 자손이 없다 — 조용히 빈 결과여야 한다.
        var result = CategoryExpander.Expand(Table(), ["bag_b"]);

        Assert.Empty(result);
    }

    [Fact]
    public void 빈_입력은_빈_결과다()
    {
        Assert.Empty(CategoryExpander.Expand(Table(), []));
        Assert.Empty(CategoryExpander.Expand(Table(), ["", "  "]));
    }

    [Fact]
    public void 사슬이_돌아도_멈춘다()
    {
        var looped = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a"] = "b",
            ["b"] = "a",
            ["x"] = CategoryCatalog.Backpack,
            [CategoryCatalog.Backpack] = CategoryCatalog.RootItem,
        };

        var result = CategoryExpander.Expand(looped, [CategoryCatalog.Backpack]);

        Assert.Equal(["x"], result);
    }

    [Fact]
    public void 결과에_중복이_없다()
    {
        var result = CategoryExpander.Expand(Table(), [CategoryCatalog.Backpack, CategoryCatalog.Backpack]);

        Assert.Equal(result.Count, result.Distinct().Count());
    }
}
