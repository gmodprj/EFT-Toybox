using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 편집기가 사라진 뒤 도착한 저장 거절 사유를 지금 이 화면에 보여 줘도 되는가. 틀린 팀원의 화면에 뜨는 사유는 사유를 잃는 것보다 나쁘고,
// 오래된 사유는 이미 다시 편집해 해결된 일을 되살린다. 그래서 주인이 같고 아직 신선할 때만 보인다.
public class PendingRefusalPolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 2, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void 주인이_같고_신선하면_보인다()
    {
        Assert.Equal(PendingRefusalVerdict.Show,
            PendingRefusalPolicy.Judge("aid-1", Now.AddMinutes(-3), "aid-1", Now));
    }

    [Fact]
    public void 열_분이_넘으면_만료다()
    {
        Assert.Equal(PendingRefusalVerdict.Expired,
            PendingRefusalPolicy.Judge("aid-1", Now - PendingRefusalPolicy.MaxAge - TimeSpan.FromSeconds(1), "aid-1", Now));
    }

    [Fact]
    public void 정확히_만료_시간이면_아직_보인다()
    {
        Assert.Equal(PendingRefusalVerdict.Show,
            PendingRefusalPolicy.Judge("aid-1", Now - PendingRefusalPolicy.MaxAge, "aid-1", Now));
    }

    [Fact]
    public void 시계가_뒤로_가서_시각이_미래여도_사유를_잃지_않는다()
    {
        Assert.Equal(PendingRefusalVerdict.Show,
            PendingRefusalPolicy.Judge("aid-1", Now.AddMinutes(5), "aid-1", Now));
    }

    [Fact]
    public void 다른_팀원의_화면에는_보이지_않는다_만료_여부와_무관하다()
    {
        Assert.Equal(PendingRefusalVerdict.NotOwner, PendingRefusalPolicy.Judge("aid-1", Now, "aid-2", Now));
        Assert.Equal(PendingRefusalVerdict.NotOwner, PendingRefusalPolicy.Judge("aid-1", Now.AddHours(-5), "aid-2", Now));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "aid-1")]
    [InlineData("aid-1", null)]
    [InlineData("", "")]
    [InlineData("  ", "  ")]
    public void 주인이나_화면의_팀원을_모르면_맞는_것으로_치지_않는다(string? owner, string? viewer)
    {
        Assert.Equal(PendingRefusalVerdict.NotOwner, PendingRefusalPolicy.Judge(owner, Now, viewer, Now));
        Assert.False(PendingRefusalPolicy.SameOwner(owner, viewer));
    }

    [Fact]
    public void 같은_주인_비교는_대소문자를_구분한다()
    {
        Assert.True(PendingRefusalPolicy.SameOwner("aid-1", "aid-1"));
        Assert.False(PendingRefusalPolicy.SameOwner("aid-1", "AID-1"));
    }
}
