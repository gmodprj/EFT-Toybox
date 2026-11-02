using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 런처 프로필은 PIT 용병이 아니다. 용병 전용 조작이 프로필 팀원에게 조용히 성공한 척하면
// 플레이어는 바뀌었다고 믿는데 아무것도 바뀌지 않았거나, 더 나쁘게는 같은 Aid 를 쓰는 용병이 바뀐다.
public class ProfileMemberOperationPolicyTests
{
    public static IEnumerable<object[]> AllOperations() =>
        Enum.GetValues<ProfileMemberOperation>().Select(o => new object[] { o });

    [Theory]
    [MemberData(nameof(AllOperations))]
    public void 모든_조작은_거부되고_사유가_비어_있지_않다(ProfileMemberOperation operation)
    {
        Assert.False(string.IsNullOrWhiteSpace(ProfileMemberOperationPolicy.RefusalReason(operation)));
    }

    [Fact]
    public void 이름_변경_사유는_그_프로필의_것임을_말한다()
    {
        Assert.Contains("그 프로필", ProfileMemberOperationPolicy.RefusalReason(ProfileMemberOperation.Rename));
    }

    [Fact]
    public void 외형_변경_사유는_그_프로필의_것임을_말한다()
    {
        Assert.Contains("그 프로필", ProfileMemberOperationPolicy.RefusalReason(ProfileMemberOperation.Suit));
    }

    [Fact]
    public void 모르는_조작도_거부한다()
    {
        // 열거에 없는 값은 막힌 쪽으로 닫는다. 빈 문자열을 돌려주면 호출자가 "거부할 사유 없음" 으로 읽을 수 있다.
        var reason = ProfileMemberOperationPolicy.RefusalReason((ProfileMemberOperation)999);
        Assert.False(string.IsNullOrWhiteSpace(reason));
    }

    [Theory]
    [MemberData(nameof(AllOperations))]
    public void 사유는_센티넬_문자열이나_예외_이름이_아닌_사람의_말이다(ProfileMemberOperation operation)
    {
        var reason = ProfileMemberOperationPolicy.RefusalReason(operation);
        Assert.DoesNotContain("Exception", reason);
        Assert.DoesNotContain("Stm", reason);
        Assert.Contains(" ", reason);
    }
}
