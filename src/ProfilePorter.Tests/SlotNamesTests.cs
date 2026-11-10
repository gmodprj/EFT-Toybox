using ProfilePorter.Server;
using Xunit;

namespace ProfilePorter.Tests;

/// <summary>
/// 슬롯 이름 사전. **모르는 것은 영문 그대로** 돌려주는 것이 이 사전의 계약이다 —
/// 글자를 지어내거나 빈 칸으로 만들면 화면에서 그 자리가 사라진 것처럼 보인다.
/// </summary>
public class SlotNamesTests
{
    [Fact]
    public void 장비_열넷은_한국어다()
    {
        Assert.Equal("모자", SlotNames.Korean("Headwear"));
        Assert.Equal("리그", SlotNames.Korean("TacticalVest"));
        Assert.Equal("칼집", SlotNames.Korean("Scabbard"));
        Assert.Equal("가방", SlotNames.Korean("Backpack"));
    }

    [Fact]
    public void 안쪽_슬롯도_한국어다()
    {
        Assert.Equal("탄창", SlotNames.Korean("mod_magazine"));
        Assert.Equal("약실", SlotNames.Korean("patron_in_weapon"));
        Assert.Equal("전면 플레이트", SlotNames.Korean("Front_plate"));
    }

    [Fact]
    public void 모르는_것은_영문_그대로다()
    {
        Assert.Equal("mod_wibble", SlotNames.Korean("mod_wibble"));
        Assert.False(SlotNames.Knows("mod_wibble"));
        Assert.True(SlotNames.Knows("Headwear"));
    }

    [Fact]
    public void 빈_것은_빈_글자다()
    {
        Assert.Equal(string.Empty, SlotNames.Korean(null));
        Assert.Equal(string.Empty, SlotNames.Korean(string.Empty));
        Assert.False(SlotNames.Knows(null));
    }
}
