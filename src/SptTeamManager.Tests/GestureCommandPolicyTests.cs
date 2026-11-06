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
    [InlineData("take cover")]
    [InlineData(" Take Cover ")]
    public void 대소문자와_공백은_무시한다(string command)
    {
        Assert.NotNull(GestureCommandPolicy.Translate(command));
    }

    [Theory]
    [InlineData("F I R E")]
    [InlineData("HO LD FIRE")]
    [InlineData("HOLDFIRE")]
    public void 낱말_안의_공백은_메뉴_철자로_보지_않아_거절한다(string command)
    {
        // 메뉴 철자는 FIRE / HOLD FIRE / TAKE COVER. 낱말 사이 공백의 개수와 앞뒤 공백만 봐주고, 낱말을 쪼개거나 붙인 것은 받지 않는다.
        Assert.Null(GestureCommandPolicy.Translate(command));
    }

    [Fact]
    public void 한_낱말만으로는_다른_명령이_되지_않는다()
    {
        Assert.Null(GestureCommandPolicy.Translate("HOLD"));
        Assert.Null(GestureCommandPolicy.Translate("COVER"));
    }

    [Theory]
    [InlineData("Fire")]
    [InlineData("HoldFire")]
    [InlineData("GetInCover")]
    public void 번역하는_명령은_메뉴에_보인다(string phraseName)
    {
        // 메뉴 필터와 번역이 어긋나면 만든 명령이 눈에 보이지 않는다 (이 Task 의 첫 결함).
        Assert.True(GestureCommandPolicy.IsSupportedPhrase(phraseName));
        Assert.NotNull(GestureCommandPolicy.Translate(GestureCommandPolicy.MenuCommandForPhrase(phraseName)));
    }

    [Theory]
    [InlineData("Silence")]
    [InlineData("KnifesOnly")]
    [InlineData("Spreadout")]
    [InlineData("GetBack")]
    public void 처리하지_않는_넷은_숨겨진_채_번역되지_않는다(string phraseName)
    {
        Assert.False(GestureCommandPolicy.IsSupportedPhrase(phraseName));
        Assert.Null(GestureCommandPolicy.MenuCommandForPhrase(phraseName));
    }

    [Fact]
    public void 메뉴_목록에서_번역되는_것은_정확히_셋이다()
    {
        // 목록에 이름이 늘면 번역(또는 라우터 분기)도 함께 늘었는지 이 시험이 걸린다.
        var translated = System.Linq.Enumerable.Where(GestureCommandPolicy.CommandGroupPhrases,
            name => GestureCommandPolicy.Translate(GestureCommandPolicy.MenuCommandForPhrase(name)) != null);
        Assert.Equal(new[] { "Fire", "HoldFire", "GetInCover" }, translated);
        Assert.Equal(13, GestureCommandPolicy.CommandGroupPhrases.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fire")]
    [InlineData("FOLLOW ME")]
    public void 모르는_구문_이름은_지원되지_않는다(string? phraseName)
    {
        // 대소문자를 봐주지 않는다: 게임의 enum 이름 그대로만 허용한다.
        Assert.False(GestureCommandPolicy.IsSupportedPhrase(phraseName));
        Assert.Null(GestureCommandPolicy.MenuCommandForPhrase(phraseName));
    }
}
