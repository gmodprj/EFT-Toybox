using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Server.Models;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace pitTeam.Server.Services;

// 레이드 종료 때 팀원 한 명의 사실이 **두 라우트로 갈라져** 온다.
//
//   /client/game/bot/followerprogress   → KillCount · RaidSeconds (클라이언트가 봇의
//                                          SessionCounters 에서 센 값. 레이드 id 는 없다)
//   …/teammate/raid-outcomes            → 레이드 id · 생사(escaped)
//
// 적립은 **생사 쪽에서 방아쇠를 당긴다** — 레이드 id 가 거기에만 있고(영수증의 열쇠다) 그쪽이
// 팀원마다 반드시 한 번 오는 신호이기 때문이다. 수치 쪽은 그때까지 여기에 담아 둔다.
// 수치가 **아직 안 왔으면 0 으로 적립한다**(레이드 횟수·생사·연속 기록은 그대로 쌓인다) -
// 한 판을 통째로 빠뜨리는 것보다 낫고, 그때 로그가 "수치 없이 적립했다" 고 말한다.
//
// **두 라우트의 실제 순서는 아직 재지 않았다.** 그 로그가 첫 레이드에서 그것을 알려 준다:
// 수치가 붙어 적립되면 수치가 먼저 온 것이고, 0 으로 적립된 뒤 "늦게 도착" 줄이 뜨면 반대다.
// 반대였으면 방아쇠를 수치 쪽으로 옮기면 된다(그때는 레이드 id 를 여기 담아 두는 모양이 된다).
// 추측으로 한쪽을 고르지 않는다.
[Injectable(InjectionType.Singleton)]
public class StmRaidTallyBuffer(ISptLogger<StmRaidTallyBuffer> logger)
{
    // 한 판의 수치. 담길 때의 시각을 함께 들고 있어 묵은 것을 버린다.
    // A5-3 부터 **경험치·스킬도 같이** 담는다 — 같은 보고(followerprogress)로 오므로 따로 담으면
    // 두 짝이 어긋날 수 있다. 쓰는 쪽은 여전히 둘이다(수치는 ApplyNumbers, 성장은 ApplyGrowth).
    private sealed record Half(
        int Kills,
        int RaidSeconds,
        double Experience,
        IReadOnlyList<FriendlyTeammateSkillProgressRequest>? Skills,
        DateTimeOffset At);

    // 세션+aid 가 열쇠다. 한 세션에서 레이드 종료는 한 번에 하나뿐이므로 레이드 id 는 필요 없다
    // (그리고 수치 쪽 payload 에는 그 값이 아예 없다).
    private readonly Dictionary<string, Half> pending = new(StringComparer.Ordinal);

    // 반대 방향. **2026-11-07 실측으로 필요해졌다** — 생사가 수치보다 먼저 처리된다(그 판 네 명
    // 모두 "수치 보고가 아직 오지 않아" 였다). 그래서 1단계가 수치 없이 쌓은 뒤, 뒤늦게 올
    // 수치가 어느 레이드의 것인지 알 수 있도록 **레이드 id 를 여기 담아 둔다.**
    private sealed record RaidRef(string RaidId, DateTimeOffset At);

    private readonly Dictionary<string, RaidRef> pendingRaids = new(StringComparer.Ordinal);

    private readonly object sync = new();

    // 묶여 있을 수 있는 시간. 레이드 종료의 두 요청은 같은 teardown 안에서 수백 ms 안에 오므로
    // 10분은 아주 넉넉하다. 이보다 오래 남은 것은 짝이 오지 않은 것이고, 다음 레이드의 수치로
    // 오해되면 안 된다.
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    // 담아 둘 수 있는 최대 개수. 넘으면 가장 묵은 것을 버린다 - 짝이 안 오는 보고가 쌓여
    // 서버가 돌아가는 동안 메모리를 먹는 길을 막는다.
    private const int MaxPending = 64;

