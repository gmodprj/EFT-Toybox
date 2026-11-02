using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 계획 4 Task 1. 사용자가 정한 것: 팀원의 빌드와 플레이어의 빌드를 **별도로 구분하여** 보여 준다.
// 그 구분을 꼬리표가 아니라 자료 구조가 지키는지, 고를 수 없는 빌드가 목록에 서지 않는지를 고정한다.
public class GearBuildListPolicyTests
{
    private static GearBuildCandidate Candidate(string id, string? name = "빌드", int itemCount = 3) =>
        new(id, name, itemCount);

    [Fact]
    public void 두_묶음이_서로_섞이지_않는다()
    {
        var result = GearBuildListPolicy.Build(
            [Candidate("t1", "팀원 것")],
            [Candidate("p1", "플레이어 것")]);

        Assert.Equal(["팀원 것"], result.TeammateBuilds.Select(r => r.Name));
        Assert.Equal(["플레이어 것"], result.PlayerBuilds.Select(r => r.Name));
    }

    [Fact]
    public void 아이템이_없는_빌드는_고를_수_없으므로_뺀다()
    {
        var result = GearBuildListPolicy.Build([Candidate("t1", "빈 것", itemCount: 0)], null);

        Assert.Empty(result.TeammateBuilds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void id가_없는_빌드는_뺀다(string? id)
    {
        var result = GearBuildListPolicy.Build([new GearBuildCandidate(id!, "이름", 3)], null);

        Assert.Empty(result.TeammateBuilds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 이름이_없으면_부를_말을_준다(string? name)
    {
        var result = GearBuildListPolicy.Build([Candidate("t1", name)], null);

        Assert.Equal(GearBuildListPolicy.UnnamedBuild, Assert.Single(result.TeammateBuilds).Name);
    }

    [Fact]
    public void 같은_프로필_안에서_id가_겹치면_처음_것만_남긴다()
    {
        var result = GearBuildListPolicy.Build(
            [Candidate("same", "처음"), Candidate("same", "나중")],
            null);

        Assert.Equal("처음", Assert.Single(result.TeammateBuilds).Name);
    }

    // 프로필을 복사해 만들면 같은 id 가 양쪽에 있을 수 있다. 묶음이 나뉘어 있으므로 둘 다 남는다.
    [Fact]
    public void 두_프로필_사이의_id_겹침은_둘_다_남긴다()
    {
        var result = GearBuildListPolicy.Build(
            [Candidate("same", "팀원 것")],
            [Candidate("same", "플레이어 것")]);

        Assert.Single(result.TeammateBuilds);
        Assert.Single(result.PlayerBuilds);
    }

    [Fact]
    public void 이름순으로_세우고_같은_이름은_id로_가른다()
    {
        var result = GearBuildListPolicy.Build(
            [Candidate("b", "같은 이름"), Candidate("a", "같은 이름"), Candidate("c", "가장 먼저")],
            null);

        Assert.Equal(["가장 먼저", "같은 이름", "같은 이름"], result.TeammateBuilds.Select(r => r.Name));
        Assert.Equal(["c", "a", "b"], result.TeammateBuilds.Select(r => r.Id));
    }

    [Fact]
    public void 목록이_없으면_빈_묶음을_돌린다()
    {
        var result = GearBuildListPolicy.Build(null, null);

        Assert.Empty(result.TeammateBuilds);
        Assert.Empty(result.PlayerBuilds);
    }

    [Fact]
    public void 널_항목은_건너뛴다()
    {
        var result = GearBuildListPolicy.Build([null!, Candidate("t1")], null);

        Assert.Single(result.TeammateBuilds);
    }

    [Fact]
    public void id의_앞뒤_공백을_떼고_쓴다()
    {
        var result = GearBuildListPolicy.Build([Candidate("  t1  ")], null);

        Assert.Equal("t1", Assert.Single(result.TeammateBuilds).Id);
    }
}
