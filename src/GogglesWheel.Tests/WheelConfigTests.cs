using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class WheelConfigTests
{
    [Fact]
    public void Default_values_match_the_spec()
    {
        var config = WheelConfig.Default();

        Assert.True(config.Enabled);
        Assert.Equal(0.25f, config.LongPressSeconds, 3);
        Assert.Equal("nightvision", config.DefaultTarget);
        Assert.Equal(0.2f, config.WheelDeadZone, 3);
        Assert.True(config.UseGameWheelSkin);
        Assert.True(config.IncludeHelmetVisor);
        Assert.True(config.StandDownForCoti);
    }

    [Fact]
    public void LoadFromText_broken_json_falls_back_to_default_with_warning()
    {
        var config = WheelConfig.LoadFromText("{ not json", out var warning);

        Assert.Equal(0.25f, config.LongPressSeconds, 3);
        Assert.False(string.IsNullOrEmpty(warning));
    }

    [Fact]
    public void LoadFromText_accepts_comments_and_trailing_commas()
    {
        var config = WheelConfig.LoadFromText("// 주석\n{\n  \"enabled\": false,\n}", out var warning);

        Assert.False(config.Enabled);
        Assert.True(string.IsNullOrEmpty(warning));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(99f)]
    public void LongPressSeconds_is_clamped(float value)
    {
        var config = WheelConfig.LoadFromText("{ \"longPressSeconds\": " + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " }", out _);

        Assert.InRange(config.LongPressSeconds, 0.05f, 2.0f);
    }

    [Theory]
    [InlineData(-0.5f)]
    [InlineData(1.5f)]
    public void WheelDeadZone_is_clamped(float value)
    {
        var config = WheelConfig.LoadFromText("{ \"wheelDeadZone\": " + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " }", out _);

        Assert.InRange(config.WheelDeadZone, 0.0f, 0.9f);
    }

    [Fact]
    public void Unknown_default_target_falls_back_to_nightvision()
    {
        var config = WheelConfig.LoadFromText("{ \"defaultTarget\": \"바나나\" }", out _);

        Assert.Equal("nightvision", config.DefaultTarget);
    }
}
