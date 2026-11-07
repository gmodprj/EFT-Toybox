using System.Linq;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class RaidTallyPolicyTests
{
    private static RaidTally Survived(int kills = 0, int seconds = 600) =>
        new RaidTally("6ae5870eeb15d61bf003fd1f", RaidExitStatus.Survived, kills, seconds);

    private static RaidTally Killed(int kills = 0, int seconds = 600) =>
        new RaidTally("6ae5870eeb15d61bf003fd1f", RaidExitStatus.Killed, kills, seconds);

    private static long ValueOf(TallyPlan plan, params string[] key) =>
        plan.Increments.Where(i => i.Key.SequenceEqual(key)).Sum(i => i.Value);

    private static bool Has(TallyPlan plan, params string[] key) =>
        plan.Increments.Any(i => i.Key.SequenceEqual(key));

    [Fact]
    public void 생환하면_레이드_횟수와_생존이_각각_하나_늘어난다()
    {
        var plan = RaidTallyPolicy.Decide(Survived(), 0, 0);

        Assert.True(plan.Accepted);
        Assert.Equal(1, ValueOf(plan, "Sessions", "Pmc"));
        Assert.Equal(1, ValueOf(plan, "ExitStatus", "Survived", "Pmc"));
        Assert.False(Has(plan, "ExitStatus", "Killed", "Pmc"));
        Assert.False(Has(plan, "Deaths"));
    }

    [Fact]
    public void 죽으면_KIA_와_사망이_늘고_생존은_늘지_않는다()
    {
        var plan = RaidTallyPolicy.Decide(Killed(), 0, 0);

        Assert.True(plan.Accepted);
        Assert.Equal(1, ValueOf(plan, "Sessions", "Pmc"));
        Assert.Equal(1, ValueOf(plan, "ExitStatus", "Killed", "Pmc"));
        Assert.Equal(1, ValueOf(plan, "Deaths"));
        Assert.False(Has(plan, "ExitStatus", "Survived", "Pmc"));
    }

    // 셈의 불변식: 한 판에 Sessions 가 +1 이고 ExitStatus 합도 +1 이다. 어긋나면 화면의
    // 생존률이 틀어진다 - 그 비율을 우리가 쓰지 않고 게임이 계산하기 때문이다.
    [Theory]
    [InlineData(RaidExitStatus.Survived)]
    [InlineData(RaidExitStatus.Killed)]
    public void 레이드_횟수와_종료_상태_합이_언제나_짝이_맞는다(RaidExitStatus status)
    {
        var plan = RaidTallyPolicy.Decide(new RaidTally("p", status, 3, 100), 0, 0);

        var sessions = ValueOf(plan, "Sessions", "Pmc");
        var exits = plan.Increments
            .Where(i => i.Key.Count == 3 && i.Key[0] == "ExitStatus" && i.Key[2] == "Pmc")
            .Sum(i => i.Value);

        Assert.Equal(1, sessions);
        Assert.Equal(sessions, exits);
    }

    [Fact]
    public void 사살은_서브키_없는_맨_열쇠에_쌓인다()
    {
        var plan = RaidTallyPolicy.Decide(Survived(kills: 4), 0, 0);

        Assert.Equal(4, ValueOf(plan, "Kills"));
        Assert.False(Has(plan, "Kills", "Pmc"));
    }

    [Fact]
    public void 사살이_없으면_그_항목을_쓰지_않는다()
    {
        var plan = RaidTallyPolicy.Decide(Survived(kills: 0), 0, 0);

        Assert.False(Has(plan, "Kills"));
    }

    [Fact]
    public void 플레이시간은_카운터와_TotalInGameTime_양쪽에_같은_값이_간다()
    {
        var plan = RaidTallyPolicy.Decide(Survived(seconds: 813), 0, 0);

        Assert.Equal(813, ValueOf(plan, "LifeTime", "Pmc"));
        Assert.Equal(813, plan.TotalInGameTimeDelta);
    }

    [Fact]
    public void 연속_생존은_생환하면_하나_늘고_최장도_따라_올라간다()
    {
        var plan = RaidTallyPolicy.Decide(Survived(), currentWinStreak: 3, longestWinStreak: 3);

        Assert.Equal(4, plan.NewCurrentWinStreak);
        Assert.Equal(4, plan.NewLongestWinStreak);
    }

    [Fact]
    public void 연속_생존은_죽으면_0_이_되고_최장은_줄지_않는다()
    {
        var plan = RaidTallyPolicy.Decide(Killed(), currentWinStreak: 7, longestWinStreak: 12);

        Assert.Equal(0, plan.NewCurrentWinStreak);
        Assert.Equal(12, plan.NewLongestWinStreak);
    }

    [Fact]
    public void 들어온_연속_기록이_음수여도_셈이_어긋나지_않는다()
    {
        var plan = RaidTallyPolicy.Decide(Survived(), currentWinStreak: -5, longestWinStreak: -2);

        Assert.Equal(1, plan.NewCurrentWinStreak);
        Assert.Equal(1, plan.NewLongestWinStreak);
    }

    [Fact]
    public void 음수_사살은_거절한다()
    {
        var plan = RaidTallyPolicy.Decide(
            new RaidTally("p", RaidExitStatus.Survived, -1, 100), 2, 5);

        Assert.False(plan.Accepted);
        Assert.Empty(plan.Increments);
        Assert.Equal(0, plan.TotalInGameTimeDelta);
    }

    [Fact]
    public void 음수_생존시간은_거절한다()
    {
        var plan = RaidTallyPolicy.Decide(
            new RaidTally("p", RaidExitStatus.Survived, 1, -1), 0, 0);

        Assert.False(plan.Accepted);
        Assert.Empty(plan.Increments);
    }

    [Fact]
    public void 프로필_식별자가_비면_거절한다()
    {
        foreach (var id in new[] { null, "", "   " })
        {
            var plan = RaidTallyPolicy.Decide(
                new RaidTally(id, RaidExitStatus.Survived, 1, 100), 0, 0);

            Assert.False(plan.Accepted);
            Assert.Empty(plan.Increments);
        }
    }

    [Fact]
    public void 보고가_널이면_거절한다()
    {
        var plan = RaidTallyPolicy.Decide(null, 0, 0);

        Assert.False(plan.Accepted);
        Assert.Empty(plan.Increments);
    }

    // 열거 밖의 값을 캐스팅으로 넣는 길이 있다. 소거법으로 쓰면 그것이 조용히 '생환' 으로 쌓인다.
    [Fact]
    public void 알_수_없는_종료_상태는_거절한다()
    {
        var plan = RaidTallyPolicy.Decide(
            new RaidTally("p", (RaidExitStatus)99, 1, 100), 0, 0);

        Assert.False(plan.Accepted);
        Assert.Empty(plan.Increments);
        Assert.Contains("알 수 없는", plan.Reason);
    }

    // 거절이 프로필을 건드리는 길이 되면 안 된다 - 호출자가 돌려받은 연속 기록을 그대로 다시
    // 써도 아무것도 바뀌지 않아야 한다.
    [Fact]
    public void 거절은_연속_기록을_그대로_돌려준다()
    {
        var plan = RaidTallyPolicy.Decide(null, currentWinStreak: 3, longestWinStreak: 9);

        Assert.Equal(3, plan.NewCurrentWinStreak);
        Assert.Equal(9, plan.NewLongestWinStreak);
    }

    // 비율은 우리가 쓰지 않는다(설계 §5-8). 열거된 열쇠 말고 다른 것이 섞여 나오면 그 결정이
    // 깨진 것이다.
    [Fact]
    public void 비율이나_파생값을_열쇠로_내지_않는다()
    {
        var plan = RaidTallyPolicy.Decide(Killed(kills: 2, seconds: 300), 1, 1);

        var allowedFirstWords = new[] { "Sessions", "ExitStatus", "Kills", "Deaths", "LifeTime" };
        Assert.All(plan.Increments, i => Assert.Contains(i.Key[0], allowedFirstWords));
    }
}
