using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// auto-join 토글은 해석기가 Ready 인 팀원만 받아들이므로, 그 밖의 상태에서는 거절이 맞다. 이 테스트가 못박는 것은
// 거절이 아니라 "플레이어가 왜 거절됐는지 읽을 수 있다" 는 쪽이다.
public class ProfileStatePolicyTests
{
    [Fact]
    public void Ready_는_토글을_바꿀_수_있고_알림이_없다()
    {
        var notice = ProfileStatePolicy.Describe(RosterRowStatus.Ready);
        Assert.True(notice.AutoJoinChangeable);
        Assert.Equal(string.Empty, notice.Message);
    }

    [Theory]
    [InlineData(RosterRowStatus.CurrentSession)]
    [InlineData(RosterRowStatus.Unreadable)]
    [InlineData(RosterRowStatus.Empty)]
    [InlineData(RosterRowStatus.Missing)]
    public void Ready_가_아니면_토글을_바꿀_수_없고_그_사실을_말한다(RosterRowStatus status)
    {
        var notice = ProfileStatePolicy.Describe(status);
        Assert.False(notice.AutoJoinChangeable);
        Assert.Contains("자동 합류를 바꿀 수 없습니다", notice.Message);
    }

    [Fact]
    public void 로그인한_프로필은_그_이유를_말한다()
    {
        Assert.Contains("로그인", ProfileStatePolicy.Describe(RosterRowStatus.CurrentSession).Message);
    }

    [Fact]
    public void 읽을_수_없는_프로필은_그_이유를_말한다()
    {
        Assert.Contains("읽을 수 없", ProfileStatePolicy.Describe(RosterRowStatus.Unreadable).Message);
    }

    [Fact]
    public void 정의_밖_상태는_읽을_수_없음으로_닫는다()
    {
        var notice = ProfileStatePolicy.Describe((RosterRowStatus)99);
        Assert.False(notice.AutoJoinChangeable);
        Assert.Contains("읽을 수 없", notice.Message);
    }

    [Theory]
    [InlineData("CurrentSession", true)]
    [InlineData("currentsession", true)]
    [InlineData(" Unreadable ", false)]
    public void 서버_사유_문자열을_로스터_와이어와_같은_어휘로_읽는다(string reason, bool expectsLogin)
    {
        var notice = ProfileStatePolicy.DescribeReason(reason);
        Assert.False(notice.AutoJoinChangeable);
        Assert.Equal(expectsLogin, notice.Message.Contains("로그인"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("무슨_값인지_모름")]
    public void 사유를_못_읽으면_사용할_수_없는_쪽으로_닫는다(string? reason)
    {
        var notice = ProfileStatePolicy.DescribeReason(reason);
        Assert.False(notice.AutoJoinChangeable);
        Assert.False(string.IsNullOrWhiteSpace(notice.Message));
    }

    [Fact]
    public void Ready_문자열이_사유로_와도_열어_주지_않는다()
    {
        // 사유 필드는 거절의 사유다. Ready 가 거기 실려 왔다면 서버가 이상한 것이므로 열어 주지 않는다.
        Assert.False(ProfileStatePolicy.DescribeReason("Ready").AutoJoinChangeable);
    }
}
