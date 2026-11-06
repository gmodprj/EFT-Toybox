using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 계획 4 Task 3 의 관문. 하나라도 걸리면 아무것도 쓰지 않는다.
public class GearKitPurchasePolicyTests
{
    private static GearKitPurchaseGate Decide(
        bool raid = false, bool journal = false, int items = 3,
        int quoted = 1000, int fresh = 1000, int money = 5000) =>
        GearKitPurchasePolicy.Decide(raid, journal, items, quoted, fresh, money);

    [Fact]
    public void 모두_맞으면_통과한다()
    {
        Assert.Equal(GearKitPurchaseGate.Allowed, Decide());
    }

    [Fact]
    public void 레이드_중에는_막는다()
    {
        Assert.Equal(GearKitPurchaseGate.RaidInProgress, Decide(raid: true));
    }

    [Fact]
    public void 끝나지_않은_전송이_있으면_막는다()
    {
        Assert.Equal(GearKitPurchaseGate.JournalBlocked, Decide(journal: true));
    }

    // 레이드가 먼저다: "지금은 안 된다" 가 "이 구매는 안 된다" 보다 앞선다.
    [Fact]
    public void 레이드가_다른_사유보다_앞선다()
    {
        Assert.Equal(GearKitPurchaseGate.RaidInProgress, Decide(raid: true, journal: true, items: 0, money: 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void 살_것이_없으면_막는다(int items)
    {
        Assert.Equal(GearKitPurchaseGate.NothingToBuy, Decide(items: items));
    }

    [Fact]
    public void 값이_바뀌었으면_막는다()
    {
        Assert.Equal(GearKitPurchaseGate.QuoteChanged, Decide(quoted: 1000, fresh: 1200));
    }

    // 싸졌어도 막는다: 보여 준 값과 다르면 플레이어가 보지 못한 거래다.
    [Fact]
    public void 값이_싸졌어도_막는다()
    {
        Assert.Equal(GearKitPurchaseGate.QuoteChanged, Decide(quoted: 1000, fresh: 800));
    }

    [Fact]
    public void 돈이_모자라면_막는다()
    {
        Assert.Equal(GearKitPurchaseGate.InsufficientFunds, Decide(quoted: 1000, fresh: 1000, money: 999));
    }

    [Fact]
    public void 딱_맞는_돈은_통과한다()
    {
        Assert.Equal(GearKitPurchaseGate.Allowed, Decide(quoted: 1000, fresh: 1000, money: 1000));
    }

    [Fact]
    public void 통과가_아닌_사유는_모두_말을_가진다()
    {
        foreach (GearKitPurchaseGate gate in Enum.GetValues<GearKitPurchaseGate>())
        {
            var text = GearKitPurchasePolicy.Describe(gate, 1000, 10);
            if (gate == GearKitPurchaseGate.Allowed) Assert.Equal(string.Empty, text);
            else Assert.False(string.IsNullOrWhiteSpace(text));
        }
    }

    [Fact]
    public void 돈_부족_사유는_필요한_값과_가진_값을_말한다()
    {
        var text = GearKitPurchasePolicy.Describe(GearKitPurchaseGate.InsufficientFunds, 1234567, 10);

        Assert.Contains("1,234,567", text);
        Assert.Contains("10", text);
    }
}
