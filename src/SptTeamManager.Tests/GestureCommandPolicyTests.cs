using pitTeam.Shared;
using Xunit;

public class GestureCommandPolicyTests
{
    [Fact]
    public void FIRE_는_교전_허가로_번역된다()
    {
        Assert.Equal(GestureCommandPolicy.WeaponsFree, GestureCommandPolicy.Translate("FIRE")?.Engagement);
    }

    [Fact]
    public void HOLD_FIRE_는_교전_금지로_번역된다()
    {
        Assert.Equal(GestureCommandPolicy.HoldFire, GestureCommandPolicy.Translate("HOLD FIRE")?.Engagement);
    }

    [Fact]
    public void TAKE_COVER_는_엄폐_지시로_번역된다()
    {
        Assert.Equal(GestureCommandPolicy.SeekCover, GestureCommandPolicy.Translate("TAKE COVER")?.Engagement);
    }

    [Fact]
    public void 세_명령은_서로_다른_행동으로_번역된다()
    {
        // 셋이 같은 값을 돌려주는 구현도 위의 개별 테스트는 통과한다.
        var all = new[]
        {
            GestureCommandPolicy.Translate("FIRE")?.Engagement,
            GestureCommandPolicy.Translate("HOLD FIRE")?.Engagement,
            GestureCommandPolicy.Translate("TAKE COVER")?.Engagement,
        };
        Assert.Equal(3, new System.Collections.Generic.HashSet<string?>(all).Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("SILENCE")]
    [InlineData("KNIVES ONLY")]
    [InlineData("SPREADOUT")]
    [InlineData("GET BACK")]
    [InlineData("FOLLOW ME")]
    [InlineData("FIRE AT WILL")]
    [InlineData("CEASE")]
    public void 모르는_명령은_널이다(string? command)
    {
        // 허용 목록이다. 모르는 명령을 가까운 행동으로 짐작해 돌려주면 팀원이 엉뚱하게 움직인다.
        Assert.Null(GestureCommandPolicy.Translate(command));
    }

    [Theory]
    [InlineData("fire")]
    [InlineData("  Fire  ")]
    [InlineData("hold fire")]
    [InlineData("Hold   Fire")]
    [InlineData("HOLDFIRE")]
    [InlineData("take cover")]
    [InlineData(" Take Cover ")]
    public void 대소문자와_공백은_무시한다(string command)
    {
        Assert.NotNull(GestureCommandPolicy.Translate(command));
    }

    [Fact]
    public void 공백을_지워도_다른_명령으로_넘어가지_않는다()
    {
        // 공백을 모두 지우는 정규화가 FIRE 를 HOLD FIRE 의 부분으로 읽으면 안 된다.
        Assert.Equal(GestureCommandPolicy.WeaponsFree, GestureCommandPolicy.Translate("F I R E")?.Engagement);
        Assert.Null(GestureCommandPolicy.Translate("HOLD"));
    }
}
