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
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: true, cotiModifierHeld: true, runOriginal: true);

        Assert.Equal(GogglesDecision.StandDown, decision);
    }

    [Fact]
    public void Someone_else_already_claimed_this_press_stands_down()
    {
        // HarmonyX 는 앞의 prefix 가 false 를 돌려줘도 **나머지 prefix 를 전부 실행한다**.
        // UI Fixes 가 나침반을 치우며 false 를 돌려준 바로 그 호출에서 우리도 불린다.
        // __runOriginal 이 false 라는 건 누군가 이미 이 입력을 가져갔다는 뜻이다 — 손대면 두 번 토글된다.
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: true, cotiModifierHeld: false, runOriginal: false);

        Assert.Equal(GogglesDecision.StandDown, decision);
    }

    [Fact]
    public void Claimed_press_stands_down_even_with_a_single_target()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(1), keyHeld: true, cotiModifierHeld: false, runOriginal: false);

        Assert.Equal(GogglesDecision.StandDown, decision);
    }

    [Fact]
    public void Empty_target_list_does_nothing()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(0), keyHeld: true, cotiModifierHeld: false, runOriginal: true);

        Assert.Equal(GogglesDecision.DoNothing, decision);
    }

    [Fact]
    public void Single_target_toggles_immediately()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(1), keyHeld: true, cotiModifierHeld: false, runOriginal: true);

        Assert.Equal(GogglesDecision.ToggleDefaultNow, decision);
    }

    [Fact]
    public void Programmatic_call_toggles_immediately()
    {
        // UI Fixes 가 나침반을 치운 뒤 되부르는 경우 — 그때는 키가 이미 떼어져 있다.
        // 여기서 휠을 띄우면 떼어질 일 없는 키를 기다리며 입력이 잠긴다.
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: false, cotiModifierHeld: false, runOriginal: true);

        Assert.Equal(GogglesDecision.ToggleDefaultNow, decision);
    }

    [Fact]
    public void Held_key_with_two_targets_watches()
    {
        var decision = GogglesDecisionMaker.Decide(Targets(2), keyHeld: true, cotiModifierHeld: false, runOriginal: true);

        Assert.Equal(GogglesDecision.WatchForLongPress, decision);
    }

    [Fact]
    public void Null_target_list_does_nothing()
    {
        var decision = GogglesDecisionMaker.Decide(null, keyHeld: true, cotiModifierHeld: false, runOriginal: true);

        Assert.Equal(GogglesDecision.DoNothing, decision);
    }

    [Fact]
    public void Coti_modifier_wins_even_with_no_targets()
    {
        // 물러나는 것이 먼저다 — 우리가 목록을 못 만들었다고 COTI 입력을 삼키면 안 된다.
        var decision = GogglesDecisionMaker.Decide(Targets(0), keyHeld: true, cotiModifierHeld: true, runOriginal: true);

        Assert.Equal(GogglesDecision.StandDown, decision);
    }
}
