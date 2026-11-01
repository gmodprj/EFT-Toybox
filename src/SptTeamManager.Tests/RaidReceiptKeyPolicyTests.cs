using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class RaidReceiptKeyPolicyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 레이드_ID가_비어_있으면_쓸_수_없는_키다(string? raidId)
    {
        Assert.False(RaidReceiptKeyPolicy.IsUsableKey(raidId, "profile1"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 프로필_ID가_비어_있으면_쓸_수_없는_키다(string? profileId)
    {
        Assert.False(RaidReceiptKeyPolicy.IsUsableKey("raid1", profileId));
    }

    [Fact]
    public void 둘_다_채워져_있으면_쓸_수_있는_키다()
    {
        Assert.True(RaidReceiptKeyPolicy.IsUsableKey("raid1", "profile1"));
    }

    [Theory]
    [InlineData(null, "p")]
    [InlineData("", "p")]
    [InlineData("  ", "p")]
    [InlineData("r", null)]
    [InlineData("r", "")]
    [InlineData("r", "  ")]
    public void 쓸_수_없는_키는_같은_키여도_일치하지_않는다(string? raidId, string? profileId)
    {
        Assert.False(RaidReceiptKeyPolicy.KeysMatch(raidId, profileId, raidId, profileId));
    }

    [Fact]
    public void 같은_레이드와_같은_프로필이면_일치한다()
    {
        Assert.True(RaidReceiptKeyPolicy.KeysMatch("raid1", "profile1", "raid1", "profile1"));
    }

    [Fact]
    public void 앞뒤_공백은_같은_키로_본다()
    {
        Assert.True(RaidReceiptKeyPolicy.KeysMatch(" raid1 ", "\tprofile1\n", "raid1", "profile1"));
        Assert.True(RaidReceiptKeyPolicy.KeysMatch("raid1", "profile1", " raid1", "profile1 "));
    }

    [Fact]
    public void 대소문자만_다르면_같은_키로_본다()
    {
        // 못 맞추면 "적용 안 됨"으로 읽혀 장비를 두 번 지우므로, 애매하면 같은 키 쪽으로 기운다.
        Assert.True(RaidReceiptKeyPolicy.KeysMatch("ABC123", "profile1", "abc123", "profile1"));
        Assert.True(RaidReceiptKeyPolicy.KeysMatch("raid1", "ABCDEF", "raid1", "abcdef"));
    }

    [Fact]
    public void 같은_레이드라도_프로필이_다르면_일치하지_않는다()
    {
        Assert.False(RaidReceiptKeyPolicy.KeysMatch("raid1", "profileA", "raid1", "profileB"));
    }

    [Fact]
    public void 같은_프로필이라도_레이드가_다르면_일치하지_않는다()
    {
        Assert.False(RaidReceiptKeyPolicy.KeysMatch("raid1", "profile1", "raid2", "profile1"));
    }

    [Theory]
    [InlineData(null, "p")]
    [InlineData("r", null)]
    public void 비교_대상_쪽의_키가_비어_있어도_일치하지_않고_예외도_없다(string? rightRaidId, string? rightProfileId)
    {
        Assert.False(RaidReceiptKeyPolicy.KeysMatch("r", "p", rightRaidId, rightProfileId));
    }
}
