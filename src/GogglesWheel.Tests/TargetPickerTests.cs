using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class TargetPickerTests
{
    private static ToggleTarget T(ToggleKind kind, string name = null)
        => new ToggleTarget(kind.ToString() + "-id", kind, name ?? kind.ToString());

    [Fact]
    public void Two_items_sit_left_and_right()
    {
        Assert.Equal(180f, TargetPicker.AngleForIndex(0, 2), 3);
        Assert.Equal(0f, TargetPicker.AngleForIndex(1, 2), 3);
    }

    [Fact]
    public void Four_items_start_at_top_clockwise()
    {
        Assert.Equal(90f, TargetPicker.AngleForIndex(0, 4), 3);
        Assert.Equal(0f, TargetPicker.AngleForIndex(1, 4), 3);
        Assert.Equal(-90f, TargetPicker.AngleForIndex(2, 4), 3);
        Assert.Equal(-180f, TargetPicker.AngleForIndex(3, 4), 3);
    }

    [Fact]
    public void Direction_inside_the_dead_zone_picks_nothing()
    {
        Assert.Equal(-1, TargetPicker.PickByDirection(0.1f, 0f, 2, 0.2f));
    }

    [Fact]
    public void Direction_right_picks_the_right_item()
    {
        Assert.Equal(1, TargetPicker.PickByDirection(1f, 0f, 2, 0.2f));
    }

    [Fact]
    public void Direction_left_picks_the_left_item()
    {
        Assert.Equal(0, TargetPicker.PickByDirection(-1f, 0f, 2, 0.2f));
    }

    [Fact]
    public void Direction_wraps_around_180_degrees()
    {
        // 네 항목에서 왼쪽(=180도) 바로 위·아래를 가리키면 그 이웃 항목으로만 간다.
        // 180 과 -180 이 같은 방향인데 숫자로는 멀어 보이는 자리다.
        var justAbove = TargetPicker.PickByDirection(-1f, 0.01f, 4, 0.2f);
        var justBelow = TargetPicker.PickByDirection(-1f, -0.01f, 4, 0.2f);

        Assert.Equal(3, justAbove);
        Assert.Equal(3, justBelow);
    }

    [Fact]
    public void Exact_boundary_between_two_items_is_stable()
    {
        // 세 항목: 90, -30, -150. 90 과 -30 의 중간각은 30 도.
        var first = TargetPicker.PickByDirection(0.866f, 0.5f, 3, 0.2f);
        var again = TargetPicker.PickByDirection(0.866f, 0.5f, 3, 0.2f);

        Assert.Equal(first, again);
        Assert.InRange(first, 0, 2);
    }

    [Fact]
    public void No_items_picks_nothing()
    {
        Assert.Equal(-1, TargetPicker.PickByDirection(1f, 0f, 0, 0.2f));
    }

    [Fact]
    public void Default_target_prefers_night_vision()
    {
        var targets = new[] { T(ToggleKind.FaceShield), T(ToggleKind.NightVision) };

        Assert.Equal(1, TargetPicker.DefaultTargetIndex(targets, "nightvision"));
    }

    [Fact]
    public void Default_target_falls_back_to_first_when_kind_absent()
    {
        var targets = new[] { T(ToggleKind.FaceShield), T(ToggleKind.HelmetVisor) };

        Assert.Equal(0, TargetPicker.DefaultTargetIndex(targets, "nightvision"));
    }

    [Fact]
    public void Default_target_can_be_the_face_shield()
    {
        var targets = new[] { T(ToggleKind.NightVision), T(ToggleKind.FaceShield) };

        Assert.Equal(1, TargetPicker.DefaultTargetIndex(targets, "faceshield"));
    }

    [Fact]
    public void Default_target_first_takes_the_first()
    {
        var targets = new[] { T(ToggleKind.FaceShield), T(ToggleKind.NightVision) };

        Assert.Equal(0, TargetPicker.DefaultTargetIndex(targets, "first"));
    }

    [Fact]
    public void Default_target_with_two_night_visions_takes_the_first()
    {
        var targets = new[]
        {
            new ToggleTarget("nv-a", ToggleKind.NightVision, "야시경 A"),
            new ToggleTarget("nv-b", ToggleKind.NightVision, "야시경 B"),
        };

        Assert.Equal(0, TargetPicker.DefaultTargetIndex(targets, "nightvision"));
    }

    [Fact]
    public void Default_target_on_empty_list_is_minus_one()
    {
        Assert.Equal(-1, TargetPicker.DefaultTargetIndex(new ToggleTarget[0], "nightvision"));
    }
}
