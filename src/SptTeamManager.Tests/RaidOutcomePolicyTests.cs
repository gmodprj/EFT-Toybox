using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class RaidOutcomePolicyTests
{
    [Fact]
    public void 실물_장비로_나갔다_죽으면_착용_장비를_지운다()
    {
        var plan = RaidOutcomePolicy.Decide(false, SpawnGearKind.Real, false);

        Assert.Equal(OutcomeAction.StripWornGear, plan.Action);
    }

    [Fact]
    public void 탈출했으면_아무것도_하지_않는다()
    {
        var plan = RaidOutcomePolicy.Decide(true, SpawnGearKind.Real, false);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("탈출", plan.Reason);
    }

    [Fact]
    public void 임시_장비였으면_죽어도_아무것도_하지_않는다()
    {
        var plan = RaidOutcomePolicy.Decide(false, SpawnGearKind.Temporary, false);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("임시", plan.Reason);
    }

    [Fact]
    public void 이미_적용된_키면_죽었어도_아무것도_하지_않는다()
    {
        var plan = RaidOutcomePolicy.Decide(false, SpawnGearKind.Real, true);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("이미 적용", plan.Reason);
    }

    [Fact]
    public void 어느_결정이든_사유가_담긴다()
    {
        foreach (var escaped in new[] { false, true })
        foreach (var applied in new[] { false, true })
        foreach (var kind in new SpawnGearKind?[] { SpawnGearKind.Real, SpawnGearKind.Temporary, null })
        {
            var plan = RaidOutcomePolicy.Decide(escaped, kind, applied);
            Assert.False(string.IsNullOrWhiteSpace(plan.Reason));
        }

        // 다섯 결과(적용됨·기록 없음·임시·탈출·삭제)는 서로 다른 사유를 가진다. 같은 문구를 나눠 쓰면 로그에서 규칙을 구별할 수 없다.
        var reasons = new[]
        {
            RaidOutcomePolicy.Decide(false, SpawnGearKind.Real, true).Reason,
            RaidOutcomePolicy.Decide(false, null, false).Reason,
            RaidOutcomePolicy.Decide(false, SpawnGearKind.Temporary, false).Reason,
            RaidOutcomePolicy.Decide(true, SpawnGearKind.Real, false).Reason,
            RaidOutcomePolicy.Decide(false, SpawnGearKind.Real, false).Reason,
        };
        Assert.Equal(5, reasons.Distinct().Count());
    }

    [Fact]
    public void 장비_기록이_없으면_죽었어도_아무것도_하지_않는다()
    {
        var plan = RaidOutcomePolicy.Decide(false, null, false);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("기록이 없", plan.Reason);
    }

    // 아래는 결과(Action)가 아니라 어느 규칙이 이겼는지(Reason)를 못박는다. 둘 다 Nothing 이라 Action 만으로는 순서가 드러나지 않는다.

    [Fact]
    public void 이미_적용된_키가_임시_장비이기도_하면_이미_적용_사유가_이긴다()
    {
        var plan = RaidOutcomePolicy.Decide(false, SpawnGearKind.Temporary, true);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("이미 적용", plan.Reason);
        Assert.DoesNotContain("임시", plan.Reason);
    }

    [Fact]
    public void 이미_적용된_키가_탈출했어도_이미_적용_사유가_이긴다()
    {
        var plan = RaidOutcomePolicy.Decide(true, SpawnGearKind.Real, true);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("이미 적용", plan.Reason);
        Assert.DoesNotContain("탈출", plan.Reason);
    }

    [Fact]
    public void 이미_적용된_키가_기록도_없으면_이미_적용_사유가_이긴다()
    {
        var plan = RaidOutcomePolicy.Decide(false, null, true);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("이미 적용", plan.Reason);
        Assert.DoesNotContain("기록이 없", plan.Reason);
    }

    [Fact]
    public void 임시_장비로_나갔다_탈출했으면_임시_장비_사유가_이긴다()
    {
        var plan = RaidOutcomePolicy.Decide(true, SpawnGearKind.Temporary, false);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("임시", plan.Reason);
        Assert.DoesNotContain("탈출", plan.Reason);
    }

    [Fact]
    public void 기록이_없고_탈출했으면_기록_없음_사유가_이긴다()
    {
        var plan = RaidOutcomePolicy.Decide(true, null, false);

        Assert.Equal(OutcomeAction.Nothing, plan.Action);
        Assert.Contains("기록이 없", plan.Reason);
        Assert.DoesNotContain("탈출", plan.Reason);
    }
}
