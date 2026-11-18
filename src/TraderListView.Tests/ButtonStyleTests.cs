using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class ButtonStyleTests
{
    [Theory]
    [InlineData(false, false, false, ButtonStyle.Default)]
    [InlineData(false, true, false, ButtonStyle.Hover)]
    [InlineData(false, true, true, ButtonStyle.Pressed)]
    [InlineData(true, false, false, ButtonStyle.Active)]
    [InlineData(true, true, false, ButtonStyle.Active)]   // 활성은 마우스 오버보다 앞선다 — 지금 상태가 늘 보이게
    [InlineData(true, true, true, ButtonStyle.Pressed)]   // 누르는 순간만은 눌림이 보인다
    public void Background_FollowsStatePriority(bool active, bool hover, bool pressed, string expected)
    {
        Assert.Equal(expected, ButtonStyle.Background(active, hover, pressed, disabled: false));
    }

    [Fact]
    public void Disabled_WinsOverEverything()
    {
        Assert.Equal(ButtonStyle.Disabled, ButtonStyle.Background(active: true, hover: true, pressed: true, disabled: true));
    }

    [Fact]
    public void Palette_MatchesProposal()
    {
        Assert.Equal("#202528", ButtonStyle.Default);
        Assert.Equal("#343B3F", ButtonStyle.Hover);
        Assert.Equal("#477D9D", ButtonStyle.Active);
        Assert.Equal("#454B4E", ButtonStyle.Border);
        Assert.Equal("#C5C9C7", ButtonStyle.Icon);
    }

    [Fact]
    public void IconColor_DimsWhenDisabled()
    {
        Assert.Equal(ButtonStyle.Icon, ButtonStyle.IconColor(disabled: false));
        Assert.Equal(ButtonStyle.IconDisabled, ButtonStyle.IconColor(disabled: true));
    }
}
