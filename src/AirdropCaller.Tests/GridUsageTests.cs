using AirdropCaller.Server;
using Xunit;

namespace AirdropCaller.Tests;

/// <summary>
/// 칸 셈만 본다. 칸 지도는 SPT 가 채우므로 여기서는 **0 이 빈 칸**이라는 규칙만 지킨다.
/// </summary>
public class GridUsageTests
{
    private static int[,] Map(int w, int h, params (int X, int Y)[] filled)
    {
        var map = new int[h, w];
        foreach (var (x, y) in filled)
        {
            map[y, x] = 1;
        }

        return map;
    }

    [Fact]
    public void 빈_격자는_찬_칸이_없다()
    {
        var map = Map(10, 12);

        Assert.Equal(0, GridUsage.Used(map));
        Assert.Equal(120, GridUsage.Total(map));
    }

    [Fact]
    public void 찬_칸만_센다()
    {
        var map = Map(4, 3, (0, 0), (1, 0), (3, 2));

        Assert.Equal(3, GridUsage.Used(map));
        Assert.Equal(12, GridUsage.Total(map));
    }

    [Fact]
    public void 영이_아니면_무엇이든_찬_것으로_센다()
    {
        var map = Map(2, 2);
        map[0, 0] = 7;
        map[1, 1] = -1;

        Assert.Equal(2, GridUsage.Used(map));
    }

    [Fact]
    public void 격자가_여럿이면_합쳐_센다()
    {
        var (used, total) = GridUsage.Sum([Map(4, 3, (0, 0)), Map(2, 2, (0, 0), (1, 1))]);

        Assert.Equal(3, used);
        Assert.Equal(16, total);
    }

    [Fact]
    public void 격자가_없으면_0_이다()
    {
        var (used, total) = GridUsage.Sum(null);

        Assert.Equal(0, used);
        Assert.Equal(0, total);
    }

    [Theory]
    [InlineData(0, 120, "칸 0/120 (0%)")]
    [InlineData(37, 120, "칸 37/120 (31%)")]
    [InlineData(120, 120, "칸 120/120 (100%)")]
    [InlineData(1, 3, "칸 1/3 (33%)")]
    [InlineData(1, 2, "칸 1/2 (50%)")]
    public void 사람이_읽는_토막(int used, int total, string expected)
    {
        Assert.Equal(expected, GridUsage.Describe(used, total));
    }

    [Fact]
    public void 칸_수를_모르면_아는_척하지_않는다()
    {
        Assert.Equal("칸 수를 모른다", GridUsage.Describe(0, 0));
        Assert.Equal("칸 수를 모른다", GridUsage.Describe(5, -1));
    }
}
