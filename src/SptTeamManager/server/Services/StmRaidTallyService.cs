using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// 요청 1-1 — 프로필 팀원이 레이드를 돈 것을 그 런처 프로필의 종합정보에 쌓는다.
// 무엇을 세고 어느 열쇠에 쌓는지는 doc/specs/2026-11-07-stm-raid-tally-mapping.md 의 표이고,
// 판정은 RaidTallyPolicy(순수·시험 있음)의 몫이다. 이 클래스는 **기계**만 가진다.
//
// 이것은 STM 이 런처 프로필에 **더하는** 첫 코드다. 지금까지는 빼기(사망 시 장비 삭제)와
// 한 프로필 안의 이동(장비 편집)뿐이었다. 그래서 설계 §4 가 "조용히 망가진다" 로 못박은 자리를
// 건드리며, 지키는 것은 셋이다.
//   1. **영수증을 먼저 예약한다.** 같은 (레이드, 프로필) 에 두 번 쌓이지 않는다. 적립은
//      되돌릴 수 없다 - 카운터를 되빼는 코드는 없고, 있어야 할 이유도 없다.
//   2. **프로필이 저장 가능한지 먼저 본다.** SPT 가 손상/적재 불가로 표시한 프로필은
//      SaveProfileAsync 가 아무것도 쓰지 않고 돌아가므로(StmRaidOutcomeService 가 같은 자리에서
//      재어 둔 사실이다) 메모리만 바뀌고 영수증은 소모된다.
//   3. **쓴 뒤에 다시 읽어 확인한다.** 어긋나면 Error 로 남긴다. 되돌리지는 않는다 -
//      카운터는 사람이 읽는 수치이고, 추측으로 되빼면 그쪽이 더 나쁘다.
//
// **비율은 쓰지 않는다**(생존률·K/D). 게임이 원시 카운터로 계산한다. 그것이 요청 1-2(런처에서도
// 같게 보인다)가 저절로 맞는 이유다 - 우리가 쓰는 것은 프로필 JSON 의 그 숫자들 자체다.
[Injectable(InjectionType.Singleton)]
public class StmRaidTallyService(
    StmProfileMemberResolver resolver,
    StmRaidReceiptStore receipts,
    SaveServer saveServer,
    GlobalTable globalTable,
    ISptLogger<StmRaidTallyService> logger)
{
    private const string TallyAction = "accrue-raid-tally";

    // 영수증의 열쇠 공간은 하나다 - StmRaidReceiptStore.Matches 는 (레이드 id, 프로필 id) 만 보고
    // action 은 **보지 않는다**(기록만 한다). 그래서 장비 삭제가 같은 판에서 이미 예약해 둔
    // 영수증과 부딪히지 않도록 레이드 id 에 접미사를 붙인다. 그 저장소의 비교 규칙을 바꾸는 쪽은
    // 고르지 않았다 - 그 규칙은 "두 번 지우지 않는다" 를 지키는 자리이고, 여기 때문에 건드릴
    // 이유가 없다(불변식 4 와 같은 결).
    private const string TallyRaidSuffix = "#tally";

    // 2단계(수치)의 영수증은 **열쇠 공간이 따로다.** 같은 판에서 1단계가 이미 '#tally' 를 썼으므로
    // 같은 접미사를 쓰면 수치가 "이미 적립된 판" 으로 걸러져 영원히 안 쌓인다.
    private const string NumbersAction = "accrue-raid-numbers";
    private const string NumbersRaidSuffix = "#tally-nums";

    // A5-3 — 경험치·스킬의 영수증. 같은 이유로 **세 번째** 열쇠 공간이다(설계 §4-1).
    // 경험치와 스킬은 한 영수증으로 묶는다 - 같은 항목으로 같이 오고, 하나만 쌓이는 중간 상태를
    // 만들 이유가 없다.
    private const string GrowthAction = "accrue-raid-growth";
    private const string GrowthRaidSuffix = "#tally-xp";

    // 열거된 다섯 열쇠만 쓴다. 표에 없는 것은 만들지 않는다 - 없는 열쇠를 프로필에 넣으면
    // 게임이나 다른 모드가 그것을 어떻게 읽을지 모른다(2026-10-28 에 `Ammo` 클래스 정의 노드에
    // 값을 써서 장전이 먹통이 된 그 종류의 실수다).
    public void Apply(MongoId sessionId, string? raidId, string? accountId, bool escaped, int kills, int raidSeconds, bool hadNumbers)
    {
        try
        {
            if (!resolver.TryResolve(sessionId, accountId, out var member, out var unavailableReason) || member == null)
            {
                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    logger.Warning($"[STM] 전적 적립: 프로필 팀원 '{accountId}'(세션 '{sessionId}')을 읽을 수 없어({unavailableReason}) 적립하지 않는다.");
                }

                return;
            }

            var profileId = new MongoId(member.ProfileId);

            // 2. 저장되지 않을 프로필은 영수증을 쓰기 전에 돌려보낸다. 쓰지 않으면 나중에 그 프로필이
            //    정상으로 돌아왔을 때 다시 적립될 수 있다.
            if (saveServer.IsProfileInvalidOrUnloadable(profileId))
            {
                logger.Error($"[STM] 전적 적립: 프로필 '{member.ProfileId}'(레이드 '{raidId}')이 SPT에서 손상/적재 불가로 표시돼 있어 저장이 되지 않는다. 적립하지 않고 영수증도 남기지 않는다.");
                return;
            }

            // 3. 레이드 식별자가 없으면 멱등을 지킬 수 없다. 두 번 쌓이는 것이 안 쌓이는 것보다
            //    나쁘므로(되돌릴 수 없다) 적립하지 않는다.
            var tallyRaidId = string.IsNullOrWhiteSpace(raidId) ? null : raidId!.Trim() + TallyRaidSuffix;
            if (!RaidReceiptKeyPolicy.IsUsableKey(tallyRaidId, member.ProfileId))
            {
                logger.Error($"[STM] 전적 적립: '{member.Nickname}'(프로필 '{member.ProfileId}')의 레이드 식별자가 비어 있어(raidId '{raidId}') 멱등을 지킬 수 없다. 적립하지 않는다.");
                return;
            }

            // 1. 예약이 먼저다. true 는 "새 영수증을 썼다" 하나만 뜻한다.
            if (!receipts.TryReserve(sessionId, tallyRaidId, member.ProfileId, TallyAction))
            {
                logger.Info($"[STM] 전적 적립: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}')은 이미 적립된 판이라 다시 쌓지 않는다.");
                return;
            }

            var pmc = member.Pmc;
            var eft = EnsureCounters(pmc);
            if (eft == null)
            {
                logger.Error($"[STM] 전적 적립: 프로필 '{member.ProfileId}'의 통계 자리를 만들 수 없어 적립하지 않는다. 영수증은 이미 예약됐으므로 이 판은 다시 처리되지 않는다.");
                return;
            }

            var exit = escaped ? RaidExitStatus.Survived : RaidExitStatus.Killed;
            var plan = RaidTallyPolicy.Decide(
                new RaidTally(member.ProfileId, exit, kills, raidSeconds),
                Read(eft, "CurrentWinStreak", RaidTallyPolicy.PmcSide),
                Read(eft, "LongestWinStreak", RaidTallyPolicy.PmcSide));

            if (!plan.Accepted)
            {
                logger.Error($"[STM] 전적 적립: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason} 영수증은 이미 예약됐으므로 이 판은 다시 처리되지 않는다.");
                return;
            }

            foreach (var increment in plan.Increments)
            {
                Add(eft, increment.Key, increment.Value);
            }

            // 연속 기록은 더하기가 아니라 덮어쓰기다 - 끊기면 0 이 된다.
            Set(eft, plan.NewCurrentWinStreak, "CurrentWinStreak", RaidTallyPolicy.PmcSide);
            Set(eft, plan.NewLongestWinStreak, "LongestWinStreak", RaidTallyPolicy.PmcSide);

            // 플레이시간은 카운터와 이 필드 양쪽에 간다(대응표 §3 - 화면이 어느 쪽을 읽는지 아직
            // 재지 않았다). 서로 다른 필드라 두 배가 되지 않는다.
            eft.TotalInGameTime = (eft.TotalInGameTime ?? 0) + plan.TotalInGameTimeDelta;

            var elapsedMs = saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
            logger.Info(
                $"[STM] 전적 적립: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason} " +
                $"저장했다({elapsedMs}ms)." +
                (hadNumbers ? string.Empty : " **수치 보고가 아직 오지 않아 사살·생존시간은 0 으로 쌓았다** - 두 라우트의 순서를 이 줄로 가린다."));

            VerifyApplied(member, profileId, raidId, plan);
        }
        catch (Exception ex)
        {
            // PIT 의 경로로 던지지 않는다. 여기서 실패하면 그 판의 전적이 안 쌓이는 것이 값이고,
            // 던지면 레이드 종료 처리 전체가 멈춘다.
            logger.Error($"[STM] 전적 적립: 프로필 팀원 '{accountId}'(세션 '{sessionId}', 레이드 '{raidId}') 처리에 실패했다: {ex}");
        }
    }

    // 2단계 — 뒤늦게 온 수치(사살·생존 시간)만 쌓는다. **레이드 횟수·생사·연속 기록은 건드리지
    // 않는다**(1단계가 이미 쌓았고, 다시 더하면 한 판이 두 판이 된다).
    //
    // 2026-11-07 실측으로 생긴 길이다 — 생사가 수치보다 먼저 처리돼, 1단계만으로는 사살과
    // 플레이시간이 영원히 0 이었다(그 판 네 명 모두 그랬다).
    public void ApplyNumbers(MongoId sessionId, string? raidId, string? accountId, int kills, int raidSeconds)
    {
        try
        {
            // 쌓을 것이 없으면 영수증도 쓰지 않는다 - 진짜 수치가 더 늦게 오면 그때 쌓을 수 있어야 한다.
            if (kills <= 0 && raidSeconds <= 0) return;

            if (!resolver.TryResolve(sessionId, accountId, out var member, out var unavailableReason) || member == null)
            {
                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    logger.Warning($"[STM] 전적 적립(수치): 프로필 팀원 '{accountId}'(세션 '{sessionId}')을 읽을 수 없어({unavailableReason}) 적립하지 않는다.");
                }

                return;
            }

            var profileId = new MongoId(member.ProfileId);

            if (saveServer.IsProfileInvalidOrUnloadable(profileId))
            {
                logger.Error($"[STM] 전적 적립(수치): 프로필 '{member.ProfileId}'(레이드 '{raidId}')이 손상/적재 불가로 표시돼 있어 저장되지 않는다. 적립하지 않고 영수증도 남기지 않는다.");
                return;
            }

            var numbersRaidId = string.IsNullOrWhiteSpace(raidId) ? null : raidId!.Trim() + NumbersRaidSuffix;
            if (!RaidReceiptKeyPolicy.IsUsableKey(numbersRaidId, member.ProfileId))
            {
                logger.Error($"[STM] 전적 적립(수치): '{member.Nickname}'(프로필 '{member.ProfileId}')의 레이드 식별자가 비어 있어(raidId '{raidId}') 멱등을 지킬 수 없다. 적립하지 않는다.");
                return;
            }

            if (!receipts.TryReserve(sessionId, numbersRaidId, member.ProfileId, NumbersAction))
            {
                logger.Info($"[STM] 전적 적립(수치): '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}')의 수치는 이미 쌓인 판이라 다시 쌓지 않는다.");
                return;
            }

            var eft = EnsureCounters(member.Pmc);
            if (eft == null)
            {
                logger.Error($"[STM] 전적 적립(수치): 프로필 '{member.ProfileId}'의 통계 자리를 만들 수 없어 적립하지 않는다. 영수증은 이미 예약됐다.");
                return;
            }

            var plan = RaidTallyPolicy.DecideNumbers(new RaidNumbers(member.ProfileId, kills, raidSeconds));
            if (!plan.Accepted)
            {
                logger.Error($"[STM] 전적 적립(수치): '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason} 영수증은 이미 예약됐다.");
                return;
            }

            foreach (var increment in plan.Increments)
            {
                Add(eft, increment.Key, increment.Value);
            }

            eft.TotalInGameTime = (eft.TotalInGameTime ?? 0) + plan.TotalInGameTimeDelta;

            var elapsedMs = saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
            logger.Info(
                $"[STM] 전적 적립(수치): '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason} 저장했다({elapsedMs}ms). " +
                $"누적 사살 {Read(eft, "Kills")} / 누적 플레이 {eft.TotalInGameTime ?? 0}초.");
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전적 적립(수치): 프로필 팀원 '{accountId}'(세션 '{sessionId}', 레이드 '{raidId}') 처리에 실패했다: {ex}");
        }
    }

    // A5-3 — 경험치와 스킬. 수치(ApplyNumbers)와 **같은 보고**로 오지만 영수증 열쇠 공간이 따로라
    // 한쪽이 다른 쪽을 걸러내지 않는다(설계 §4-1).
    //
    // 이것은 STM 이 런처 프로필의 **레벨**을 건드리는 첫 코드다. 지키는 것은 A4 와 같은 셋(예약 먼저 /
    // 저장 가능한 프로필인지 먼저 / 쓴 뒤 다시 읽어 확인)이고, 더해서 **표가 없으면 아무것도 쓰지
    // 않는다** - 경험치만 올리고 레벨을 못 세면 Level 과 Experience 가 어긋난 채 저장된다.
    // 판정은 전부 RaidGrowthPolicy(순수·시험 20건)의 몫이고 이 메서드는 기계만 가진다.
    public void ApplyGrowth(MongoId sessionId, string? raidId, string? accountId, double experience,
        IReadOnlyList<FriendlyTeammateSkillProgressRequest>? skills)
    {
        try
        {
            if (!resolver.TryResolve(sessionId, accountId, out var member, out var unavailableReason) || member == null)
            {
                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    logger.Warning($"[STM] 성장 적립: 프로필 팀원 '{accountId}'(세션 '{sessionId}')을 읽을 수 없어({unavailableReason}) 적립하지 않는다.");
                }

                return;
            }

            var profileId = new MongoId(member.ProfileId);

            if (saveServer.IsProfileInvalidOrUnloadable(profileId))
            {
                logger.Error($"[STM] 성장 적립: 프로필 '{member.ProfileId}'(레이드 '{raidId}')이 손상/적재 불가로 표시돼 있어 저장되지 않는다. 적립하지 않고 영수증도 남기지 않는다.");
                return;
            }

            var pmc = member.Pmc;
            if (pmc?.Info == null)
            {
                logger.Error($"[STM] 성장 적립: 프로필 '{member.ProfileId}'의 Info 를 읽을 수 없어 적립하지 않는다.");
                return;
            }

            // 표를 **먼저** 본다. 없으면 영수증도 쓰지 않는다 - 다음 기동에서 다시 해 볼 수 있게.
            var table = ExperienceTable();
            if (table.Count == 0)
            {
                logger.Error($"[STM] 성장 적립: 경험치 표를 읽을 수 없어 레벨을 셀 수 없다(프로필 '{member.ProfileId}', 레이드 '{raidId}'). 적립하지 않고 영수증도 남기지 않는다.");
                return;
            }

            var gains = ToSkillGains(skills);
            var current = CurrentSkills(pmc);
            long beforeExperience = pmc.Info.Experience ?? 0;
            int beforeLevel = pmc.Info.Level ?? 1;

            var plan = RaidGrowthPolicy.Decide(
                new RaidGrowth(member.ProfileId, experience, gains), beforeExperience, beforeLevel, current, table);

            // 쌓을 것이 없으면 영수증도 쓰지 않는다 - 진짜 수치가 더 늦게 오면 그때 쌓을 수 있어야 한다.
            if (!plan.Accepted)
            {
                logger.Info($"[STM] 성장 적립: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason} 적립하지 않는다.");
                return;
            }

            // 프로필의 경험치 칸은 int 다(SPT 모델). 표의 끝(레벨 79)이 8,100만이라 닿을 일이 없지만,
            // 넘는 값을 깎아 넣으면 레벨과 어긋난다 - 그래서 넘으면 아무것도 쓰지 않는다.
            if (plan.NewExperience > int.MaxValue)
            {
                logger.Error($"[STM] 성장 적립: 경험치 {plan.NewExperience} 가 프로필이 담을 수 있는 범위를 넘는다(프로필 '{member.ProfileId}', 레이드 '{raidId}'). 적립하지 않는다.");
                return;
            }

            var growthRaidId = string.IsNullOrWhiteSpace(raidId) ? null : raidId!.Trim() + GrowthRaidSuffix;
            if (!RaidReceiptKeyPolicy.IsUsableKey(growthRaidId, member.ProfileId))
            {
                logger.Error($"[STM] 성장 적립: '{member.Nickname}'(프로필 '{member.ProfileId}')의 레이드 식별자가 비어 있어(raidId '{raidId}') 멱등을 지킬 수 없다. 적립하지 않는다.");
                return;
            }

            if (!receipts.TryReserve(sessionId, growthRaidId, member.ProfileId, GrowthAction))
            {
                logger.Info($"[STM] 성장 적립: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}')의 성장은 이미 쌓인 판이라 다시 쌓지 않는다.");
                return;
            }

            pmc.Info.Experience = (int)plan.NewExperience;
            pmc.Info.Level = plan.NewLevel;
            WriteSkills(pmc, plan.SkillWrites);

            var elapsedMs = saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
            logger.Info(
                $"[STM] 성장 적립: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') 경험치 +{plan.ExperienceDelta} " +
                $"({beforeExperience} → {plan.NewExperience}), 레벨 {beforeLevel} → {plan.NewLevel}, " +
                $"스킬 {plan.SkillWrites.Count}개. 저장했다({elapsedMs}ms).");

            VerifyGrowthApplied(member, profileId, raidId, plan);
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 성장 적립: 프로필 팀원 '{accountId}'(세션 '{sessionId}', 레이드 '{raidId}') 처리에 실패했다: {ex}");
        }
    }

    // 적립 뒤 다시 읽어 **기대값과 같은지** 본다. VerifyApplied 와 같은 성질이다(SaveServer 가
    // 방금 고친 객체를 돌려주므로 "쓰기가 반영됐나" 를 잡고 저장 실패는 못 잡는다 - 저장 실패는
    // 위에서 던져 catch 로 간다).
    private void VerifyGrowthApplied(StmResolvedMember member, MongoId profileId, string? raidId, GrowthPlan plan)
    {
        var reread = saveServer.GetProfile(profileId)?.CharacterData?.PmcData;
        if (reread?.Info == null)
        {
            logger.Error($"[STM] 성장 적립 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}')를 다시 읽을 수 없었다. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
            return;
        }

        long experience = reread.Info.Experience ?? 0;
        int level = reread.Info.Level ?? 0;
        if (experience != plan.NewExperience || level != plan.NewLevel)
        {
            logger.Error(
                $"[STM] 성장 적립 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}') - 기대 경험치 {plan.NewExperience}/레벨 {plan.NewLevel} 인데 " +
                $"{experience}/{level} 이다. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
            return;
        }

        // 스킬은 **하나라도** 어긋나면 소리를 낸다. 0~5,100 을 벗어난 값이 들어가면 게임이 어떻게
        // 읽을지 모른다.
        var written = reread.Skills?.Common ?? [];
        var mismatched = plan.SkillWrites
            .Where(w => !written.Any(s => string.Equals(s.Id.ToString(), w.Id, StringComparison.OrdinalIgnoreCase)
                                          && Math.Abs(s.Progress - w.Progress) < 0.005))
            .Select(w => w.Id)
            .ToList();
        if (mismatched.Count > 0)
        {
            logger.Error(
                $"[STM] 성장 적립 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}') - 스킬 {mismatched.Count}개가 기대와 다르다 " +
                $"({string.Join(", ", mismatched)}). 프로필을 직접 확인하라.");
            return;
        }

        logger.Info(
            $"[STM] 성장 적립 확인: 프로필 '{member.ProfileId}'(레이드 '{raidId}') - 경험치 {experience} / 레벨 {level} / 스킬 {plan.SkillWrites.Count}개가 기대와 같다.");
    }

    // globals 의 exp_table 을 **단계별 값**으로 낸다(누적이 아니다 - 정책이 누적을 센다).
    // 읽을 수 없으면 빈 목록이고, 호출자가 그때 아무것도 쓰지 않는다.
    private IReadOnlyList<long> ExperienceTable()
    {
        try
        {
            var table = globalTable?.Configuration?.Exp?.Level?.ExperienceTable;
            if (table == null) return [];
            return table.Select(row => (long)(row?.Experience ?? 0)).ToList();
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 성장 적립: 경험치 표를 읽다 실패했다: {ex.Message}");
            return [];
        }
    }

    // 지금 프로필의 스킬. 정책은 문자열 id 로 다루고(순수하게 두려고 SPT 열거를 모른다),
    // 쓸 때 다시 열거로 돌린다.
    private static IReadOnlyList<SkillState> CurrentSkills(PmcData pmc) =>
        (pmc.Skills?.Common ?? []).Select(s => new SkillState(s.Id.ToString(), s.Progress)).ToList();

    // 요청의 Id 는 문자열로도 숫자로도 온다(PIT 의 TryParseSkillType 과 같은 규칙). 정의 밖 값은
    // 버린다 - 없는 스킬을 프로필에 만들지 않는다. delta 는 PointsEarnedDuringSession 이다(설계 §2-4).
    private static IReadOnlyList<SkillGain> ToSkillGains(IReadOnlyList<FriendlyTeammateSkillProgressRequest>? skills)
    {
        if (skills == null || skills.Count == 0) return [];

        var gains = new List<SkillGain>(skills.Count);
        foreach (var skill in skills)
        {
            if (skill == null) continue;
            if (!TryParseSkillType(skill.Id, out var skillType)) continue;
            gains.Add(new SkillGain(skillType.ToString(), skill.PointsEarnedDuringSession));
        }

        return gains;
    }

    private static bool TryParseSkillType(JsonElement idValue, out SkillTypes skillType)
    {
        skillType = default;

        if (idValue.ValueKind == JsonValueKind.String)
        {
            var raw = idValue.GetString();
            return !string.IsNullOrWhiteSpace(raw) && Enum.TryParse(raw, ignoreCase: true, out skillType);
        }

        if (idValue.ValueKind == JsonValueKind.Number && idValue.TryGetInt32(out var numeric))
        {
            skillType = (SkillTypes)numeric;
            return Enum.IsDefined(skillType);
        }

        return false;
    }

    // 정책이 낸 **최종값**을 그 줄에 쓴다. 없는 줄은 만들고, 있는 줄은 Progress 만 바꾼다 -
    // 다른 필드(PointsEarnedDuringSession·LastAccess)는 그 프로필의 것이라 건드리지 않는다.
    private static void WriteSkills(PmcData pmc, IReadOnlyList<SkillWrite> writes)
    {
        if (writes.Count == 0) return;

        pmc.Skills ??= new Skills();
        pmc.Skills.Common ??= [];
        var common = pmc.Skills.Common.ToList();

        foreach (var write in writes)
        {
            if (!Enum.TryParse(write.Id, ignoreCase: true, out SkillTypes skillType)) continue;

            var existing = common.FirstOrDefault(s => s.Id == skillType);
            if (existing != null)
            {
                existing.Progress = write.Progress;
                continue;
            }

            common.Add(new CommonSkill
            {
                Id = skillType,
                Progress = write.Progress,
                PointsEarnedDuringSession = 0,
                LastAccess = 0,
            });
        }

        pmc.Skills.Common = common;
    }

    // 쓴 값을 SaveServer 에서 다시 읽어 본다. 닿는 범위를 분명히 해 둔다 - SaveServer 는 방금 고친
    // 그 메모리 객체를 돌려주므로 이것은 **쓰기가 실제로 반영됐는지**를 잡고, 저장 실패는 잡지
    // 못한다(저장 실패는 위에서 던져 catch 로 간다). StmRaidOutcomeService.VerifyDeletionApplied 와
    // 같은 성질이고 같은 이유로 그렇게 두었다.
    private void VerifyApplied(StmResolvedMember member, MongoId profileId, string? raidId, TallyPlan plan)
    {
        var reread = saveServer.GetProfile(profileId)?.CharacterData?.PmcData?.Stats?.Eft;
        if (reread == null)
        {
            logger.Error($"[STM] 전적 적립 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}')의 통계를 다시 읽을 수 없었다. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
            return;
        }

        var sessions = Read(reread, "Sessions", RaidTallyPolicy.PmcSide);
        var exits = (reread.OverallCounters?.Items ?? [])
            .Where(i => i.Key != null && i.Key.Count == 3 && i.Key.Contains("ExitStatus") && i.Key.Contains(RaidTallyPolicy.PmcSide))
            .Sum(i => i.Value ?? 0);

        // 셈의 불변식(대응표 §1): 레이드 횟수와 종료 상태 합이 같다. 어긋나면 화면의 생존률이
        // 틀어지는데, 그 비율은 우리가 쓰지 않아 고칠 자리도 없다 - 그래서 여기서 소리를 낸다.
        if (sessions != exits)
        {
            logger.Error(
                $"[STM] 전적 적립 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}') - 레이드 횟수 {sessions} 와 종료 상태 합 {exits} 이 어긋난다. " +
                $"생존률이 틀리게 보인다. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
            return;
        }

        logger.Info(
            $"[STM] 전적 적립 확인: 프로필 '{member.ProfileId}'(레이드 '{raidId}') - 레이드 {sessions}판 / 종료 상태 합 {exits} / 연속 생존 {Read(reread, "CurrentWinStreak", RaidTallyPolicy.PmcSide)}(최장 {Read(reread, "LongestWinStreak", RaidTallyPolicy.PmcSide)}) / 누적 플레이 {reread.TotalInGameTime ?? 0}초.");
    }

    // PIT 의 EnsureTeammateEftStats 와 같은 자리를 만든다. 다만 여기 대상은 **실제 런처 프로필**
    // 이므로 이미 있는 값을 새 객체로 갈아 끼우지 않는다(??= 뿐이다) - 갈면 그 프로필의 전적이
    // 통째로 사라진다.
    private static EftStats? EnsureCounters(PmcData pmc)
    {
        if (pmc == null) return null;
        pmc.Stats ??= new Stats();
        pmc.Stats.Eft ??= new EftStats();
        pmc.Stats.Eft.OverallCounters ??= new OverallCounters { Items = [] };
        pmc.Stats.Eft.OverallCounters.Items ??= [];
        return pmc.Stats.Eft;
    }

    // 열쇠는 HashSet<string> 이다(설치된 4.1.6 어셈블리를 역컴파일해 확인했다 -
    // CounterKeyValue.Key 가 List 가 아니다). 그래서 **집합으로** 견준다: 순서가 달라도 같은
    // 열쇠이고, ["Kills"] 와 ["Kills","Pmc"] 는 다른 열쇠다.
    private static CounterKeyValue? Find(EftStats eft, IReadOnlyList<string> key)
    {
        var wanted = new HashSet<string>(key, StringComparer.Ordinal);
        return (eft.OverallCounters?.Items ?? [])
            .FirstOrDefault(i => i.Key != null && i.Key.SetEquals(wanted));
    }

    private static long Read(EftStats eft, params string[] key) => Find(eft, key)?.Value ?? 0;

    private static void Add(EftStats eft, IReadOnlyList<string> key, long delta)
    {
        var existing = Find(eft, key);
        if (existing != null)
        {
            existing.Value = (existing.Value ?? 0) + delta;
            return;
        }

        eft.OverallCounters!.Items!.Add(new CounterKeyValue
        {
            Key = new HashSet<string>(key, StringComparer.Ordinal),
            Value = delta,
        });
    }

    private static void Set(EftStats eft, long value, params string[] key)
    {
        var existing = Find(eft, key);
        if (existing != null)
        {
            existing.Value = value;
            return;
        }

        eft.OverallCounters!.Items!.Add(new CounterKeyValue
        {
            Key = new HashSet<string>(key, StringComparer.Ordinal),
            Value = value,
        });
    }
}
