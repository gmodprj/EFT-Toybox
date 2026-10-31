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
    public void Exact_boundary_goes_to_the_lower_index()
    {
        // 네 항목(90·0·-90·-180)에서 45도는 0번과 1번의 **정확한 경계**다.
        // 부동소수 잡음으로 갈리지 않게, 비길 때는 낮은 번호가 이긴다고 못박는다.
        var atFortyFive = (float)System.Math.Sqrt(0.5);

        Assert.Equal(0, TargetPicker.PickByDirection(atFortyFive, atFortyFive, 4, 0.2f));
    }

    [Fact]
    public void Three_items_pick_across_the_180_seam()
    {
        // 세 항목: 90·-30·-150. ±180 이음매가 **항목 사이**에 떨어지는 어려운 경우다.
        // 179도(= 거의 왼쪽)는 -150 쪽(2번)이 가장 가깝다.
        Assert.Equal(2, TargetPicker.PickByDirection(-0.9998f, 0.0175f, 3, 0.2f));

        // -179도도 같은 항목이어야 한다. 숫자로는 358도 떨어져 보이는 자리다.
        Assert.Equal(2, TargetPicker.PickByDirection(-0.9998f, -0.0175f, 3, 0.2f));
    }

    [Fact]
    public void Normalized_tilt_is_capped_at_one()
    {
        // 쌓인 시점 입력은 끝없이 커진다. 데드존이 0~1 단위이므로 거기에 맞춰 눌러 준다.
        var (x, y) = TargetPicker.NormalizeTilt(600f, 0f, 6f);

        Assert.Equal(1f, x, 3);
        Assert.Equal(0f, y, 3);
    }

    [Fact]
    public void Normalized_tilt_keeps_direction_below_the_cap()
    {
        var (x, y) = TargetPicker.NormalizeTilt(3f, 0f, 6f);

        Assert.Equal(0.5f, x, 3);
        Assert.Equal(0f, y, 3);
    }

    [Fact]
    public void Normalized_tilt_survives_a_zero_or_negative_full_tilt()
    {
        var (x, y) = TargetPicker.NormalizeTilt(3f, 4f, 0f);

        Assert.InRange(System.Math.Sqrt(x * x + y * y), 0.999, 1.001);
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
