using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class DeadZoneTests
{
    [Fact]
    public void The_default_dead_zone_leaves_room_for_both_the_hole_and_the_ring()
    {
        Assert.Equal(0.45f, WheelConfig.Default().WheelDeadZone, 3);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(0.95f)]
    [InlineData(1.5f)]
    public void The_dead_zone_is_clamped_so_the_wheel_stays_usable(float value)
    {
        var config = WheelConfig.LoadFromText("{ \"wheelDeadZone\": " + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " }", out _);

        Assert.InRange(config.WheelDeadZone, WheelConfig.MinDeadZone, WheelConfig.MaxDeadZone);
    }

    [Fact]
    public void A_zero_dead_zone_can_never_be_configured()
    {
        // 0 이면 가운데에 둬도 길이가 0 이라 데드존 검사가 걸리지 않고,
        // Atan2(0,0)=0 이라 0도에 가장 가까운 항목이 늘 골라진다 — 취소할 길이 없어진다.
        Assert.True(WheelConfig.MinDeadZone > 0f);

        var config = WheelConfig.LoadFromText("{ \"wheelDeadZone\": 0 }", out _);
        Assert.Equal(-1, TargetPicker.PickByDirection(0f, 0f, 2, config.WheelDeadZone));
    }

    [Fact]
    public void The_hole_the_player_sees_is_exactly_where_selection_starts()
    {
        // 그려지는 구멍의 반지름과 고르기가 시작되는 거리가 같은 수에서 나와야
        // "아무 것도 안 그려진 빈 자리인데 이미 골라져 있는" 구간이 생기지 않는다.
        var deadZone = WheelConfig.Default().WheelDeadZone;

        Assert.Equal(deadZone * WheelLayout.OuterRadius, WheelLayout.InnerRadius(deadZone), 3);
    }

    [Fact]
    public void Neither_the_hole_nor_the_ring_can_be_squeezed_away()
    {
        // 설정을 끝까지 밀어도 구멍에 글자가 들어가고 고리가 가리킬 만한 두께로 남는다.
        foreach (var deadZone in new[] { WheelConfig.MinDeadZone, 0.3f, WheelConfig.MaxDeadZone })
        {
            var inner = WheelLayout.InnerRadius(deadZone);

            Assert.InRange(inner, 40f, WheelLayout.OuterRadius - 50f);
        }
    }
}
