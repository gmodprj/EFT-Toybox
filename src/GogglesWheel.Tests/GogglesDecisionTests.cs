using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class GogglesDecisionTests
{
    private static ToggleTarget[] Targets(int count)
    {
        var list = new ToggleTarget[count];
        for (var i = 0; i < count; i++)
        {
            list[i] = new ToggleTarget("id-" + i, i == 0 ? ToggleKind.NightVision : ToggleKind.FaceShield, "항목 " + i);
        }

        return list;
    }

    [Fact]
    public void Coti_modifier_stands_down()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: true, cotiModifierHeld: true);

        Assert.Equal(GogglesDecision.StandDown, decision);
    }

    [Fact]
    public void Empty_target_list_does_nothing()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(0), keyHeld: true, cotiModifierHeld: false);

        Assert.Equal(GogglesDecision.DoNothing, decision);
    }

    [Fact]
    public void Single_target_toggles_immediately()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(1), keyHeld: true, cotiModifierHeld: false);

        Assert.Equal(GogglesDecision.ToggleDefaultNow, decision);
    }

    [Fact]
    public void Programmatic_call_toggles_immediately()
    {
        // UI Fixes 가 나침반을 치운 뒤 되부르는 경우 — 그때는 키가 이미 떼어져 있다.
        // 여기서 휠을 띄우면 떼어질 일 없는 키를 기다리며 입력이 잠긴다.
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: false, cotiModifierHeld: false);

        Assert.Equal(GogglesDecision.ToggleDefaultNow, decision);
    }

    [Fact]
    public void Held_key_with_two_targets_watches()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: true, cotiModifierHeld: false);

        Assert.Equal(GogglesDecision.WatchForLongPress, decision);
    }

    [Fact]
    public void Null_target_list_does_nothing()
    {
        var decision = GogglesDecisionMaker.Decide(null, keyHeld: true, cotiModifierHeld: false);

        Assert.Equal(GogglesDecision.DoNothing, decision);
    }

    [Fact]
    public void Coti_modifier_wins_even_with_no_targets()
    {
        // 물러나는 것이 먼저다 — 우리가 목록을 못 만들었다고 COTI 입력을 삼키면 안 된다.
        var decision = GogglesDecisionMaker.Decide(Targets(0), keyHeld: true, cotiModifierHeld: true);

        Assert.Equal(GogglesDecision.StandDown, decision);
    }
}
