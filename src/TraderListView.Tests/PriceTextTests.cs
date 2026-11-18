using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class PriceTextTests
{
    private static string ShortName(string tpl) => tpl == "bolts" ? "볼트" : tpl;

    private static PricePart P(string tpl, double count) => new PricePart(tpl, count);

    [Theory]
    [InlineData(PriceText.RubTpl, 101, "101 ₽")]
    [InlineData(PriceText.UsdTpl, 10, "10 $")]
    [InlineData(PriceText.EurTpl, 10, "10 €")]
    [InlineData(PriceText.RubTpl, 6000, "6 000 ₽")]
    [InlineData(PriceText.RubTpl, 1234567, "1 234 567 ₽")]
    public void Price_Currency(string tpl, double amount, string expected)
    {
        Assert.Equal(expected, PriceText.Price(new[] { P(tpl, amount) }, ShortName));
    }

    [Fact]
    public void Price_BarterOneKind_ShowsNameAndCount()
    {
        Assert.Equal("⇄ 볼트 ×3", PriceText.Price(new[] { P("bolts", 3) }, ShortName));
    }

    [Fact]
    public void Price_BarterTwoKinds_ShowsKindCount()
    {
        Assert.Equal("⇄ 물물교환 2종", PriceText.Price(new[] { P("bolts", 3), P("nuts", 1) }, ShortName));
    }

    [Fact]
    public void Price_Empty_IsDash()
    {
        Assert.Equal("-", PriceText.Price(Array.Empty<PricePart>(), ShortName));
    }

    [Fact]
    public void Stock_NoRestriction_IsStack()
    {
        Assert.Equal("999999", PriceText.Stock(999999, null, null));
    }

    [Fact]
    public void Stock_WithRestriction_ShowsRemaining()
    {
        Assert.Equal("50 (2/3)", PriceText.Stock(50, 3, 1));
    }

    [Theory]
    [InlineData(2, false, "LL2")]
    [InlineData(3, true, "LL3 🔒")]
    public void Loyalty(int level, bool locked, string expected)
    {
        Assert.Equal(expected, PriceText.Loyalty(level, locked));
    }
}
