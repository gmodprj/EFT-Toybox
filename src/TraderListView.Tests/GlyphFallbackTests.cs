using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class GlyphFallbackTests
{
    private static bool HasAll(int codepoint) => true;

    private static bool HasNoSymbols(int codepoint) => codepoint < 0x2000 || (codepoint >= 0xAC00 && codepoint <= 0xD7A3);

    [Fact]
    public void AllGlyphsPresent_Unchanged()
    {
        Assert.Equal("LL3 🔒", GlyphFallback.Replace("LL3 🔒", HasAll));
        Assert.Equal("⇄ 볼트 ×3", GlyphFallback.Replace("⇄ 볼트 ×3", HasAll));
    }

    [Fact]
    public void MissingLock_BecomesKoreanWord()
    {
        Assert.Equal("LL3 (잠김)", GlyphFallback.Replace("LL3 🔒", HasNoSymbols));
    }

    [Fact]
    public void MissingSymbols_AllReplaced()
    {
        Assert.Equal("교환 물물교환 2종", GlyphFallback.Replace("⇄ 물물교환 2종", HasNoSymbols));
        Assert.Equal("가격 ^", GlyphFallback.Replace("가격 ▲", HasNoSymbols));
        Assert.Equal("가격 v", GlyphFallback.Replace("가격 ▼", HasNoSymbols));
        Assert.Equal("격자", GlyphFallback.Replace("▦", HasNoSymbols));
        Assert.Equal("목록", GlyphFallback.Replace("☰", HasNoSymbols));
    }

    [Fact]
    public void UnknownMissingGlyph_LeftAsIs()
    {
        Assert.Equal("★", GlyphFallback.Replace("★", HasNoSymbols));
    }

    [Fact]
    public void NullOrEmpty_ReturnsSame()
    {
        Assert.Null(GlyphFallback.Replace(null, HasNoSymbols));
        Assert.Equal(string.Empty, GlyphFallback.Replace(string.Empty, HasNoSymbols));
    }
}
