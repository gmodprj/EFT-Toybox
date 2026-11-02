using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 계획 4 Task 2. 규칙 하나가 전부다: 하나라도 값을 못 매기면 전체를 거절한다.
public class GearBuildQuotePolicyTests
{
    private static GearItemRow Item(string tpl) => new($"id-{tpl}", tpl, "parent", "slot");

    [Fact]
    public void 값을_더해_합계를_낸다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a"), Item("b")], tpl => tpl == "a" ? 100 : 250,
            out var quote, out var missing);

        Assert.True(ok);
        Assert.Equal(350, quote!.Total);
        Assert.Equal(2, quote.Lines.Count);
        Assert.Empty(missing);
    }

    [Fact]
    public void 소수점은_올린다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a")], _ => 100.2, out var quote, out _);

        Assert.True(ok);
        Assert.Equal(101, quote!.Total);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void 값이_없는_것이_하나라도_있으면_전체를_거절한다(double price)
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a"), Item("b")], tpl => tpl == "a" ? 100 : price,
            out var quote, out var missing);

        Assert.False(ok);
        Assert.Null(quote);
        Assert.Equal(["b"], missing);
    }

    [Fact]
    public void 값을_묻다_던지면_못_매긴_것으로_친다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a")], _ => throw new InvalidOperationException("표에 없다"),
            out var quote, out var missing);

        Assert.False(ok);
        Assert.Null(quote);
        Assert.Equal(["a"], missing);
    }

    [Fact]
    public void 못_매긴_tpl은_중복_없이_돌려준다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a"), Item("a")], _ => 0, out _, out var missing);

        Assert.False(ok);
        Assert.Equal(["a"], missing);
    }

    // 이미 그 빌드를 입고 있는 경우. 실패가 아니라 0 원이다.
    [Fact]
    public void 살_것이_없으면_0원이다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([], _ => 100, out var quote, out var missing);

        Assert.True(ok);
        Assert.Equal(0, quote!.Total);
        Assert.Empty(quote.Lines);
        Assert.Empty(missing);
    }

    // 값을 물을 길 자체가 없으면 조용히 0 원으로 넘기지 않는다.
    [Fact]
    public void 값을_물을_길이_없으면_거절한다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a")], null, out var quote, out var missing);

        Assert.False(ok);
        Assert.Null(quote);
        Assert.Equal(["a"], missing);
    }

    [Fact]
    public void 합이_청구할_수_없는_크기면_거절한다()
    {
        var ok = GearBuildQuotePolicy.TryQuote([Item("a"), Item("b")], _ => int.MaxValue, out var quote, out var missing);

        Assert.False(ok);
        Assert.Null(quote);
        Assert.NotEmpty(missing);
    }

    [Fact]
    public void 널_입력도_던지지_않는다()
    {
        var ok = GearBuildQuotePolicy.TryQuote(null, _ => 100, out var quote, out var missing);

        Assert.True(ok);
        Assert.Equal(0, quote!.Total);
        Assert.Empty(missing);
    }
}
