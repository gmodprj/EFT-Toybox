using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class FriendListMergePolicyTests
{
    private static FriendEntry Native(string id, int aid) => new(id, aid, FriendKind.Native);
    private static FriendEntry Merc(string id, int aid) => new(id, aid, FriendKind.Mercenary);
    private static FriendEntry Member(string id, int aid) => new(id, aid, FriendKind.ProfileMember);

    [Fact]
    public void 같은_ProfileId_는_한_번만_들어간다()
    {
        var outcome = FriendListMergePolicy.Plan(
            new[] { Native("n1", 1) },
            new[] { Member("p1", 100), Member("p1", 100) });

        Assert.Single(outcome.Final, e => e.Id == "p1");
        Assert.Single(outcome.Skipped);
    }

    [Fact]
    public void Aid_가_같아도_종류가_다르면_프로필_팀원이_우선하고_용병이_밀린다()
    {
        // 용병이 먼저 들어와 있어도 진짜 프로필이 살아남아야 한다.
        var outcome = FriendListMergePolicy.Plan(
            Array.Empty<FriendEntry>(),
            new[] { Merc("m1", 555), Member("p1", 555) });

        Assert.Contains(outcome.Final, e => e.Id == "p1" && e.Kind == FriendKind.ProfileMember);
        Assert.DoesNotContain(outcome.Final, e => e.Id == "m1");
        var skipped = Assert.Single(outcome.Skipped);
        Assert.Equal("m1", skipped.Dropped.Id);
    }

    [Fact]
    public void Id_가_같으면_용병이_먼저_와도_프로필_팀원이_남고_용병이_밀린다()
    {
        var outcome = FriendListMergePolicy.Plan(
            Array.Empty<FriendEntry>(),
            new[] { Merc("same", 1), Member("same", 2) });

        var kept = Assert.Single(outcome.Final);
        Assert.Equal(FriendKind.ProfileMember, kept.Kind);
        var skipped = Assert.Single(outcome.Skipped);
        Assert.Equal(FriendKind.Mercenary, skipped.Dropped.Kind);
    }

    [Fact]
    public void 후보_ProfileId_의_공백도_등록_id_와_같게_본다()
    {
        var c = new ProfileCandidate(" bbbbbbbbbbbbbbbbbbbbbbbb ", 1, "R", 5, "Usec", ProfileAvailability.Ready);
        Assert.Single(ProfileRosterPolicy.SelectRegisteredReady(new[] { c }, new[] { "bbbbbbbbbbbbbbbbbbbbbbbb" }));
    }

    [Fact]
    public void Aid_가_0_이면_중복_판정에_쓰지_않는다()
    {
        var outcome = FriendListMergePolicy.Plan(
            new[] { Native("n1", 0) },
            new[] { Member("p1", 0), Merc("m1", 0) });

        Assert.Equal(3, outcome.Final.Count);
        Assert.Empty(outcome.Skipped);
    }

    [Fact]
    public void 밀려난_항목은_이유와_함께_보고된다()
    {
        var outcome = FriendListMergePolicy.Plan(
            new[] { Native("n1", 7) },
            new[] { Merc("m1", 7) });

        var skipped = Assert.Single(outcome.Skipped);
        Assert.Equal("m1", skipped.Dropped.Id);
        Assert.False(string.IsNullOrWhiteSpace(skipped.Reason));
    }

    [Fact]
    public void 네이티브_친구는_어떤_경우에도_제거되지_않는다()
    {
        var existing = new[] { Native("n1", 1), Native("n2", 2), Native("n3", 0) };
        var outcome = FriendListMergePolicy.Plan(
            existing,
            new[] { Member("n1", 9), Member("p2", 2), Merc("n2", 9), Merc("m3", 1) });

        foreach (var native in existing)
            Assert.Contains(native, outcome.Final);
        Assert.DoesNotContain(outcome.Skipped, s => s.Dropped.Kind == FriendKind.Native);
        Assert.Equal(existing, outcome.Final.Take(3).ToArray());
    }

    [Fact]
    public void 용병은_모두_남고_순서가_유지된다()
    {
        var outcome = FriendListMergePolicy.Plan(
            Array.Empty<FriendEntry>(),
            new[] { Merc("m1", 11), Member("p1", 21), Merc("m2", 12) });

        Assert.Equal(new[] { "m1", "p1", "m2" }, outcome.Final.Select(e => e.Id).ToArray());
        Assert.Empty(outcome.Skipped);
    }

    [Fact]
    public void Ready_가_아닌_후보와_등록되지_않은_후보는_병합_대상에_오르지_않는다()
    {
        const string ready = "bbbbbbbbbbbbbbbbbbbbbbbb";
        const string me = "aaaaaaaaaaaaaaaaaaaaaaaa";
        const string empty = "cccccccccccccccccccccccc";
        const string bad = "dddddddddddddddddddddddd";
        const string notRegistered = "eeeeeeeeeeeeeeeeeeeeeeee";
        const string deleted = "ffffffffffffffffffffffff";
        var candidates = new[]
        {
            new ProfileCandidate(ready, 1, "R", 5, "Usec", ProfileAvailability.Ready),
            new ProfileCandidate(me, 2, "Me", 5, "Usec", ProfileAvailability.CurrentSession),
            new ProfileCandidate(empty, 3, "E", 0, "", ProfileAvailability.Empty),
            new ProfileCandidate(bad, 4, "B", 0, "", ProfileAvailability.Unreadable),
            new ProfileCandidate(notRegistered, 5, "N", 5, "Usec", ProfileAvailability.Ready),
        };

        // deleted: 등록은 됐지만 카탈로그에 이제 없다.
        var result = ProfileRosterPolicy.SelectRegisteredReady(candidates, new[] { ready, me, empty, bad, deleted });

        var only = Assert.Single(result);
        Assert.Equal(ready, only.ProfileId);
    }
}
