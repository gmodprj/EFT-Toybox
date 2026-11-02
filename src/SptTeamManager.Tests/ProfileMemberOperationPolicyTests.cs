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
    public void 키트_구매_사유는_출처가_그_팀원_스태시임을_말한다()
    {
        // 계획 3 Task 6 결정: 프로필 팀원에게 키트를 사 주는 길은 두지 않는다 - 플레이어 돈으로
        // 산 장비를 다른 프로필에 넣는 것은 프로필 두 개를 오가는 전송이고(설계 §6-5) 그것은 미뤘다.
        var reason = ProfileMemberOperationPolicy.RefusalReason(ProfileMemberOperation.BuyKit);
        Assert.Contains("스태시", reason);
    }

    [Fact]
    public void 장비_수리_사유는_그_프로필로_로그인해서_하라고_말한다()
    {
        Assert.Contains("로그인", ProfileMemberOperationPolicy.RefusalReason(ProfileMemberOperation.RepairEquipment));
    }

    [Fact]
    public void 용병_장비_저장_경로의_사유는_출처가_틀렸음을_말한다()
    {
        // PIT 의 default-equipment 는 플레이어 스태시를 출처로 쓴다. 프로필 팀원에게는 §6-4 위반이다.
        var reason = ProfileMemberOperationPolicy.RefusalReason(ProfileMemberOperation.DefaultEquipmentCommit);
        Assert.Contains("플레이어 스태시", reason);
        Assert.Contains("그 팀원 자신의 스태시", reason);
        // 다른 조작의 사유와 섞이지 않는다 - 화면에 엉뚱한 말이 나오면 플레이어가 길을 못 찾는다.
        Assert.NotEqual(ProfileMemberOperationPolicy.RefusalReason(ProfileMemberOperation.Loadout), reason);
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
    public void 팀원인지_확인할_수_없을_때도_거부하고_사유를_말한다(ProfileMemberOperation operation)
    {
        // 판별 실패는 "프로필 팀원이 아님" 이 아니다. 같은 Aid 의 보이지 않는 용병이 바뀌지 않도록 거부 쪽으로 닫는다.
        var reason = ProfileMemberOperationPolicy.UndeterminedReason(operation);
        Assert.Contains("확인할 수 없어", reason);
        Assert.NotEqual(ProfileMemberOperationPolicy.RefusalReason(operation), reason);
    }

    [Theory]
    [MemberData(nameof(AllOperations))]
    public void 사유는_센티넬_문자열이나_예외_이름이_아닌_사람의_말이다(ProfileMemberOperation operation)
    {
        var reason = ProfileMemberOperationPolicy.RefusalReason(operation);
        Assert.DoesNotContain("Exception", reason);
        Assert.DoesNotContain("Stm", reason);
        Assert.EndsWith(".", reason);
        Assert.Matches("[가-힣]{2,}", reason);
    }
}
