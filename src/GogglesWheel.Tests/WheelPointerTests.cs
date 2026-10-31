using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class WheelPointerTests
{
    [Fact]
    public void No_movement_keeps_the_pointer_where_it_was()
    {
        var p = WheelPointer.Advance(0.3f, -0.4f, 0f, 0f, 0.15f);

        Assert.Equal(0.3f, p.X, 4);
        Assert.Equal(-0.4f, p.Y, 4);
    }

    [Fact]
    public void Right_and_up_move_the_pointer_right_and_up()
    {
        var p = WheelPointer.Advance(0f, 0f, 2f, 3f, 0.1f);

        Assert.Equal(0.2f, p.X, 4);
        Assert.Equal(0.3f, p.Y, 4);
    }

    [Fact]
    public void Sensitivity_scales_the_movement()
    {
        var slow = WheelPointer.Advance(0f, 0f, 4f, 0f, 0.05f);
        var fast = WheelPointer.Advance(0f, 0f, 4f, 0f, 0.20f);

        Assert.Equal(0.2f, slow.X, 4);
        Assert.Equal(0.8f, fast.X, 4);
    }

    [Fact]
    public void The_pointer_never_leaves_the_unit_circle()
    {
        var p = WheelPointer.Advance(0f, 0f, 100f, 100f, 1f);

        Assert.Equal(1.0, System.Math.Sqrt(p.X * p.X + p.Y * p.Y), 4);
        Assert.True(p.X > 0f && p.Y > 0f);
    }

    [Fact]
    public void Moving_back_brings_the_pointer_into_the_dead_zone_again()
    {
        var out1 = WheelPointer.Advance(0f, 0f, 4f, 0f, 0.2f);
        var back = WheelPointer.Advance(out1.X, out1.Y, -4f, 0f, 0.2f);

        Assert.True(System.Math.Abs(back.X) < 0.2f);
        Assert.Equal(-1, TargetPicker.PickByDirection(back.X, back.Y, 2, 0.2f));
    }

    [Fact]
    public void The_edge_clamp_is_a_position_not_a_budget()
    {
        // 테두리까지 밀고 나서 같은 양을 되돌리면 반대쪽 테두리로 간다 —
        // 넘친 만큼이 쌓여 있지 않기 때문이다. 손으로 다시 가운데로 가져오면 된다.
        var pushed = WheelPointer.Advance(0f, 0f, 10f, 0f, 0.2f);
        var back = WheelPointer.Advance(pushed.X, pushed.Y, -10f, 0f, 0.2f);

        Assert.Equal(1f, pushed.X, 4);
        Assert.Equal(-1f, back.X, 4);
    }

    [Theory]
    [InlineData(float.NaN, 0f)]
    [InlineData(0f, float.NaN)]
    [InlineData(float.PositiveInfinity, 0f)]
    public void Broken_axis_values_leave_the_pointer_alone(float dx, float dy)
    {
        var p = WheelPointer.Advance(0.5f, 0.1f, dx, dy, 0.15f);

        Assert.Equal(0.5f, p.X, 4);
        Assert.Equal(0.1f, p.Y, 4);
    }

    [Fact]
    public void A_pointer_pushed_to_the_edge_picks_the_item_in_that_direction()
    {
        // 오른쪽으로 끝까지 밀면 두 항목 중 오른쪽(1번) 이 잡힌다.
        var p = WheelPointer.Advance(0f, 0f, 20f, 0f, 0.15f);

        Assert.Equal(1, TargetPicker.PickByDirection(p.X, p.Y, 2, 0.2f));
    }
}
