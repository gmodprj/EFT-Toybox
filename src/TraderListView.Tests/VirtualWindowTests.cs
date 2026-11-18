using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class VirtualWindowTests
{
    [Fact]
    public void Top_StartsAtZero_WithTwoSpare()
    {
        var w = VirtualWindow.Compute(scrollTop: 0, rowHeight: 48, viewportHeight: 480, rowCount: 5000);

        Assert.Equal(0, w.First);
        Assert.Equal(12, w.Slots); // 10 보임 + 2 여유
    }

    [Fact]
    public void Scrolled_FirstIsFloorOfTop()
    {
        var w = VirtualWindow.Compute(scrollTop: 48 * 37 + 20, rowHeight: 48, viewportHeight: 480, rowCount: 5000);

        Assert.Equal(37, w.First);
    }

    [Fact]
    public void NegativeTop_FromElasticScroll_IsZero()
    {
        var w = VirtualWindow.Compute(scrollTop: -30, rowHeight: 48, viewportHeight: 480, rowCount: 5000);

        Assert.Equal(0, w.First);
    }

    [Fact]
    public void SameWindow_IsEqual_SoRebindCanBeSkipped()
    {
        var a = VirtualWindow.Compute(100, 48, 480, 5000);
        var b = VirtualWindow.Compute(110, 48, 480, 5000);
        var c = VirtualWindow.Compute(150, 48, 480, 5000);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void ZeroRowHeight_IsSafe()
    {
        var w = VirtualWindow.Compute(100, 0, 480, 10);

        Assert.Equal(0, w.First);
        Assert.Equal(0, w.Slots);
    }
}
