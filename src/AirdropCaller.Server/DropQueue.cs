using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Location;

namespace AirdropCaller.Server;

/// <summary>
/// 예약된 드롭 한 칸과, 떨어진 상자의 내용물.
///
/// **한 칸이다.** 이미 예약이 있거나 상자가 하늘에 있으면 거절한다 — 동시에 여러 개를
/// 띄워 동기화 객체를 이상한 상태로 만드는 것이 설계의 안티목표였다.
///
/// 조립은 **예약받는 순간** 끝낸다. 1단계에서 본 대로 전리품 요청은 상자가 생기자마자
/// 날아오므로(착지 때가 아니다), 그때 가서 만들면 늦다.
///
/// 메모리에만 둔다. 서버를 다시 켜면 비는 것이 맞다 — 레이드도 함께 끝났을 것이다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class DropQueue
{
    private readonly object _lock = new();

    /// <summary>플러그인이 아직 안 꺼내 간 예약. 없으면 null.</summary>
    private string? _pendingId;

    /// <summary>드롭 id → 담을 것. 전리품 요청이 올 때까지 들고 있는다.</summary>
    private readonly Dictionary<string, GetAirdropLootResponse> _built = new();

    /// <summary>이 시각 전에는 새 예약을 받지 않는다.</summary>
    private DateTime _cooldownUntil = DateTime.MinValue;

    /// <summary>들고 있는 상자가 너무 쌓이지 않게. 레이드 하나에 이 정도면 넉넉하다.</summary>
    private const int MaxBuilt = 16;

    public enum QueueResult
    {
        Accepted,
        AlreadyQueued,
        Cooldown,
    }

    public (QueueResult Result, string? DropId, int CooldownSeconds) Enqueue(
        GetAirdropLootResponse response, TimeSpan cooldown)
    {
        lock (_lock)
        {
            if (_pendingId is not null)
            {
                return (QueueResult.AlreadyQueued, null, 0);
            }

            var now = DateTime.UtcNow;
            if (now < _cooldownUntil)
            {
                return (QueueResult.Cooldown, null, (int)Math.Ceiling((_cooldownUntil - now).TotalSeconds));
            }

            var dropId = new MongoId().ToString();
            _pendingId = dropId;

            // 오래된 것부터 버린다. 안 쓰인 상자가 영원히 남을 이유가 없다.
            while (_built.Count >= MaxBuilt)
            {
                _built.Remove(_built.Keys.First());
            }

            _built[dropId] = response;
            _cooldownUntil = now + cooldown;
            return (QueueResult.Accepted, dropId, 0);
        }
    }

    /// <summary>
    /// 예약을 꺼내 간다. **꺼내면 사라진다** — 응답을 못 받아 두 번 떨어지는 것보다
    /// 한 번 안 떨어지는 쪽이 낫다.
    /// </summary>
    public string? Pop()
    {
        lock (_lock)
        {
            var id = _pendingId;
            _pendingId = null;
            return id;
        }
    }

    /// <summary>아직 안 꺼내 간 예약을 취소한다. 이미 떨어지는 상자는 건드리지 않는다.</summary>
    public bool Cancel()
    {
        lock (_lock)
        {
            if (_pendingId is null)
            {
                return false;
            }

            _built.Remove(_pendingId);
            _pendingId = null;
            _cooldownUntil = DateTime.MinValue;
            return true;
        }
    }

    /// <summary>레이드가 끝났거나 게임이 꺼졌을 때. 다음 레이드로 넘어가면 안 된다.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _pendingId = null;
            _built.Clear();
            _cooldownUntil = DateTime.MinValue;
        }
    }

    public GetAirdropLootResponse? TakeBuilt(string dropId)
    {
        lock (_lock)
        {
            return _built.TryGetValue(dropId, out var r) ? r : null;
        }
    }

    public (bool HasPending, int CooldownSeconds, int BuiltCount) Status()
    {
        lock (_lock)
        {
            var left = _cooldownUntil - DateTime.UtcNow;
            return (_pendingId is not null, left > TimeSpan.Zero ? (int)Math.Ceiling(left.TotalSeconds) : 0, _built.Count);
        }
    }
}
