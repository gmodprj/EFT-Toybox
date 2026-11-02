using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 편집 가드의 길. 조회(ProfileViewPolicy)와 같은 PIT 먼저 탈출구를 가지되, 모를 때의 기본값은 거절이다.
public class ProfileEditPolicyTests
{
    [Fact]
    public void 확인된_비팀원은_PIT_로_보내고_PIT_에게_묻지_않는다()
    {
        var asked = false;
        var route = ProfileViewPolicy.RouteEditAfterLookup(ProfileLookupOutcome.NonMember, () => { asked = true; return true; });
        Assert.Equal(ProfileEditRoute.PassToPit, route);
        Assert.False(asked);
    }

    [Theory]
    [InlineData(ProfileLookupOutcome.Resolved)]
    [InlineData(ProfileLookupOutcome.Unavailable)]
    public void 프로필_팀원은_PIT_에게_묻지_않고_거절한다(ProfileLookupOutcome outcome)
    {
        var asked = false;
        var route = ProfileViewPolicy.RouteEditAfterLookup(outcome, () => { asked = true; return true; });
        Assert.Equal(ProfileEditRoute.RefuseProfileMember, route);
        Assert.False(asked);
    }

    [Fact]
    public void 읽기_실패여도_PIT_용병이면_PIT_로_보낸다()
    {
        // 릴리스 차단 조건: 문서 하나가 깨져도 PIT 용병만 쓰는 플레이어의 조작이 막히면 안 된다.
        Assert.Equal(ProfileEditRoute.PassToPit, ProfileViewPolicy.RouteEditAfterLookup(ProfileLookupOutcome.ReadFailed, () => true));
    }

    [Fact]
    public void 읽기_실패이고_PIT_용병도_아니면_거절한다()
    {
        Assert.Equal(ProfileEditRoute.RefuseUndetermined, ProfileViewPolicy.RouteEditAfterLookup(ProfileLookupOutcome.ReadFailed, () => false));
    }

    [Fact]
    public void 읽기_실패이고_PIT_에_물을_수_없으면_거절한다()
    {
        Assert.Equal(ProfileEditRoute.RefuseUndetermined, ProfileViewPolicy.RouteEditAfterLookup(ProfileLookupOutcome.ReadFailed, null!));
    }

    [Fact]
    public void 정의_밖_분류는_읽기_실패와_같이_닫힌_쪽이다()
    {
        Assert.Equal(ProfileEditRoute.RefuseUndetermined, ProfileViewPolicy.RouteEditAfterLookup((ProfileLookupOutcome)99, () => false));
        Assert.Equal(ProfileEditRoute.PassToPit, ProfileViewPolicy.RouteEditAfterLookup((ProfileLookupOutcome)99, () => true));
    }
}
