using GogglesWheel;
using Xunit;

namespace GogglesWheel.Tests;

public class TimeoutDecisionTests
{
    [Fact]
    public void Releasing_the_key_while_alive_applies_what_was_pointed_at()
    {
        Assert.True(GogglesDecisionMaker.ShouldApplySelection(alive: true, timedOut: false));
    }

    [Fact]
    public void Dying_with_the_wheel_open_applies_nothing()
    {
        Assert.False(GogglesDecisionMaker.ShouldApplySelection(alive: false, timedOut: false));
    }

    [Fact]
    public void The_safety_timeout_applies_nothing()
    {
        // 시간이 다 돼 접히는 것은 "뗐다" 가 아니다. 그 사이 시점을 돌리면 커서가 끌려가
        // 손대지 않은 항목이 여닫힌다 — 15초 전에 손을 뗀 사람에게는 영문 모를 일이다.
        Assert.False(GogglesDecisionMaker.ShouldApplySelection(alive: true, timedOut: true));
    }

    [Fact]
    public void Dying_and_timing_out_at_once_applies_nothing()
    {
        Assert.False(GogglesDecisionMaker.ShouldApplySelection(alive: false, timedOut: true));
    }
}
