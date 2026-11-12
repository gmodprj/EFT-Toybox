namespace AirdropCaller.Server;

/// <summary>
/// 예약을 막는 이유를 정하는 **유일한 자리**.
///
/// SPT 어셈블리를 쓰지 않는다 — 시험이 이 파일만 링크해 판정을 본다.
/// </summary>
public readonly record struct QueueGateState
{
    public bool ModEnabled { get; init; }

    public bool GameConnected { get; init; }

    public bool InRaid { get; init; }

    public bool CanSummon { get; init; }

    public int AvailablePoints { get; init; }

    public bool CrateInFlight { get; init; }

    public bool HasPending { get; init; }

    public int CooldownSeconds { get; init; }
}

public static class QueueGate
{
    /// <summary>
    /// 실제로 부를 때 못 누르는 이유를 문장으로. 아무 일도 안 일어나는데 이유를 모르는 것이
    /// 가장 나쁘다 — 외부 창은 이 문장을 그대로 띄운다.
    /// </summary>
    public static string? WhyBlocked(QueueGateState s)
    {
        if (!s.ModEnabled) return "모드가 설정에서 꺼져 있다.";
        if (!s.GameConnected) return "게임이 안 켜져 있다.";
        if (!s.InRaid) return "레이드 중이 아니다.";
        if (!s.CanSummon) return "이 맵에서는 보급기가 뜨지 않는다.";
        if (s.AvailablePoints <= 0) return "쓸 수 있는 보급 지점이 없다.";
        if (s.CrateInFlight) return "상자가 아직 하늘에 있다.";
        if (s.HasPending) return "이미 예약된 드롭이 있다.";
        if (s.CooldownSeconds > 0) return $"쿨다운 {s.CooldownSeconds}초 남았다.";
        return null;
    }

    /// <summary>
    /// **마른 조립**(조립만 하고 예약하지 않는 길)을 막는 이유.
    ///
    /// 위의 빗장은 전부 "예약이 실물이 되는 것" 을 지키려고 있는 것이다. 마른 조립은
    /// 예약하지 않고 쿨다운도 쓰지 않으므로, 게임이 꺼져 있어도·레이드 밖에서도·쿨다운 중에도
    /// 조립 결과를 볼 수 있다. 남기는 빗장은 **모드를 껐다** 하나뿐이다 — 그건 사람이 끈 것이라
    /// 조립도 하지 않는 쪽이 맞다.
    ///
    /// 2026-11-08 에 레이드 중 `queue` 를 시험 삼아 쏘자 18초 뒤 그 레이드에 상자가 실제로
    /// 떨어졌다(설계 §20.4). 그래서 조립과 예약을 떼어 놓는다.
    /// </summary>
    public static string? WhyDryRunBlocked(QueueGateState s)
    {
        return s.ModEnabled ? null : "모드가 설정에서 꺼져 있다.";
    }

    /// <summary>마른 조립이냐에 따라 맞는 빗장을 고른다.</summary>
    public static string? WhyBlocked(QueueGateState s, bool dryRun)
    {
        return dryRun ? WhyDryRunBlocked(s) : WhyBlocked(s);
    }
}
