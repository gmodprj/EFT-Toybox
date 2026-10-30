using DynamicMapsFactionMarkers;
using Xunit;

namespace DynamicMapsFactionMarkers.Tests;

public class RoleCatalogTests
{
    private static MarkerConfig ConfigWith(params MarkerGroupConfig[] groups)
    {
        var config = MarkerConfig.Default();
        config.Groups = groups.ToList();
        return config;
    }

    [Fact]
    public void Configured_role_uses_configured_color_and_label()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());

        catalog.Register(13709, "ISBBoss", "ISB BOSS");

        Assert.True(catalog.TryGet(13709, out var entry));
        Assert.Equal("ISB BOSS", entry.Category);
        Assert.Equal("Markers/star.png", entry.Style.IconPath);
        Assert.Equal(1.0f, entry.Style.R, 3);
        Assert.Equal(0.176f, entry.Style.G, 3);
        Assert.Equal(0.333f, entry.Style.B, 3);
    }

    [Fact]
    public void Three_white_tusk_roles_share_one_category()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());

        catalog.Register(13702, "ISBWhiteTusk", "WHITE TUSK");
        catalog.Register(13707, "ISBCommander", "WHITE TUSK");
        catalog.Register(13708, "ISBBossFollower", "WHITE TUSK");

        foreach (var roleId in new[] { 13702, 13707, 13708 })
        {
            Assert.True(catalog.TryGet(roleId, out var entry));
            Assert.Equal("WHITE TUSK", entry.Category);
            Assert.Equal(1.0f, entry.Style.R, 3);
            Assert.Equal(1.0f, entry.Style.G, 3);
            Assert.Equal(1.0f, entry.Style.B, 3);
        }
    }

    [Fact]
    public void Missing_locale_label_falls_back_to_scav_role_key()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());

        catalog.Register(13714, "ISBSniper", null);

        Assert.True(catalog.TryGet(13714, out var entry));
        Assert.Equal("ISBSniper", entry.Category);
    }

    [Fact]
    public void Duplicate_scav_role_in_config_first_line_wins()
    {
        var catalog = new RoleCatalog(ConfigWith(
            new MarkerGroupConfig("ISBBoss", "#FF2D55", "star"),
            new MarkerGroupConfig("ISBBoss", "#00FF00", "dot")));

        catalog.Register(13709, "ISBBoss", "ISB BOSS");

        Assert.True(catalog.TryGet(13709, out var entry));
        Assert.Equal("Markers/star.png", entry.Style.IconPath);
        Assert.Equal(1.0f, entry.Style.R, 3);
    }

    [Fact]
    public void Unparseable_color_falls_back_to_default_color_without_throwing()
    {
        var catalog = new RoleCatalog(ConfigWith(new MarkerGroupConfig("ISBBoss", "red", "star")));

        catalog.Register(13709, "ISBBoss", "ISB BOSS");

        Assert.True(catalog.TryGet(13709, out var entry));
        Assert.True(MarkerStyle.TryParseColor(RoleCatalog.DefaultColorHex, out var r, out var g, out var b));
        Assert.Equal(r, entry.Style.R, 3);
        Assert.Equal(g, entry.Style.G, 3);
        Assert.Equal(b, entry.Style.B, 3);
    }

    [Fact]
    public void Unknown_role_gets_auto_assigned_and_is_listed_in_discovered()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());

        catalog.Register(848400, "blackDivAssault", "GHOST BUST.");

        Assert.True(catalog.TryGet(848400, out var entry));
        Assert.Equal("GHOST BUST.", entry.Category);
        Assert.Single(catalog.Discovered);
        Assert.Equal(848400, catalog.Discovered[0].RoleId);
    }

    [Fact]
    public void Unknown_role_is_skipped_when_auto_assign_is_off()
    {
        var config = MarkerConfig.Default();
        config.AutoAssignUnknown = false;
        var catalog = new RoleCatalog(config);

        catalog.Register(848400, "blackDivAssault", "GHOST BUST.");

        Assert.False(catalog.TryGet(848400, out _));
        Assert.Empty(catalog.Discovered);
    }

    [Fact]
    public void Extra_roles_are_classified_even_when_auto_assign_is_off()
    {
        var config = MarkerConfig.Default();
        config.AutoAssignUnknown = false;
        config.ExtraRoles.Add(848400);
        var catalog = new RoleCatalog(config);

        catalog.Register(848400, "blackDivAssault", "GHOST BUST.");

        Assert.True(catalog.TryGet(848400, out var entry));
        Assert.Equal("GHOST BUST.", entry.Category);
    }

    [Fact]
    public void Palette_wraps_when_more_unknown_roles_than_colors()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());
        var count = RoleCatalog.FallbackPalette.Length + 3;

        for (var i = 0; i < count; i++)
        {
            catalog.Register(900000 + i, "modRole" + i, "MOD " + i);
        }

        Assert.Equal(count, catalog.Discovered.Count);
        for (var i = 0; i < count; i++)
        {
            Assert.True(catalog.TryGet(900000 + i, out var entry));

            var matchesPalette = RoleCatalog.FallbackPalette.Any(hex =>
            {
                MarkerStyle.TryParseColor(hex, out var r, out var g, out var b);
                return Math.Abs(r - entry.Style.R) < 0.001f
                    && Math.Abs(g - entry.Style.G) < 0.001f
                    && Math.Abs(b - entry.Style.B) < 0.001f;
            });
            Assert.True(matchesPalette, "자동 배정 색이 팔레트 밖이다: " + entry.ScavRole);
        }
    }

    [Fact]
    public void IsOurCategory_is_false_for_vanilla_categories()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());
        catalog.Register(13709, "ISBBoss", "ISB BOSS");

        foreach (var vanilla in new[] { "Scav", "Boss", "Enemy Player", "Friendly Player" })
        {
            Assert.False(catalog.IsOurCategory(vanilla));
        }

        Assert.True(catalog.IsOurCategory("ISB BOSS"));
    }

    [Fact]
    public void Register_is_idempotent()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());

        catalog.Register(848400, "blackDivAssault", "GHOST BUST.");
        catalog.Register(848400, "blackDivAssault", "GHOST BUST.");

        Assert.Single(catalog.Discovered);
    }

    [Fact]
    public void Extra_roles_register_without_morebots()
    {
        var config = MarkerConfig.Default();
        config.ExtraRoles.Add(848400);
        var catalog = new RoleCatalog(config);

        catalog.RegisterExtraRoles();

        Assert.True(catalog.TryGet(848400, out var entry));
        Assert.Equal("848400", entry.Category);
    }

    [Fact]
    public void Extra_role_takes_its_group_when_the_id_is_listed_as_scav_role()
    {
        var config = MarkerConfig.Default();
        config.Groups.Add(new MarkerGroupConfig("848400", "#2EC8FF", "diamond") { Label = "GHOST BUST." });
        config.ExtraRoles.Add(848400);
        var catalog = new RoleCatalog(config);

        catalog.RegisterExtraRoles();

        Assert.True(catalog.TryGet(848400, out var entry));
        Assert.Equal("GHOST BUST.", entry.Category);
        Assert.Equal("../DynamicMapsFactionMarkers/diamond.png", entry.Style.IconPath);
    }

    [Fact]
    public void Apply_labels_merges_white_tusk_once_the_locale_is_ready()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());
        catalog.Register(13702, "ISBWhiteTusk", null);
        catalog.Register(13707, "ISBCommander", null);
        catalog.Register(13708, "ISBBossFollower", null);

        // locale 이 늦게 올라온다 — Awake 시점에는 빈 표라 키가 그대로 범주가 된다.
        Assert.True(catalog.IsOurCategory("ISBWhiteTusk"));

        catalog.ApplyLabels(key => "WHITE TUSK");

        foreach (var roleId in new[] { 13702, 13707, 13708 })
        {
            Assert.True(catalog.TryGet(roleId, out var entry));
            Assert.Equal("WHITE TUSK", entry.Category);
        }

        Assert.True(catalog.IsOurCategory("WHITE TUSK"));
        Assert.False(catalog.IsOurCategory("ISBWhiteTusk"));
    }

    [Fact]
    public void Apply_labels_keeps_the_configured_label()
    {
        var catalog = new RoleCatalog(ConfigWith(new MarkerGroupConfig("ISBBoss", "#FF2D55", "star")
        {
            Label = "내 ISB 대장",
        }));
        catalog.Register(13709, "ISBBoss", null);

        catalog.ApplyLabels(key => "ISB BOSS");

        Assert.True(catalog.TryGet(13709, out var entry));
        Assert.Equal("내 ISB 대장", entry.Category);
    }

    [Fact]
    public void Apply_labels_refuses_a_vanilla_category_name_from_the_locale()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());
        catalog.Register(13709, "ISBBoss", null);

        catalog.ApplyLabels(key => "Scav");

        Assert.True(catalog.TryGet(13709, out var entry));
        Assert.Equal("ISBBoss", entry.Category);
        Assert.False(catalog.IsOurCategory("Scav"));
    }

    [Fact]
    public void Apply_labels_runs_once_only()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());
        catalog.Register(13709, "ISBBoss", null);

        Assert.True(catalog.ApplyLabels(key => "ISB BOSS"));
        Assert.False(catalog.ApplyLabels(key => "다른 이름"));

        Assert.True(catalog.TryGet(13709, out var entry));
        Assert.Equal("ISB BOSS", entry.Category);
    }

    [Fact]
    public void Auto_assigned_roles_get_different_colors_until_the_palette_wraps()
    {
        var catalog = new RoleCatalog(MarkerConfig.Default());

        for (var i = 0; i < RoleCatalog.FallbackPalette.Length; i++)
        {
            catalog.Register(900000 + i, "modRole" + i, "MOD " + i);
        }

        var seen = new HashSet<string>();
        for (var i = 0; i < RoleCatalog.FallbackPalette.Length; i++)
        {
            Assert.True(catalog.TryGet(900000 + i, out var entry));
            seen.Add($"{entry.Style.R:F3},{entry.Style.G:F3},{entry.Style.B:F3}");
        }

        Assert.Equal(RoleCatalog.FallbackPalette.Length, seen.Count);
    }

    [Fact]
    public void Vanilla_category_name_in_config_is_never_treated_as_ours()
    {
        var catalog = new RoleCatalog(ConfigWith(new MarkerGroupConfig("ISBBoss", "#FF2D55", "star")
        {
            Label = "Scav",
        }));

        catalog.Register(13709, "ISBBoss", "ISB BOSS");

        Assert.False(catalog.TryGet(13709, out _));
        Assert.False(catalog.IsOurCategory("Scav"));
    }
}