    public void RecordNumbers(MongoId sessionId, string? accountId, int kills, int raidSeconds,
        double experience = 0, IReadOnlyList<FriendlyTeammateSkillProgressRequest>? skills = null)
    {
        if (string.IsNullOrWhiteSpace(accountId)) return;

        // 음수는 담지 않는다. 정책도 거절하지만, 담아 두면 그 거절이 레이드 전체의 적립을
        // 막는 쪽으로 번진다 - 여기서 0 으로 깎지도 않는다(깎으면 틀린 수치를 적립한다).
        if (kills < 0 || raidSeconds < 0)
        {
            logger.Warning($"[STM] 적립 수치: aid '{accountId}'(세션 '{sessionId}')의 보고에 음수가 있어 담지 않는다(사살 {kills}, 생존 {raidSeconds}초).");
            return;
        }

        lock (sync)
        {
            DropStale();
            pending[Key(sessionId, accountId)] = new Half(kills, raidSeconds, experience, skills, DateTimeOffset.UtcNow);
        }
    }

    // 생사 쪽이 부를 때. 담긴 수치가 있으면 꺼내 주고 비운다(한 판에 한 번만 쓰인다).
    // 없으면 (0, 0, false) - 호출자는 그래도 적립하고 로그에 그렇게 적는다.
    public bool TryTakeNumbers(MongoId sessionId, string? accountId, out int kills, out int raidSeconds) =>
        TryTakeNumbers(sessionId, accountId, out kills, out raidSeconds, out _, out _);

    public bool TryTakeNumbers(MongoId sessionId, string? accountId, out int kills, out int raidSeconds,
        out double experience, out IReadOnlyList<FriendlyTeammateSkillProgressRequest>? skills)
    {
        kills = 0;
        raidSeconds = 0;
        experience = 0;
        skills = null;
        if (string.IsNullOrWhiteSpace(accountId)) return false;

        lock (sync)
        {
            DropStale();
            var key = Key(sessionId, accountId);
            if (!pending.TryGetValue(key, out var half)) return false;
            pending.Remove(key);
            kills = half.Kills;
            raidSeconds = half.RaidSeconds;
            experience = half.Experience;
            skills = half.Skills;
            return true;
        }
    }

    // 1단계가 수치 없이 쌓았을 때, 그 판의 레이드 id 를 담아 둔다. 뒤늦게 올 수치가
    // **어느 판의 것인지** 알아야 멱등 열쇠를 만들 수 있다.
    public void RecordRaid(MongoId sessionId, string? accountId, string? raidId)
    {
        if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(raidId)) return;

        lock (sync)
        {
            DropStale();
            pendingRaids[Key(sessionId, accountId)] = new RaidRef(raidId!.Trim(), DateTimeOffset.UtcNow);
        }
    }

    // 수치 쪽이 부를 때. 담긴 레이드 id 가 있으면 꺼내 주고 비운다(한 판에 한 번만 쓰인다).
    public bool TryTakeRaid(MongoId sessionId, string? accountId, out string? raidId)
    {
        raidId = null;
        if (string.IsNullOrWhiteSpace(accountId)) return false;

        lock (sync)
        {
            DropStale();
            var key = Key(sessionId, accountId);
            if (!pendingRaids.TryGetValue(key, out var reference)) return false;
            pendingRaids.Remove(key);
            raidId = reference.RaidId;
            return true;
        }
    }

    private void DropStale()
    {
        var cutoff = DateTimeOffset.UtcNow - Ttl;
        var stale = pending.Where(kv => kv.Value.At < cutoff).Select(kv => kv.Key).ToList();
        foreach (var key in stale) pending.Remove(key);

        var staleRaids = pendingRaids.Where(kv => kv.Value.At < cutoff).Select(kv => kv.Key).ToList();
        foreach (var key in staleRaids) pendingRaids.Remove(key);

        while (pendingRaids.Count >= MaxPending)
        {
            var oldest = pendingRaids.OrderBy(kv => kv.Value.At).First().Key;
            pendingRaids.Remove(oldest);
            logger.Warning($"[STM] 적립 수치: 담아 둔 레이드 id 가 {MaxPending}개를 넘어 가장 묵은 것을 버렸다('{oldest}'). 수치가 오지 않는 판이 쌓이고 있다.");
        }

        while (pending.Count >= MaxPending)
        {
            var oldest = pending.OrderBy(kv => kv.Value.At).First().Key;
            pending.Remove(oldest);
            logger.Warning($"[STM] 적립 수치: 담아 둔 것이 {MaxPending}개를 넘어 가장 묵은 것을 버렸다('{oldest}'). 짝이 오지 않는 보고가 쌓이고 있다.");
        }
    }

    private static string Key(MongoId sessionId, string accountId) =>
        sessionId.ToString() + "/" + accountId.Trim();
}
