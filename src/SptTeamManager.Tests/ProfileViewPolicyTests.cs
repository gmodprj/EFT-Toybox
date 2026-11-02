using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 등록을 읽을 수 없는 동안 프로필 조회가 무엇을 내보내고 무엇을 막는가. 막아야 하는 것은 "등록된 런처 프로필일 수 있는 Aid" 뿐이고,
// PIT 자기 용병은 STM 의 고장으로 못 보게 되면 안 된다.
public class ProfileViewPolicyTests
{
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
