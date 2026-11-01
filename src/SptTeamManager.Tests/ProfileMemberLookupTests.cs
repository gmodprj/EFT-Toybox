using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class ProfileMemberLookupTests
{
    private static ProfileCandidate Cand(string id, int aid, ProfileAvailability a) =>
        new(id, aid, "닉" + id, a == ProfileAvailability.Ready ? 10 : 0, a == ProfileAvailability.Ready ? "Usec" : "", a);

    [Fact]
    public void 등록되고_Ready_인_Aid_는_ProfileId_를_돌려준다()
    {
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1" }, new[] { Cand("p1", 1001, ProfileAvailability.Ready) });
        Assert.Equal(MemberLookupResult.Ready, r.Result);
        Assert.Equal("p1", r.ProfileId);
        Assert.Equal(ProfileAvailability.Ready, r.Availability);
    }

    [Fact]
    public void 등록되지_않은_Aid_는_NotAMember_다()
    {
        var cands = new[] { Cand("p1", 1001, ProfileAvailability.Ready), Cand("p2", 1002, ProfileAvailability.Ready) };
        // p2 는 카탈로그에 있고 Ready 지만 등록되지 않았다. 등록 안 된 Aid 와 카탈로그에 없는 Aid 모두 해당.
        Assert.Equal(MemberLookupResult.NotAMember, ProfileMemberLookup.ByAid(1002, new[] { "p1" }, cands).Result);
        var none = ProfileMemberLookup.ByAid(9999, new[] { "p1" }, cands);
        Assert.Equal(MemberLookupResult.NotAMember, none.Result);
        Assert.Null(none.ProfileId);
        Assert.Null(none.Availability);
    }

    [Fact]
    public void 등록됐으나_Empty_인_후보는_NotAvailable_이고_사유가_담긴다()
    {
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1" }, new[] { Cand("p1", 1001, ProfileAvailability.Empty) });
        Assert.Equal(MemberLookupResult.NotAvailable, r.Result);
        Assert.Equal("p1", r.ProfileId);
        Assert.Equal(ProfileAvailability.Empty, r.Availability);

        var cur = ProfileMemberLookup.ByAid(1001, new[] { "p1" }, new[] { Cand("p1", 1001, ProfileAvailability.CurrentSession) });
        Assert.Equal(MemberLookupResult.NotAvailable, cur.Result);
        Assert.Equal(ProfileAvailability.CurrentSession, cur.Availability);
    }

    [Fact]
    public void Aid_가_0_이하면_NotAMember_다()
    {
        var cands = new[] { Cand("p1", 0, ProfileAvailability.Ready), Cand("p2", -5, ProfileAvailability.Ready) };
        var reg = new[] { "p1", "p2" };
        Assert.Equal(MemberLookupResult.NotAMember, ProfileMemberLookup.ByAid(0, reg, cands).Result);
        Assert.Equal(MemberLookupResult.NotAMember, ProfileMemberLookup.ByAid(-5, reg, cands).Result);
        Assert.Equal(MemberLookupResult.NotAMember, ProfileMemberLookup.ByAid(int.MinValue, reg, cands).Result);
    }

    [Fact]
    public void 같은_Aid_후보가_둘이면_Ready_인_쪽을_고른다()
    {
        var cands = new[] { Cand("p1", 1001, ProfileAvailability.Empty), Cand("p2", 1001, ProfileAvailability.Ready) };
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1", "p2" }, cands);
        Assert.Equal(MemberLookupResult.Ready, r.Result);
        Assert.Equal("p2", r.ProfileId);

        // 순서를 뒤집어도 같다.
        var rev = ProfileMemberLookup.ByAid(1001, new[] { "p1", "p2" }, new[] { cands[1], cands[0] });
        Assert.Equal("p2", rev.ProfileId);
    }
}
