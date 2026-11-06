using System.Collections.Generic;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 출격 화면에서 분대에 자동으로 넣을 프로필 팀원을 가리는 규칙. 허용 목록이다: Ready 가 아닌 모든 값
// (모르는 enum 값 포함)은 담지 않는다. 담았다가 못 쓰는 팀원을 출격에 넣으면 클라이언트가 조용히 떨군다.
public class AutoJoinRosterPolicyTests
{
    // 이 정책은 전술을 보지 않는다. 값은 유효한 임의의 전술일 뿐 의미가 없다.
    private const string ArbitraryTactic = "Rifleman";

    private static StmProfileMemberFields Member(string id, bool autoJoin) => new(id, ArbitraryTactic, autoJoin, false);

    private static ProfileCandidate Candidate(string id, int aid, ProfileAvailability availability = ProfileAvailability.Ready) =>
        new(id, aid, "nick", 10, "Usec", availability);

    [Fact]
    public void AutoJoin_이_켜진_Ready_멤버의_aid_를_돌린다()
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true) },
            new[] { Candidate("p1", 1234) });

        Assert.Equal(new[] { "1234" }, result);
    }

    [Fact]
    public void AutoJoin_이_꺼지면_목록에_없다()
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", false) },
            new[] { Candidate("p1", 1234) });

        Assert.Empty(result);
    }

    [Fact]
    public void 현_세션_프로필은_auto_join_목록에_없다()
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true) },
            new[] { Candidate("p1", 1234, ProfileAvailability.CurrentSession) });

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(ProfileAvailability.Unreadable)]
    [InlineData(ProfileAvailability.Empty)]
    [InlineData((ProfileAvailability)99)]
    public void 읽을_수_없는_프로필은_목록에_없다(ProfileAvailability availability)
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true) },
            new[] { Candidate("p1", 1234, availability) });

        Assert.Empty(result);
    }

    [Fact]
    public void 등록은_됐지만_카탈로그에_없으면_목록에_없다()
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("gone", true) },
            new[] { Candidate("other", 1234) });

        Assert.Empty(result);
    }

    [Fact]
    public void 같은_aid_가_둘이면_한_번만_담는다()
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true), Member("p2", true) },
            new[] { Candidate("p1", 1234), Candidate("p2", 1234) });

        Assert.Equal(new[] { "1234" }, result);
    }

    [Fact]
    public void 입력이_널이면_빈_목록이다()
    {
        Assert.Empty(AutoJoinRosterPolicy.SelectAccountIds(null, null));
        Assert.Empty(AutoJoinRosterPolicy.SelectAccountIds(new[] { Member("p1", true) }, null));
        Assert.Empty(AutoJoinRosterPolicy.SelectAccountIds(null, new[] { Candidate("p1", 1234) }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void 양수가_아닌_aid_는_목록에_없다(int aid)
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true) },
            new[] { Candidate("p1", aid) });

        Assert.Empty(result);
    }

    [Fact]
    public void 카탈로그에_같은_프로필id_가_둘이면_첫_항목만_본다()
    {
        var firstReady = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true) },
            new[] { Candidate("p1", 1111), Candidate("p1", 2222, ProfileAvailability.Unreadable) });
        var firstUnreadable = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { Member("p1", true) },
            new[] { Candidate("p1", 1111, ProfileAvailability.Unreadable), Candidate("p1", 2222) });

        Assert.Equal(new[] { "1111" }, firstReady);
        Assert.Empty(firstUnreadable);
    }

    [Fact]
    public void 널_원소는_건너뛰고_나머지는_처리한다()
    {
        var result = AutoJoinRosterPolicy.SelectAccountIds(
            new[] { null!, Member("p1", true) },
            new[] { null!, Candidate("p1", 1234) });

        Assert.Equal(new[] { "1234" }, result);
    }
    // 2026-11-03: 상한이 없어 용병 넷 + 프로필 다섯이면 아홉이 따라붙었다. 사용자가 정한 규칙은
    // "프로필 먼저, 그 안에서 무작위. 남으면 용병에서 무작위" 다. 씨앗을 주면 결과가 정해지므로
    // 기계가 확인할 수 있다.
    [Fact]
    public void 상한이_0이면_전부_나간다()
    {
        var result = AutoJoinRosterPolicy.Compose(new[] { "1", "2" }, new[] { "3", "4", "5" }, 0, new Random(1));
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void 상한만큼만_자른다()
    {
        var result = AutoJoinRosterPolicy.Compose(new[] { "1", "2" }, new[] { "3", "4", "5" }, 4, new Random(1));
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void 프로필이_먼저_나가고_용병은_남는_자리만_채운다()
    {
        var profiles = new[] { "11", "12", "13" };
        var result = AutoJoinRosterPolicy.Compose(profiles, new[] { "21", "22" }, 3, new Random(7));
        Assert.Equal(3, result.Count);
        // 셋 다 프로필이다 - 차례는 섞이지만 용병이 끼어들지는 않는다.
        Assert.All(result, id => Assert.Contains(id, profiles));
    }

    [Fact]
    public void 프로필이_모자라면_용병이_채운다()
    {
        var result = AutoJoinRosterPolicy.Compose(new[] { "11" }, new[] { "21", "22", "23" }, 3, new Random(3));
        Assert.Equal(3, result.Count);
        Assert.Equal("11", result[0]);
    }

    [Fact]
    public void 같은_id_는_한_번만_나가고_프로필_쪽이_이긴다()
    {
        var result = AutoJoinRosterPolicy.Compose(new[] { "11" }, new[] { "11", "21" }, 0, new Random(5));
        Assert.Equal(2, result.Count);
        Assert.Equal("11", result[0]);
    }

    [Fact]
    public void 씨앗이_다르면_차례가_달라진다()
    {
        var ids = new[] { "1", "2", "3", "4", "5", "6", "7", "8" };
        var a = AutoJoinRosterPolicy.Compose(ids, null, 0, new Random(1));
        var b = AutoJoinRosterPolicy.Compose(ids, null, 0, new Random(2));
        Assert.NotEqual(a, b);
    }
}
