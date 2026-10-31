using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class ProfileRosterPolicyTests
{
    private const string Me = "aaaaaaaaaaaaaaaaaaaaaaaa";

    private static ProfileSnapshot Ready(string id, string? nick, int aid = 1, int level = 10, string side = "Usec") =>
        new(id, aid, nick, level, side, false);

    [Fact]
    public void 현재_세션_프로필은_CurrentSession_으로_표시된다()
    {
        var result = ProfileRosterPolicy.Select(new[] { Ready(Me, "Me"), Ready("bbbbbbbbbbbbbbbbbbbbbbbb", "Other") }, Me);

        Assert.Equal(2, result.Count);
        Assert.Equal(ProfileAvailability.CurrentSession, result.Single(c => c.ProfileId == Me).Availability);
        Assert.Equal(ProfileAvailability.Ready, result.Single(c => c.ProfileId != Me).Availability);
    }

    [Fact]
    public void Info_가_없는_프로필은_Empty_로_표시되고_Ready_가_아니다()
    {
        var empty = new ProfileSnapshot("cccccccccccccccccccccccc", 0, null, null, null, false);

        var result = ProfileRosterPolicy.Select(new[] { empty }, Me);

        var c = Assert.Single(result);
        Assert.Equal(ProfileAvailability.Empty, c.Availability);
        Assert.NotEqual(ProfileAvailability.Ready, c.Availability);
    }

    [Fact]
    public void 읽기에_실패한_프로필_하나가_나머지_열거를_막지_않는다()
    {
        var broken = new ProfileSnapshot("dddddddddddddddddddddddd", 0, null, null, null, true);
        var good = Ready("bbbbbbbbbbbbbbbbbbbbbbbb", "Good");

        var result = ProfileRosterPolicy.Select(new[] { broken, good }, Me);

        Assert.Equal(2, result.Count);
        Assert.Equal(ProfileAvailability.Unreadable, result.Single(c => c.ProfileId == broken.ProfileId).Availability);
        Assert.Equal(ProfileAvailability.Ready, result.Single(c => c.ProfileId == good.ProfileId).Availability);
    }

    [Fact]
    public void 닉네임이_비면_프로필_id_앞_8자를_표시명으로_쓴다()
    {
        var result = ProfileRosterPolicy.Select(new[] { Ready("0123456789abcdef01234567", "  ") }, Me);

        var c = Assert.Single(result);
        Assert.Equal("01234567", c.Nickname);
        Assert.Equal(ProfileAvailability.Ready, c.Availability);
    }

    [Fact]
    public void 결과는_입력_순서와_무관하게_닉네임_오름차순이다()
    {
        var a = Ready("111111111111111111111111", "Alpha");
        var b = Ready("222222222222222222222222", "Bravo");
        var c = Ready("333333333333333333333333", "Charlie");

        var forward = ProfileRosterPolicy.Select(new[] { a, b, c }, Me).Select(x => x.Nickname).ToArray();
        var reversed = ProfileRosterPolicy.Select(new[] { c, a, b }, Me).Select(x => x.Nickname).ToArray();

        Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, forward);
        Assert.Equal(forward, reversed);
    }

    [Fact]
    public void Ready_가_아닌_후보는_Level_0_Side_빈문자열로_채워진다()
    {
        // 읽히지 않은 값을 그럴듯한 값으로 메우지 않는다. 스냅샷에 값이 있어도 Ready 가 아니면 버린다.
        var snapshots = new[]
        {
            Ready(Me, "Me", level: 42, side: "Bear"),
            new ProfileSnapshot("cccccccccccccccccccccccc", 0, null, null, null, false),
            new ProfileSnapshot("dddddddddddddddddddddddd", 7, "Half", 15, "Usec", true),
        };

        var result = ProfileRosterPolicy.Select(snapshots, Me);

        Assert.Equal(3, result.Count);
        Assert.All(result, c =>
        {
            Assert.NotEqual(ProfileAvailability.Ready, c.Availability);
            Assert.Equal(0, c.Level);
            Assert.Equal(string.Empty, c.Side);
        });
    }

    [Fact]
    public void Ready_후보는_스냅샷의_Level_과_Side_를_그대로_담는다()
    {
        var c = Assert.Single(ProfileRosterPolicy.Select(new[] { Ready("bbbbbbbbbbbbbbbbbbbbbbbb", "Good", aid: 9, level: 33, side: "Bear") }, Me));

        Assert.Equal(33, c.Level);
        Assert.Equal("Bear", c.Side);
        Assert.Equal(9, c.Aid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 현재_세션_id_가_비면_누가_로그인했는지_모르므로_전부_Unreadable_이고_던지지_않는다(string? blank)
    {
        var snapshots = new[] { Ready("bbbbbbbbbbbbbbbbbbbbbbbb", "Zed", level: 20, side: "Bear"), Ready(Me, "Amy") };

        var result = ProfileRosterPolicy.Select(snapshots, blank!);

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "Amy", "Zed" }, result.Select(c => c.Nickname).ToArray());
        Assert.All(result, c =>
        {
            Assert.Equal(ProfileAvailability.Unreadable, c.Availability);
            Assert.Equal(0, c.Level);
            Assert.Equal(string.Empty, c.Side);
        });
    }

    [Theory]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("  aaaaaaaaaaaaaaaaaaaaaaaa  ")]
    public void 대소문자나_공백이_달라도_현재_세션_프로필은_Ready_가_되지_않는다(string session)
    {
        var result = ProfileRosterPolicy.Select(new[] { Ready(Me, "Me") }, session);

        Assert.Equal(ProfileAvailability.CurrentSession, Assert.Single(result).Availability);
    }

    private static ProfileCandidate Cand(string id, ProfileAvailability a, int aid = 5, string nick = "Nick") =>
        new(id, aid, nick, a == ProfileAvailability.Ready ? 20 : 0, a == ProfileAvailability.Ready ? "Bear" : "", a);

    [Fact]
    public void 조인은_등록_순서를_지키고_후보의_가용성을_그대로_옮긴다()
    {
        var cands = new[]
        {
            Cand("bbbbbbbbbbbbbbbbbbbbbbbb", ProfileAvailability.Ready),
            Cand("cccccccccccccccccccccccc", ProfileAvailability.Empty),
            Cand("dddddddddddddddddddddddd", ProfileAvailability.Unreadable),
            Cand(Me, ProfileAvailability.CurrentSession),
        };

        var rows = ProfileRosterPolicy.JoinRegistered(cands, new[] { Me, "dddddddddddddddddddddddd", "cccccccccccccccccccccccc", "bbbbbbbbbbbbbbbbbbbbbbbb" });

        Assert.Equal(new[] { RosterRowStatus.CurrentSession, RosterRowStatus.Unreadable, RosterRowStatus.Empty, RosterRowStatus.Ready },
            rows.Select(r => r.Status).ToArray());
    }

    [Fact]
    public void 등록됐지만_카탈로그에_없는_프로필은_숨기지_않고_Missing_으로_남긴다()
    {
        var rows = ProfileRosterPolicy.JoinRegistered(
            new[] { Cand("bbbbbbbbbbbbbbbbbbbbbbbb", ProfileAvailability.Ready) },
            new[] { "eeeeeeeeeeeeeeeeeeeeeeee" });

        var row = Assert.Single(rows);
        Assert.Equal(RosterRowStatus.Missing, row.Status);
        Assert.Equal("eeeeeeee", row.Nickname);
        Assert.Equal(0, row.Aid);
    }

    [Fact]
    public void 조인은_후보가_없거나_null_이어도_등록된_모두를_Missing_으로_낸다()
    {
        var rows = ProfileRosterPolicy.JoinRegistered(null!, new[] { "a1", "a2" });
        Assert.All(rows, r => Assert.Equal(RosterRowStatus.Missing, r.Status));
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void 조인은_빈_id_를_건너뛰고_중복_id_를_하나로_합친다()
    {
        var rows = ProfileRosterPolicy.JoinRegistered(
            new[] { Cand("bbbbbbbbbbbbbbbbbbbbbbbb", ProfileAvailability.Ready) },
            new[] { " ", "", "bbbbbbbbbbbbbbbbbbbbbbbb", " bbbbbbbbbbbbbbbbbbbbbbbb " });

        Assert.Single(rows);
    }

    [Fact]
    public void 조인은_알_수_없는_가용성_값을_Unreadable_로_닫는다()
    {
        var rows = ProfileRosterPolicy.JoinRegistered(
            new[] { Cand("bbbbbbbbbbbbbbbbbbbbbbbb", (ProfileAvailability)99) },
            new[] { "bbbbbbbbbbbbbbbbbbbbbbbb" });

        Assert.Equal(RosterRowStatus.Unreadable, Assert.Single(rows).Status);
    }

    [Fact]
    public void 새로_등록할_후보는_Ready_이면서_아직_등록되지_않은_것만이다()
    {
        var cands = new[]
        {
            Cand("bbbbbbbbbbbbbbbbbbbbbbbb", ProfileAvailability.Ready),
            Cand("cccccccccccccccccccccccc", ProfileAvailability.Ready),
            Cand("dddddddddddddddddddddddd", ProfileAvailability.Empty),
            Cand(Me, ProfileAvailability.CurrentSession),
        };

        var offered = ProfileRosterPolicy.SelectUnregisteredReady(cands, new[] { "bbbbbbbbbbbbbbbbbbbbbbbb" });

        Assert.Equal("cccccccccccccccccccccccc", Assert.Single(offered).ProfileId);
    }
}
