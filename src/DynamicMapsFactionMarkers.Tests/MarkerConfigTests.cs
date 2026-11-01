using DynamicMapsFactionMarkers;
using Xunit;

namespace DynamicMapsFactionMarkers.Tests;

public class MarkerConfigTests
{
    [Fact]
    public void Default_covers_all_ten_isb_scav_roles()
    {
        var config = MarkerConfig.Default();

        Assert.Equal(
            new[]
            {
                "ISB", "ISBAssassin", "ISBBoss", "ISBBossFollower", "ISBCommander",
                "ISBGuard", "ISBHeavy", "ISBScout", "ISBSniper", "ISBWhiteTusk",
            },
            config.Groups.Select(g => g.ScavRole).OrderBy(r => r, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Default_white_tusk_three_keys_share_one_look()
    {
        var config = MarkerConfig.Default();

        foreach (var scavRole in new[] { "ISBWhiteTusk", "ISBCommander", "ISBBossFollower" })
        {
            var group = config.Groups.Single(g => g.ScavRole == scavRole);
            Assert.Equal("#FFFFFF", group.ColorHex);
            Assert.Equal("star", group.Shape);
        }
    }

    [Fact]
    public void Default_plain_soldier_and_guard_are_arrows_of_their_own_color()
    {
        var config = MarkerConfig.Default();

        var soldier = config.Groups.Single(g => g.ScavRole == "ISB");
        Assert.Equal("#C400FF", soldier.ColorHex);
        Assert.Equal("arrow", soldier.Shape);

        var guard = config.Groups.Single(g => g.ScavRole == "ISBGuard");
        Assert.Equal("#FF9ED8", guard.ColorHex);
        Assert.Equal("arrow", guard.Shape);
    }

    [Fact]
    public void Default_isb_boss_is_red_star()
    {
        var group = MarkerConfig.Default().Groups.Single(g => g.ScavRole == "ISBBoss");

        Assert.Equal("#FF2D55", group.ColorHex);
        Assert.Equal("star", group.Shape);
    }

    [Fact]
    public void Default_sniper_is_cyan_diamond()
    {
        var group = MarkerConfig.Default().Groups.Single(g => g.ScavRole == "ISBSniper");

        Assert.Equal("#2EC8FF", group.ColorHex);
        Assert.Equal("diamond", group.Shape);
    }

    [Fact]
    public void Default_intel_source_is_boss()
    {
        Assert.Equal("boss", MarkerConfig.Default().IntelLevelSource);
    }

    [Fact]
    public void Default_auto_assign_unknown_is_true()
    {
        Assert.True(MarkerConfig.Default().AutoAssignUnknown);
    }

    [Fact]
    public void LoadFromText_broken_json_falls_back_to_default_with_warning()
    {
        var config = MarkerConfig.LoadFromText("{ not json", out var warning);

        Assert.Equal(MarkerConfig.Default().Groups.Count, config.Groups.Count);
        Assert.False(string.IsNullOrEmpty(warning));
    }

    [Fact]
    public void LoadFromText_comments_and_trailing_commas_are_accepted()
    {
        var text = "// 주석\n{\n  \"enabled\": false, // 끝쉼표가 붙어 있다\n}";

        var config = MarkerConfig.LoadFromText(text, out var warning);

        Assert.False(config.Enabled);
        Assert.True(string.IsNullOrEmpty(warning));
    }

    [Fact]
    public void LoadFromText_keeps_default_groups_when_file_omits_them()
    {
        var config = MarkerConfig.LoadFromText("{ \"enabled\": true }", out var warning);

        Assert.Equal(MarkerConfig.Default().Groups.Count, config.Groups.Count);
        Assert.True(string.IsNullOrEmpty(warning));
    }

    [Theory]
    [InlineData("#FF2D55")]
    [InlineData("ff2d55")]
    public void TryParseColor_accepts_six_digit_hex_with_or_without_hash(string hex)
    {
        Assert.True(MarkerStyle.TryParseColor(hex, out var r, out var g, out var b));

        Assert.Equal(1.0f, r, 3);
        Assert.Equal(0.176f, g, 3);
        Assert.Equal(0.333f, b, 3);
    }

    [Theory]
    [InlineData("red")]
    [InlineData("#GG0000")]
    [InlineData("#FFF")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParseColor_rejects_non_hex(string hex)
    {
        Assert.False(MarkerStyle.TryParseColor(hex, out _, out _, out _));
    }

    [Fact]
    public void IconPathForShape_diamond_points_into_our_own_folder()
    {
        Assert.Equal("../DynamicMapsFactionMarkers/diamond.png", MarkerStyle.IconPathForShape("diamond"));
    }

    [Fact]
    public void IconPathForShape_known_shapes_use_dynamicmaps_markers()
    {
        Assert.Equal("Markers/star.png", MarkerStyle.IconPathForShape("star"));
        Assert.Equal("Markers/arrow.png", MarkerStyle.IconPathForShape("arrow"));
        Assert.Equal("Markers/dot.png", MarkerStyle.IconPathForShape("dot"));
    }

    [Theory]
    [InlineData("triangle")]
    [InlineData("")]
    [InlineData(null)]
    public void IconPathForShape_unknown_shape_falls_back_to_arrow(string shape)
    {
        Assert.Equal("Markers/arrow.png", MarkerStyle.IconPathForShape(shape));
    }
}
