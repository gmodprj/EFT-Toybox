using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

/// <summary>플러그인이 2초마다 보내는 레이드 상태.</summary>
public record HeartbeatRequest : IRequestData
{
    [JsonPropertyName("inRaid")]
    public bool InRaid { get; set; }

    /// <summary>맵 id (`bigmap` 등). 레이드 밖이면 비어 있다.</summary>
    [JsonPropertyName("map")]
    public string? Map { get; set; }

    /// <summary>이 맵에서 보급을 부를 수 있는가(`CanSummonAirdrop`).</summary>
    [JsonPropertyName("canSummon")]
    public bool CanSummon { get; set; }

    /// <summary>지금 하늘에 우리 상자가 있는가.</summary>
    [JsonPropertyName("crateInFlight")]
    public bool CrateInFlight { get; set; }

    /// <summary>쓸 수 있는 보급 지점 수. 0 이면 불러도 "지점이 없다" 가 된다.</summary>
    [JsonPropertyName("availablePoints")]
    public int AvailablePoints { get; set; }
}

/// <summary>
/// 플러그인이 보고한 레이드 상태를 들고 있는다.
///
/// 게임이 꺼지면 보고가 끊긴다. 그래서 "마지막 보고가 언제였나" 를 함께 들고,
/// <see cref="Stale"/> 로 "게임이 안 켜져 있다" 를 구분한다 — 외부 창이 "레이드 아님" 과
/// "게임 안 켜짐" 을 다르게 보여 줘야 하기 때문이다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class RaidState
{
    private readonly object _lock = new();
    private HeartbeatRequest _last = new();
    private DateTime _lastAt = DateTime.MinValue;

    /// <summary>
    /// 이만큼 보고가 없으면 게임이 꺼진 것으로 본다.
    ///
    /// **느린 보고 주기보다 넉넉히 길어야 한다.** 창이 꺼져 있으면 플러그인은
    /// <see cref="Config.IdleHeartbeatSeconds"/>(10초)마다만 보고한다. 이 값이 그와
    /// 비슷하면, 게임이 멀쩡히 돌아가는데도 창이 "게임 안 켜짐" 과 레이드 상태를
    /// 오락가락하게 된다.
    /// </summary>
    public static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(30);

    public void Report(HeartbeatRequest report)
    {
        lock (_lock)
        {
            _last = report;
            _lastAt = DateTime.UtcNow;
        }
    }

    public (HeartbeatRequest State, bool Stale) Snapshot()
    {
        lock (_lock)
        {
            return (_last, DateTime.UtcNow - _lastAt > StaleAfter);
        }
    }
}
