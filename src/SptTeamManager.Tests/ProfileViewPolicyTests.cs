using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 등록을 읽을 수 없는 동안 프로필 조회가 무엇을 내보내고 무엇을 막는가. 막아야 하는 것은 "등록된 런처 프로필일 수 있는 Aid" 뿐이고,
// PIT 자기 용병은 STM 의 고장으로 못 보게 되면 안 된다.
public class ProfileViewPolicyTests
{
    // ---- 조회 결과의 분류: "읽다 실패" 와 "팀원이 아님이 확인됨" 은 같지 않다 ----
    [Fact]
    public void 읽다_실패하면_다른_신호가_뭐든_읽기_실패다()
    {
        // 해석기는 읽기에서 던지면 삼키고 사유 없이 false 를 준다. 그 모양(resolved=false, reason=null)이 확인된 비팀원과 같아 보여도
        // 읽기 실패가 앞선다 - 아니면 SPT 의 응답(다른 프로필의 인벤토리 전체)이 흘러간다.
        Assert.Equal(ProfileLookupOutcome.ReadFailed, ProfileViewPolicy.ClassifyLookup(readFailed: true, resolved: false, unavailableReason: null));
        Assert.Equal(ProfileLookupOutcome.ReadFailed, ProfileViewPolicy.ClassifyLookup(readFailed: true, resolved: true, unavailableReason: "CurrentSession"));
    }

    [Fact]
    public void 읽기가_성공했고_사유가_없으면_확인된_비팀원이다()
    {
        // 이 경우는 그대로 PIT 길로 흘러야 한다 - 평범한 조회를 막으면 안 된다.
        Assert.Equal(ProfileLookupOutcome.NonMember, ProfileViewPolicy.ClassifyLookup(readFailed: false, resolved: false, unavailableReason: null));
        Assert.Equal(ProfileLookupOutcome.NonMember, ProfileViewPolicy.ClassifyLookup(readFailed: false, resolved: false, unavailableReason: "  "));
    }

    [Fact]
    public void 읽기가_성공하고_해석됐으면_팀원이다()
    {
        Assert.Equal(ProfileLookupOutcome.Resolved, ProfileViewPolicy.ClassifyLookup(readFailed: false, resolved: true, unavailableReason: null));
    }

    [Fact]
    public void 읽기가_성공했고_사유가_있으면_쓸_수_없는_팀원이다()
    {
        Assert.Equal(ProfileLookupOutcome.Unavailable, ProfileViewPolicy.ClassifyLookup(readFailed: false, resolved: false, unavailableReason: "CurrentSession"));
    }

    // ---- 단일 분류가 길을 정한다: 런타임 읽기 실패도 시작 때 실패와 같은 길을 탄다 ----
    [Fact]
    public void 런타임_읽기_실패여도_PIT_용병이면_PIT_응답을_낸다()
    {
        // 4차의 퇴보를 고정한다: 읽기 실패를 곧바로 거절하면 PIT 용병 조회가 STM 의 고장으로 깨진다.
        Assert.Equal(ProfileViewRoute.ServePit, ProfileViewPolicy.RouteAfterLookup(ProfileLookupOutcome.ReadFailed, () => true));
    }

    [Fact]
    public void 런타임_읽기_실패이고_PIT_용병도_아니면_거절한다()
    {
        Assert.Equal(ProfileViewRoute.Refuse, ProfileViewPolicy.RouteAfterLookup(ProfileLookupOutcome.ReadFailed, () => false));
    }

    [Theory]
    [InlineData(ProfileLookupOutcome.NonMember)]
    [InlineData(ProfileLookupOutcome.Unavailable)]
    [InlineData(ProfileLookupOutcome.Resolved)]
    public void 읽기가_성공한_결과는_조회_결과를_그대로_따르고_PIT_에게_묻지_않는다(ProfileLookupOutcome outcome)
    {
        // 확인된 비팀원은 PIT 길로 그대로 흐르고(StmFirst 의 꼬리), 평소 순서는 건드리지 않는다.
        var asked = false;
        Assert.Equal(ProfileViewRoute.StmFirst, ProfileViewPolicy.RouteAfterLookup(outcome, () => { asked = true; return true; }));
        Assert.False(asked);
    }

    [Fact]
    public void 정의_밖_분류는_읽기_실패처럼_닫힌_쪽이다()
    {
        Assert.Equal(ProfileViewRoute.Refuse, ProfileViewPolicy.RouteAfterLookup((ProfileLookupOutcome)99, () => false));
    }

    [Fact]
    public void 읽기_실패_사유는_상태_이름이_아니다()
    {
        // 화면이 상태 안내로 잘못 읽지 않도록: 읽기 실패 신호는 어떤 상태 이름과도 다르다.
        Assert.False(ProfileStatePolicy.TryDescribeRefusal(ProfileViewPolicy.ReadFailedReason, out _));
    }

    [Fact]
    public void 등록을_읽을_수_있으면_평소_순서대로_STM_먼저다()
    {
        Assert.Equal(ProfileViewRoute.StmFirst, ProfileViewPolicy.Decide(registrationsUsable: true, () => true));
        Assert.Equal(ProfileViewRoute.StmFirst, ProfileViewPolicy.Decide(registrationsUsable: true, () => false));
    }

    [Fact]
    public void 등록을_읽을_수_있으면_PIT_에게_먼저_묻지_않는다()
    {
        // STM-before-PIT 순서의 고정: 평소 경로에서 PIT 조회가 먼저 실행되면 같은 Aid 의 보이지 않는 용병이 골라진다.
        var asked = false;
        ProfileViewPolicy.Decide(registrationsUsable: true, () => { asked = true; return true; });
        Assert.False(asked);
    }

    [Fact]
    public void 등록을_읽을_수_없어도_PIT_용병이면_PIT_응답을_낸다()
    {
        Assert.Equal(ProfileViewRoute.ServePit, ProfileViewPolicy.Decide(registrationsUsable: false, () => true));
    }

    [Fact]
    public void 등록을_읽을_수_없고_PIT_용병도_아니면_거절한다()
    {
        // 등록된 런처 프로필일 수 있다: SPT 의 응답(다른 프로필의 인벤토리 전체)을 내보내지 않는다.
        Assert.Equal(ProfileViewRoute.Refuse, ProfileViewPolicy.Decide(registrationsUsable: false, () => false));
    }

    [Fact]
    public void 등록을_읽을_수_없을_때_PIT_에_물을_수_없으면_거절한다()
    {
        Assert.Equal(ProfileViewRoute.Refuse, ProfileViewPolicy.Decide(registrationsUsable: false, null!));
    }

    [Fact]
    public void 등록을_읽을_수_없을_때만_PIT_에게_묻는다()
    {
        var asked = 0;
        ProfileViewPolicy.Decide(registrationsUsable: false, () => { asked++; return false; });
        Assert.Equal(1, asked);
    }
}
