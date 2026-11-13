using System;
using System.Collections.Generic;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 편집기의 "꾸러미로 저장" 판정(순수). 2026-11-13 사용자 요청.
//
// 이 길은 아이템을 옮기지 않는다 — 빌드는 본뜬 것이라 총량 보존을 걱정할 자리가 없다.
// 그래서 시험이 지키는 것은 **빈 것을 담지 않는 것**과 **이름이 겹치지 않는 것**이다.
public class GearBuildSavePolicyTests
{
    private static readonly DateTime When = new(2026, 11, 13, 15, 30, 0);

    private static GearBuildSavePlan Decide(
        string? nickname = "Sarge",
        int equipped = 12,
        IReadOnlyCollection<string>? existing = null)
        => GearBuildSavePolicy.Decide(nickname, equipped, existing ?? [], When);

    [Fact]
    public void 이름은_팀원_이름과_시각으로_짓는다()
    {
        var plan = Decide();

        Assert.True(plan.Accepted);
        Assert.Equal("Sarge 11-13 15:30", plan.Name);
    }

    [Fact]
    public void 입고_있는_것이_없으면_거절한다()
    {
        var plan = Decide(equipped: 0);

        Assert.False(plan.Accepted);
        Assert.Equal(string.Empty, plan.Name);
    }

    [Fact]
    public void 같은_이름이_있으면_번호를_붙인다()
    {
        var plan = Decide(existing: ["Sarge 11-13 15:30"]);

        Assert.True(plan.Accepted);
        Assert.Equal("Sarge 11-13 15:30 (2)", plan.Name);
    }

    [Fact]
    public void 번호도_겹치면_다음_번호로_간다()
    {
        var plan = Decide(existing: ["Sarge 11-13 15:30", "Sarge 11-13 15:30 (2)"]);

        Assert.Equal("Sarge 11-13 15:30 (3)", plan.Name);
    }

    [Fact]
    public void 이름이_비면_팀원으로_적는다()
    {
        Assert.Equal("팀원 11-13 15:30", Decide(nickname: "   ").Name);
        Assert.Equal("팀원 11-13 15:30", Decide(nickname: null).Name);
    }

    [Fact]
    public void 이름이_길면_자른다()
    {
        var plan = Decide(nickname: new string('가', 80));

        Assert.True(plan.Accepted);
        Assert.True(plan.Name.Length <= GearBuildSavePolicy.MaxNameLength);
    }

    [Fact]
    public void 꾸러미가_가득_차면_거절한다()
    {
        var many = new List<string>();
        for (var i = 0; i < GearBuildSavePolicy.MaxBuilds; i++) many.Add($"빌드 {i}");

        var plan = Decide(existing: many);

        Assert.False(plan.Accepted);
        Assert.Contains("100", plan.Reason);
    }

    [Fact]
    public void 받아들일_때는_사유에_몇_개인지_적는다()
    {
        var plan = Decide(equipped: 7);

        Assert.True(plan.Accepted);
        Assert.Contains("7", plan.Reason);
    }
}
