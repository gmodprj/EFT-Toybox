using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 팀원 한 명이 한 판을 어떻게 마쳤는가. **둘뿐이다** — 봇 팀원은 생환하거나 죽는다.
    // EFT 의 `Left`·`Runner`·`MissingInAction` 은 사람 플레이어의 종료 상태이고, 팀원에게는
    // 그런 보고가 오지 않는다(생사 판정은 FollowerDeathEscapeResolver 의 escaped 한 값이다).
    // 그래서 그 셋은 **일부러 0 으로 남는다** — 쓸 수 없는 값을 지어내 넣지 않는다.
    // 넓힐 자리가 생기면 doc/specs/2026-11-07-stm-raid-tally-mapping.md 의 표에 줄을 더하는 것이
    // 그 결정의 자리다.
    public enum RaidExitStatus { Survived, Killed }

    // 클라이언트가 세어 보내는 팀원 한 명의 한 판치. 비율은 들어오지 않는다(게임이 계산한다).
    public sealed record RaidTally(
        string? ProfileId,
        RaidExitStatus ExitStatus,
        int Kills,
        int LifeTimeSeconds);

    // OverallCounters 한 항목에 더할 값. Key 는 그 사전의 열쇠 배열 그대로다
    // (실측 모양: ["Sessions","Pmc"] · ["ExitStatus","Survived","Pmc"] · ["Kills"]).
    public sealed record CounterIncrement(IReadOnlyList<string> Key, long Value);

    // 적립 계획. Accepted 가 거짓이면 **아무것도 쓰지 않는다** — Reason 이 왜인지 말한다.
    // 연속 기록은 더하기가 아니라 **덮어쓰기**다(끊기면 0 이 된다). 그래서 증분 목록이 아니라
    // 새 값으로 낸다.
    public sealed record TallyPlan(
        bool Accepted,
        string Reason,
        IReadOnlyList<CounterIncrement> Increments,
        long TotalInGameTimeDelta,
        long NewCurrentWinStreak,
        long NewLongestWinStreak);

    // 클라이언트가 보낸 한 판치를 OverallCounters 증분으로 바꾼다. 순수 — SPT 타입도 프로필 접근도
    // 없고, 실제 쓰기는 A4(StmRaidTallyService)가 한다.
    //
    // 셈의 불변식 하나: **한 판에 Sessions 가 +1 이고 ExitStatus 합도 +1 이다.** 둘이 어긋나면
    // 화면의 생존률이 틀어진다 — 그 비율을 우리가 쓰지 않고 게임이 계산하기 때문이다.
    public static class RaidTallyPolicy
    {
        public const string PmcSide = "Pmc";

        // 비율(생존률·K/D)과 파생값은 여기서 만들지 않는다. 설계 §5-8 · 계획 6 §2-5.
        // 거절은 **그 팀원 한 명만** 버린다 — 한 명의 어긋난 보고가 같은 레이드의 다른 팀원
        // 적립을 막을 이유가 없다(그 반복은 호출자의 몫이다).
        public static TallyPlan Decide(RaidTally? tally, long currentWinStreak, long longestWinStreak)
        {
            if (tally == null)
            {
                return Refused("보고가 비어 있어 적립하지 않는다.", currentWinStreak, longestWinStreak);
            }

            if (string.IsNullOrWhiteSpace(tally.ProfileId))
            {
                return Refused("프로필 식별자가 비어 있어 적립하지 않는다.", currentWinStreak, longestWinStreak);
            }

            // 닫힌 쪽으로: 음수는 "모른다" 의 다른 모습이다. 더하면 프로필의 누적이 줄어든다.
            if (tally.Kills < 0 || tally.LifeTimeSeconds < 0)
            {
                return Refused(
                    $"보고에 음수가 있어 적립하지 않는다(사살 {tally.Kills}, 생존 {tally.LifeTimeSeconds}초).",
                    currentWinStreak,
                    longestWinStreak);
            }

            // 열거 밖의 값이 캐스팅으로 들어올 수 있다. 명시적으로 맞춘다 — 소거법으로 두면
            // 나중에 더해진 종료 상태가 조용히 '생환' 으로 쌓인다.
            string? exitWord = tally.ExitStatus switch
            {
                RaidExitStatus.Survived => "Survived",
                RaidExitStatus.Killed => "Killed",
                _ => null,
            };

            if (exitWord == null)
            {
                return Refused(
                    $"알 수 없는 종료 상태({(int)tally.ExitStatus})라 적립하지 않는다.",
                    currentWinStreak,
                    longestWinStreak);
            }

            var survived = tally.ExitStatus == RaidExitStatus.Survived;

            var increments = new List<CounterIncrement>
            {
                // 1. 레이드 횟수. ExitStatus 합과 반드시 짝이 맞는다.
                new CounterIncrement(new[] { "Sessions", PmcSide }, 1),
                // 2~3. 생존/KIA 중 하나만.
                new CounterIncrement(new[] { "ExitStatus", exitWord, PmcSide }, 1),
            };

            // 4. 사살. 서브키 없는 맨 열쇠다(실측: 한 판 뒤 ["Kills"] = 2).
            if (tally.Kills > 0)
            {
                increments.Add(new CounterIncrement(new[] { "Kills" }, tally.Kills));
            }

            // 5. 사망 수. K/D 의 분모라 0 일 때도 자리는 있어야 하지만, 더할 값이 0 이면
            //    적을 이유가 없다(없는 항목은 0 으로 읽힌다).
            if (!survived)
            {
                increments.Add(new CounterIncrement(new[] { "Deaths" }, 1));
            }

            // 6. 플레이시간. TotalInGameTime 에도 같은 값을 더한다 — 어느 쪽을 화면이 읽는지
            //    아직 재지 않았다(대응표 §3). 서로 다른 필드라 두 배가 되지는 않는다.
            if (tally.LifeTimeSeconds > 0)
            {
                increments.Add(new CounterIncrement(new[] { "LifeTime", PmcSide }, tally.LifeTimeSeconds));
            }

            // 7. 연속 생존. 생환이면 +1, 죽으면 0 으로 끊는다. 최장은 줄어들지 않는다.
            //    들어온 값이 음수면(손상된 프로필) 0 에서 시작한다 — 음수를 물려받아 셈이 어긋나는
            //    것보다 낫다.
            var sane = currentWinStreak > 0 ? currentWinStreak : 0;
            var newCurrent = survived ? sane + 1 : 0;
            var newLongest = Math.Max(longestWinStreak > 0 ? longestWinStreak : 0, newCurrent);

            var reason = survived
                ? $"생환 — 레이드 1판, 사살 {tally.Kills}, 생존 {tally.LifeTimeSeconds}초, 연속 생존 {newCurrent}."
                : $"전사 — 레이드 1판, 사살 {tally.Kills}, 생존 {tally.LifeTimeSeconds}초, 연속 생존이 끊겼다.";

            return new TallyPlan(
                Accepted: true,
                Reason: reason,
                Increments: increments,
                TotalInGameTimeDelta: tally.LifeTimeSeconds,
                NewCurrentWinStreak: newCurrent,
                NewLongestWinStreak: newLongest);
        }

        // 거절할 때는 연속 기록을 **그대로** 돌려준다. 호출자가 그 값을 그대로 다시 써도
        // 아무것도 바뀌지 않도록 — 거절이 프로필을 건드리는 길이 되면 안 된다.
        private static TallyPlan Refused(string reason, long currentWinStreak, long longestWinStreak) =>
            new TallyPlan(
                Accepted: false,
                Reason: reason,
                Increments: new List<CounterIncrement>(),
                TotalInGameTimeDelta: 0,
                NewCurrentWinStreak: currentWinStreak,
                NewLongestWinStreak: longestWinStreak);
    }
}
