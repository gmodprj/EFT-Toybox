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

        Assert.Equal("01234567", Assert.Single(result).Nickname);
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

    [Fact]
    public void 현재_세션_id_가_비어도_아무_프로필도_CurrentSession_이_되지_않는다_그리고_던지지_않는다()
    {
        var result = ProfileRosterPolicy.Select(new[] { Ready("bbbbbbbbbbbbbbbbbbbbbbbb", "Good") }, "");

        Assert.Equal(ProfileAvailability.Ready, Assert.Single(result).Availability);
    }
}
