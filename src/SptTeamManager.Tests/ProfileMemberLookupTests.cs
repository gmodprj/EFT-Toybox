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
    public void 같은_Aid_에_Ready_와_Empty_가_섞이면_Empty_가_막아_NotAvailable_이다()
    {
        // 같은 Aid 를 두 프로필이 쓰면 어느 쪽인지 알 수 없다. 더 막는 쪽이 이긴다.
        var a = Cand("p1", 1001, ProfileAvailability.Empty);
        var b = Cand("p2", 1001, ProfileAvailability.Ready);
        foreach (var order in new[] { new[] { a, b }, new[] { b, a } })
        {
            var r = ProfileMemberLookup.ByAid(1001, new[] { "p1", "p2" }, order);
            Assert.Equal(MemberLookupResult.NotAvailable, r.Result);
            Assert.Equal(ProfileAvailability.Empty, r.Availability);
        }
    }

    [Fact]
    public void 등록된_CurrentSession_후보는_NotAvailable_이고_ProfileId_와_사유가_담긴다()
    {
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1" }, new[] { Cand("p1", 1001, ProfileAvailability.CurrentSession) });
        Assert.Equal(MemberLookupResult.NotAvailable, r.Result);
        Assert.Equal("p1", r.ProfileId);
        Assert.Equal(ProfileAvailability.CurrentSession, r.Availability);
    }

    [Fact]
    public void 같은_Aid_의_한쪽이_CurrentSession_이면_Ready_쪽이_있어도_막힌다_순서와_무관()
    {
        var cur = Cand("p1", 1001, ProfileAvailability.CurrentSession);
        var rdy = Cand("p2", 1001, ProfileAvailability.Ready);
        foreach (var order in new[] { new[] { cur, rdy }, new[] { rdy, cur } })
        {
            var r = ProfileMemberLookup.ByAid(1001, new[] { "p1", "p2" }, order);
            Assert.Equal(MemberLookupResult.NotAvailable, r.Result);
            Assert.Equal(ProfileAvailability.CurrentSession, r.Availability);
        }
        // 현재 세션 프로필이 등록돼 있지 않아도(복사본만 등록) 같은 Aid 면 막힌다.
        var copy = ProfileMemberLookup.ByAid(1001, new[] { "p2" }, new[] { cur, rdy });
        Assert.Equal(MemberLookupResult.NotAvailable, copy.Result);
        Assert.Equal(ProfileAvailability.CurrentSession, copy.Availability);
    }

    [Fact]
    public void 같은_Aid_의_Ready_가_둘이면_어느_쪽도_고르지_않는다()
    {
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1", "p2" },
            new[] { Cand("p1", 1001, ProfileAvailability.Ready), Cand("p2", 1001, ProfileAvailability.Ready) });
        Assert.Equal(MemberLookupResult.NotAvailable, r.Result);
        Assert.Null(r.ProfileId);
        Assert.Equal(ProfileAvailability.Unreadable, r.Availability);
    }

    [Fact]
    public void 등록되지_않은_후보는_상태와_상관없이_NotAMember_이고_등록된_Ready_를_막지_않는다()
    {
        Assert.Equal(MemberLookupResult.NotAMember,
            ProfileMemberLookup.ByAid(1001, new[] { "p1" }, new[] { Cand("p9", 1001, ProfileAvailability.Empty) }).Result);
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1" },
            new[] { Cand("p9", 1001, ProfileAvailability.Empty), Cand("p1", 1001, ProfileAvailability.Ready) });
        Assert.Equal(MemberLookupResult.Ready, r.Result);
        Assert.Equal("p1", r.ProfileId);
    }

    [Fact]
    public void null_입력은_던지지_않고_NotAMember_다()
    {
        var cands = new[] { Cand("p1", 1001, ProfileAvailability.Ready) };
        Assert.Equal(MemberLookupResult.NotAMember, ProfileMemberLookup.ByAid(1001, null!, cands).Result);
        Assert.Equal(MemberLookupResult.NotAMember, ProfileMemberLookup.ByAid(1001, new[] { "p1" }, null!).Result);
        // 컬렉션 안의 null 항목은 건너뛰고 나머지는 정상 처리한다.
        var r = ProfileMemberLookup.ByAid(1001, new string[] { null!, "", "  ", "p1" }, new ProfileCandidate[] { null!, cands[0] });
        Assert.Equal(MemberLookupResult.Ready, r.Result);
        Assert.Equal("p1", r.ProfileId);
    }

    [Fact]
    public void 앞뒤_공백이_붙은_id_도_같은_프로필로_본다()
    {
        var r = ProfileMemberLookup.ByAid(1001, new[] { "  p1 " }, new[] { Cand(" p1", 1001, ProfileAvailability.Ready) });
        Assert.Equal(MemberLookupResult.Ready, r.Result);
        Assert.Equal("p1", r.ProfileId);
    }

    [Fact]
    public void 모르는_Availability_값은_사용_불가로_친다()
    {
        var r = ProfileMemberLookup.ByAid(1001, new[] { "p1" }, new[] { Cand("p1", 1001, (ProfileAvailability)99) });
        Assert.Equal(MemberLookupResult.NotAvailable, r.Result);
        Assert.Equal("p1", r.ProfileId);
        Assert.Equal(ProfileAvailability.Unreadable, r.Availability);
    }
}
