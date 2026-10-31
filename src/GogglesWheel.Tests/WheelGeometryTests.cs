using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class WheelGeometryTests
{
    [Theory]
    [InlineData(1, 180f)]
    [InlineData(2, 90f)]
    [InlineData(3, 60f)]
    [InlineData(4, 45f)]
    public void A_sector_covers_its_share_of_the_circle(int count, float expected)
    {
        Assert.Equal(expected, TargetPicker.SectorHalfWidth(count), 3);
    }

    [Fact]
    public void No_items_means_no_sector()
    {
        Assert.Equal(0f, TargetPicker.SectorHalfWidth(0), 3);
    }

    [Fact]
    public void Sectors_tile_the_circle_without_gaps_or_overlap()
    {
        // 항목이 셋이면 각 조각이 120도를 덮어 세 조각이 정확히 한 바퀴가 된다.
        var half = TargetPicker.SectorHalfWidth(3);

        Assert.Equal(360f, half * 2f * 3f, 3);
    }

    [Fact]
    public void Every_direction_in_a_sector_picks_that_sector()
    {
        // 조각의 양 끝에서 안쪽으로 조금 들어온 방향은 그 조각이 잡혀야 한다.
        var half = TargetPicker.SectorHalfWidth(3);

        for (var i = 0; i < 3; i++)
        {
            var center = TargetPicker.AngleForIndex(i, 3);

            foreach (var offset in new[] { -half + 1f, 0f, half - 1f })
            {
                var radians = (center + offset) * System.MathF.PI / 180f;
                var x = System.MathF.Cos(radians);
                var y = System.MathF.Sin(radians);

                Assert.Equal(i, TargetPicker.PickByDirection(x, y, 3, 0.2f));
            }
        }
    }
}
