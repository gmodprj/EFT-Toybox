using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class PartySlotPolicyTests
{
    [Fact]
    public void 팀원_자리는_셋이다()
    {
        Assert.Equal(3, PartySlotPolicy.MaxTeammateSlots);
        Assert.True(PartySlotPolicy.Admit(0, 2, FriendKind.ProfileMember).Allowed);
        Assert.False(PartySlotPolicy.Admit(0, 3, FriendKind.ProfileMember).Allowed);
        Assert.False(PartySlotPolicy.Admit(3, 0, FriendKind.Mercenary).Allowed);
    }

    [Fact]
    public void 용병_둘과_프로필_팀원_하나는_허용된다()
    {
        var d = PartySlotPolicy.Admit(2, 0, FriendKind.ProfileMember);
        Assert.True(d.Allowed);
        Assert.Null(d.RejectionReason);
        Assert.True(PartySlotPolicy.Admit(1, 1, FriendKind.Mercenary).Allowed);
    }

    [Fact]
    public void 넷째는_거부되고_사유가_담긴다()
    {
        foreach (var (m, p) in new[] { (2, 1), (1, 2), (3, 0), (0, 3) })
        {
            foreach (var kind in new[] { FriendKind.Mercenary, FriendKind.ProfileMember })
            {
                var d = PartySlotPolicy.Admit(m, p, kind);
                Assert.False(d.Allowed);
                Assert.False(string.IsNullOrWhiteSpace(d.RejectionReason));
            }
        }
    }

    [Fact]
    public void 거부는_조용히_자르지_않고_사유_문자열을_돌려준다()
    {
        var d = PartySlotPolicy.Admit(2, 1, FriendKind.ProfileMember);
        Assert.False(d.Allowed);
        Assert.Contains("3", d.RejectionReason);
        // A count that cannot be right is refused with a reason too, never admitted.
        var bad = PartySlotPolicy.Admit(-1, 0, FriendKind.Mercenary);
        Assert.False(bad.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(bad.RejectionReason));
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    [InlineData(2, 1)]
    public void 네이티브_합류는_팀원_자리를_쓰지_않아_가득_차도_허용되고_사유가_없다(int mercs, int profiles)
    {
        var d = PartySlotPolicy.Admit(mercs, profiles, FriendKind.Native);
        Assert.True(d.Allowed);
        Assert.Null(d.RejectionReason);
    }

    private const string UndeterminedMarker = "could not be determined";

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    public void 음수_수는_양쪽_어느_쪽이든_판단_불가_사유로_거부된다(int mercs, int profiles)
    {
        var d = PartySlotPolicy.Admit(mercs, profiles, FriendKind.Mercenary);
        Assert.False(d.Allowed);
        Assert.Contains(UndeterminedMarker, d.RejectionReason);
    }

    [Fact]
    public void 아주_큰_수의_합이_넘쳐도_거부된다()
    {
        var d = PartySlotPolicy.Admit(int.MaxValue, int.MaxValue, FriendKind.ProfileMember);
        Assert.False(d.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(d.RejectionReason));
    }
}
