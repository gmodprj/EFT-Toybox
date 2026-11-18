using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class PriceNormalizerTests
{
    private static readonly Dictionary<string, double> Handbook = new()
    {
        [PriceText.RubTpl] = 1,
        [PriceText.UsdTpl] = 140,
        ["partA"] = 1000,
        ["partB"] = 500,
    };

    private static double? Lookup(string tpl) => Handbook.TryGetValue(tpl, out var v) ? v : null;

    private static PricePart P(string tpl, double count) => new PricePart(tpl, count);

    [Fact]
    public void Rub_IsAmount()
    {
        Assert.Equal(101, PriceNormalizer.ToRubles(new[] { P(PriceText.RubTpl, 101) }, Lookup));
    }

    [Fact]
    public void Usd_UsesHandbook()
    {
        Assert.Equal(1400, PriceNormalizer.ToRubles(new[] { P(PriceText.UsdTpl, 10) }, Lookup));
    }

    [Fact]
    public void Barter_SumsParts()
    {
        Assert.Equal(2500, PriceNormalizer.ToRubles(new[] { P("partA", 2), P("partB", 1) }, Lookup));
    }

    [Fact]
    public void Barter_WithUnknownPart_IsNull()
    {
        Assert.Null(PriceNormalizer.ToRubles(new[] { P("partA", 2), P("mystery", 1) }, Lookup));
    }

    [Fact]
    public void Empty_IsNull()
    {
        Assert.Null(PriceNormalizer.ToRubles(Array.Empty<PricePart>(), Lookup));
        Assert.Null(PriceNormalizer.ToRubles(null, Lookup));
    }
}
