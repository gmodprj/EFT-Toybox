using AirdropCaller.Server;
using Xunit;

namespace AirdropCaller.Tests;

/// <summary>
/// 빗장 판정만 본다. 조립·격자는 서버 어셈블리가 필요해 여기서 보지 않는다.
///
/// 핵심은 하나다 — **마른 조립은 예약을 지키려고 있는 빗장을 지나간다.** 2026-11-08 에
/// 레이드 중 시험 삼아 쏜 예약이 18초 뒤 실물이 되어 떨어졌고(설계 §20.4), 그 뒤로
/// "조립만 보기" 를 레이드 밖에서도 할 수 있어야 한다.
/// </summary>
public class QueueGateTests
{
    /// <summary>실제 호출이 통과하는 상태 — 여기서 한 칸씩 어긋나게 해 본다.</summary>
    private static QueueGateState Ready() => new()
    {
        ModEnabled = true,
        GameConnected = true,
        InRaid = true,
        CanSummon = true,
        AvailablePoints = 3,
        CrateInFlight = false,
        HasPending = false,
        CooldownSeconds = 0,
    };

    [Fact]
    public void 다_갖춰지면_부를_수_있다()
    {
        Assert.Null(QueueGate.WhyBlocked(Ready()));
    }

    [Theory]
    [InlineData("게임이 안 켜져 있다.")]
    public void 게임이_꺼져_있으면_막는다(string expected)
    {
        Assert.Equal(expected, QueueGate.WhyBlocked(Ready() with { GameConnected = false }));
    }

    [Fact]
    public void 레이드_밖이면_막는다()
    {
        Assert.Equal("레이드 중이 아니다.", QueueGate.WhyBlocked(Ready() with { InRaid = false }));
    }

    [Fact]
    public void 보급_지점이_없으면_막는다()
    {
        Assert.Equal("쓸 수 있는 보급 지점이 없다.", QueueGate.WhyBlocked(Ready() with { AvailablePoints = 0 }));
    }

    [Fact]
    public void 이미_예약이_있으면_막는다()
    {
        Assert.Equal("이미 예약된 드롭이 있다.", QueueGate.WhyBlocked(Ready() with { HasPending = true }));
    }

    [Fact]
    public void 쿨다운은_남은_초를_말해_준다()
    {
        Assert.Equal("쿨다운 42초 남았다.", QueueGate.WhyBlocked(Ready() with { CooldownSeconds = 42 }));
    }

    [Fact]
    public void 꺼진_모드는_마른_조립도_막는다()
    {
        var off = Ready() with { ModEnabled = false };

        Assert.Equal("모드가 설정에서 꺼져 있다.", QueueGate.WhyDryRunBlocked(off));
        Assert.Equal("모드가 설정에서 꺼져 있다.", QueueGate.WhyBlocked(off, dryRun: true));
    }

    [Fact]
    public void 마른_조립은_게임이_꺼져_있어도_된다()
    {
        var closed = Ready() with { GameConnected = false, InRaid = false, AvailablePoints = 0 };

        Assert.NotNull(QueueGate.WhyBlocked(closed));          // 부르는 쪽은 막힌다
        Assert.Null(QueueGate.WhyDryRunBlocked(closed));       // 조립만 보는 쪽은 통과한다
    }

    [Fact]
    public void 마른_조립은_쿨다운과_예약을_지나간다()
    {
        var busy = Ready() with { HasPending = true, CooldownSeconds = 99, CrateInFlight = true };

        Assert.NotNull(QueueGate.WhyBlocked(busy));
        Assert.Null(QueueGate.WhyBlocked(busy, dryRun: true));
    }

    [Fact]
    public void 마른_조립이_아니면_같은_판정을_쓴다()
    {
        foreach (var s in new[]
                 {
                     Ready(),
                     Ready() with { InRaid = false },
                     Ready() with { CooldownSeconds = 7 },
                     Ready() with { ModEnabled = false },
                 })
        {
            Assert.Equal(QueueGate.WhyBlocked(s), QueueGate.WhyBlocked(s, dryRun: false));
        }
    }
}
