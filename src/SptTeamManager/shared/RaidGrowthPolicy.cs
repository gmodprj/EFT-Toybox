using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // 계획 6 A5-2 — 레이드를 돈 프로필 팀원의 **경험치·스킬**을 어떻게 쌓을지 정한다.
    // 설계: doc/specs/2026-11-08-stm-raid-xp-skills-design.md
    //
    // 순수 판정이다. 프로필을 읽지도 쓰지도, SPT 를 참조하지도 않는다 - 지금 값(경험치·레벨·스킬)과
    // **경험치 표**를 호출자가 넘긴다(RaidTallyPolicy 와 같은 결).
    //
    // 지키는 것 넷(설계 §3). 상한은 이 중에 없다:
    //   1. 레벨은 **내려가지 않는다.** 적힌 레벨이 표보다 높아도(손으로 만든 프로필) 그대로 둔다.
    //   2. 레벨은 **표의 끝을 넘지 않는다**(실제 표는 79단).
    //   3. 스킬은 **0~5,100** 을 벗어나지 않고 내려가지 않는다(게임의 ClampCurrent 와 같은 값 -
    //      Assembly-CSharp 실측, 설계 §2-4).
    //   4. `Level` 과 `Experience` 가 **어긋난 채** 나가지 않는다 - 표가 없으면 **거절한다**.
    //      경험치만 올리고 레벨을 못 세면 그 프로필로 로그인한 플레이가 망가진다.
    //
    // **한 판 상한은 두지 않는다**(설계 §4-2·§4-3, 2026-11-08). 경험치 쪽은 사용자가 정했고,
    // 스킬 쪽은 실측이 정했다 - `Strength` 가 한 판에 167~188 올라오는데(이 모드 자신의 이동 스킬
    // 보정) 난간으로 뒀던 100 이 그것을 매 판 조용히 깎고 있었다. 깎는 코드를 아예 두지 않는다.
    public sealed record SkillGain(string? Id, double Delta);

    // 클라이언트가 보낸 팀원 한 명의 한 판치. 경험치는 이미 레벨 배수(GetLevelFactor)가 곱해진
    // 값이고, 스킬 delta 는 `PointsEarnedDuringSession` 이다(설계 §2-4 가 왜 그 필드인지 적었다).
    public sealed record RaidGrowth(string? ProfileId, double Experience, IReadOnlyList<SkillGain> Skills);

    // 지금 프로필에 있는 스킬 한 줄(`Skills.Common[]` 의 Id 와 Progress).
    public sealed record SkillState(string? Id, double Progress);

    // 그 스킬에 **최종적으로 쓸 값**. 증분이 아니라 결과값이다 - 쓰는 쪽이 다시 더하지 않게.
    public sealed record SkillWrite(string Id, double Progress);

    // 적립 계획. Accepted 가 거짓이면 **아무것도 쓰지 않는다**(그때 NewExperience·NewLevel 은
    // 지금 값 그대로다 - 호출자가 거절을 무시하고 써도 바뀌는 것이 없게).
    public sealed record GrowthPlan(
        bool Accepted,
        string Reason,
        long ExperienceDelta,
        long NewExperience,
        int NewLevel,
        IReadOnlyList<SkillWrite> SkillWrites);

    public static class RaidGrowthPolicy
    {
        // 게임의 BaseSkill.ClampCurrent 와 같은 값(51레벨 × 100). 어셈블리에서 읽었다.
        public const double MaxSkillProgress = 5100d;

        public static GrowthPlan Decide(
            RaidGrowth? growth,
            long currentExperience,
            int currentLevel,
            IReadOnlyList<SkillState>? currentSkills,
            IReadOnlyList<long>? experienceTable)
        {
            GrowthPlan Refuse(string reason) =>
                new GrowthPlan(false, reason, 0, currentExperience, currentLevel, Array.Empty<SkillWrite>());

            if (growth == null || string.IsNullOrWhiteSpace(growth.ProfileId))
            {
                return Refuse("보고가 없거나 프로필 id 가 비어 있다.");
            }

            // 표가 없으면 레벨을 셀 수 없다. 경험치만 올리면 Level 과 Experience 가 어긋난다(불변식 4).
            if (experienceTable == null || experienceTable.Count == 0)
            {
                return Refuse("경험치 표가 없어 레벨을 셀 수 없다.");
            }

            long earned = 0;
            if (IsUsable(growth.Experience) && growth.Experience > 0)
            {
                double rounded = Math.Round(growth.Experience, MidpointRounding.AwayFromZero);
                // 넘치면 받지 않는다 - 지어낸 값으로 프로필을 덮지 않는다.
                if (rounded > long.MaxValue - currentExperience)
                {
                    return Refuse("경험치가 담을 수 없을 만큼 크다.");
                }

                earned = (long)rounded;
            }

            long newExperience = currentExperience + earned;
            int newLevel = LevelFor(newExperience, experienceTable);
            if (newLevel < currentLevel)
            {
                // 적힌 레벨이 표보다 높다. 내리지 않는다 - 내리는 쪽이 플레이를 망가뜨린다.
                newLevel = currentLevel;
            }

            var writes = PlanSkills(growth.Skills, currentSkills);

            if (earned <= 0 && writes.Count == 0)
            {
                return Refuse("쌓을 것이 없다.");
            }

            return new GrowthPlan(true, string.Empty, earned, newExperience, newLevel, writes);
        }

        // 누적이 아니라 **단계별** 값을 받는다(SPT globals 의 exp_table 그대로).
        // 표의 끝이 곧 레벨 상한이다(실제 표는 79단).
        public static int LevelFor(long experience, IReadOnlyList<long> experienceTable)
        {
            long accumulated = 0;
            int level = 0;
            for (int i = 0; i < experienceTable.Count; i++)
            {
                long step = experienceTable[i];
                accumulated += step > 0 ? step : 0;
                if (experience < accumulated)
                {
                    break;
                }

                level = i + 1;
            }

            return level < 1 ? 1 : level;
        }

        private static IReadOnlyList<SkillWrite> PlanSkills(
            IReadOnlyList<SkillGain>? gains,
            IReadOnlyList<SkillState>? currentSkills)
        {
            if (gains == null || gains.Count == 0)
            {
                return Array.Empty<SkillWrite>();
            }

            // 같은 Id 가 두 번 오면 합친다(한 번만 쓴다). 처음 나온 순서를 지킨다.
            var summed = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();
            foreach (SkillGain gain in gains)
            {
                string? id = gain?.Id?.Trim();
                if (string.IsNullOrEmpty(id)) continue;
                if (gain == null || !IsUsable(gain.Delta) || gain.Delta <= 0) continue;

                if (summed.TryGetValue(id!, out double already))
                {
                    summed[id!] = already + gain.Delta;
                }
                else
                {
                    summed[id!] = gain.Delta;
                    order.Add(id!);
                }
            }

            var writes = new List<SkillWrite>(order.Count);
            foreach (string id in order)
            {
                SkillState? existing = currentSkills?.FirstOrDefault(
                    s => s != null && string.Equals(s.Id?.Trim(), id, StringComparison.OrdinalIgnoreCase));

                // 프로필에 있는 이름을 그대로 쓴다 - 대소문자가 달라도 같은 줄에 쌓이게.
                string name = string.IsNullOrWhiteSpace(existing?.Id) ? id : existing!.Id!.Trim();
                double start = existing != null && IsUsable(existing.Progress) && existing.Progress > 0
                    ? existing.Progress
                    : 0d;

                double next = Math.Min(MaxSkillProgress, start + summed[id]);
                if (next <= start)
                {
                    // 이미 만렙이거나 더해도 그대로다. 쓰지 않는다 - 안 바뀌는 쓰기는 쓰기가 아니다.
                    continue;
                }

                writes.Add(new SkillWrite(name, Math.Round(next, 2)));
            }

            return writes;
        }

        private static bool IsUsable(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
