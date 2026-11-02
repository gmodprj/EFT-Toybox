using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 등록 정보 갱신의 규칙. 비자명한 규칙은 하나다: "손으로 고친 표시" 는 단방향이다.
// 되돌릴 수 있으면 "편집했는데 임시 장비가 입혀진다" 가 다시 열린다. 사용자의 되돌리는 길은 등록을 지우고 다시 넣는 것이다.
public class ProfileMemberUpdatePolicyTests
{
    private static StmProfileMemberFields Current(bool autoJoin = false, bool handEdited = false) =>
        new StmProfileMemberFields("profile-1", "Rifleman", autoJoin, handEdited);

    [Fact]
    public void 널인_필드는_그대로_둔다()
    {
        var current = Current(autoJoin: true, handEdited: true);

        var result = ProfileMemberUpdatePolicy.Apply(current, new StmProfileMemberChange(null, null));

        Assert.Equal(current, result);
    }

    [Fact]
    public void 지정한_필드만_바꾼다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(handEdited: true), new StmProfileMemberChange(true, null));

        Assert.True(result!.AutoJoin);
        Assert.True(result.HandEdited);

        // 반대 방향도: HandEdited 가 false 인 채로 AutoJoin 만 바꾸면 HandEdited 는 false 로 남는다.
        var other = ProfileMemberUpdatePolicy.Apply(Current(handEdited: false), new StmProfileMemberChange(true, null));
        Assert.True(other!.AutoJoin);
        Assert.False(other.HandEdited);
    }

    [Fact]
    public void 자동_합류를_끌_수_있다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(autoJoin: true), new StmProfileMemberChange(false, null));

        Assert.False(result!.AutoJoin);
    }

    [Fact]
    public void 손으로_고친_표시는_되돌리지_않는다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(handEdited: true), new StmProfileMemberChange(null, false));

        Assert.True(result!.HandEdited);
    }

    [Fact]
    public void 손으로_고친_표시는_세울_수_있다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(handEdited: false), new StmProfileMemberChange(null, true));

        Assert.True(result!.HandEdited);
    }

    [Fact]
    public void 프로필id는_변경_대상이_아니다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(), new StmProfileMemberChange(true, true));

        Assert.Equal("profile-1", result!.ProfileId);
    }

    // 전술은 2026-11-02 에 열었다: 그 전에는 저장만 되고 바꿀 길이 없었다.
    [Fact]
    public void 전술을_바꾼다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(), new StmProfileMemberChange(null, null, "Marksman"));

        Assert.Equal("Marksman", result!.Tactic);
    }

    [Fact]
    public void 전술이_없으면_그대로_둔다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(), new StmProfileMemberChange(true, null));

        Assert.Equal("Rifleman", result!.Tactic);
    }

    // 빈 값은 "지우기" 가 아니라 조용한 되돌리기다. 투입 때 기본 전술로 떨어지는 것을 사용자가 모른다.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void 빈_전술은_그대로_둔다(string blank)
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(), new StmProfileMemberChange(null, null, blank));

        Assert.Equal("Rifleman", result!.Tactic);
    }

    [Fact]
    public void 전술의_앞뒤_공백을_떼고_저장한다()
    {
        var result = ProfileMemberUpdatePolicy.Apply(Current(), new StmProfileMemberChange(null, null, "  SainMan  "));

        Assert.Equal("SainMan", result!.Tactic);
    }

    [Fact]
    public void 현재_등록이_없으면_널을_돌린다()
    {
        Assert.Null(ProfileMemberUpdatePolicy.Apply(null, new StmProfileMemberChange(true, true)));
    }

    [Fact]
    public void 변경이_없으면_현재를_그대로_돌린다()
    {
        var current = Current(autoJoin: true);

        Assert.Equal(current, ProfileMemberUpdatePolicy.Apply(current, null));
    }
}
