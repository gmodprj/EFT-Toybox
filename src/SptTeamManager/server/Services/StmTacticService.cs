using System;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace pitTeam.Server.Services;

// 프로필 팀원의 전투 전술(상세 화면의 "AI 타입" 드롭다운)을 등록 문서에 저장한다.
//
// 저장만 하면 끝인 이유: 전술은 이미 투입 경로를 타고 있다. 등록의 Tactic 이
// FriendlyTeammateMatchCallbacks 의 팀원 상세 응답에 실리고 → 클라이언트의 botsTactic[aid] →
// profileTactic[profileId] → BotFollowerPlayer.CombatTactic 으로 간다. 2026-11-02 전까지 비어 있던 것은
// **저장할 길이 없었기** 때문이다(상세 화면이 전술 조작을 거부했다). 그래서 이 서비스는 그 한 칸만 연다.
//
// StmAutoJoinService 와 같은 모양이다. PIT 콜백 안에서 불리므로 어떤 경우에도 던지지 않는다 - 던지면
// PIT 자기 용병의 전술 변경까지 같이 죽는다.
[Injectable(InjectionType.Singleton)]
public class StmTacticService(
    StmProfileMemberResolver resolver,
    StmProfileMemberStore store,
    ISptLogger<StmTacticService> logger)
{
    // true  = STM 이 처리했다(저장됨). 호출자는 PIT 를 부르지 않는다.
    // false + unavailableReason == null  = 프로필 팀원이 아니다. 호출자는 PIT 경로를 그대로 탄다.
    // false + unavailableReason != null  = 등록된 프로필 팀원인데 처리하지 못했다. PIT 에는 이 Aid 의 용병이
    //                                     없으므로 PIT 도 부르지 않는다.
    // 해석기를 PIT 보다 먼저 묻는다(불변식 3).
    public bool TrySetTactic(MongoId sessionId, string? accountId, string? tactic, out string? unavailableReason)
    {
        unavailableReason = null;
        var identifiedAsMember = false;
        try
        {
            if (!resolver.TryResolve(sessionId, accountId, out var member, out var resolveReason) || member == null)
            {
                unavailableReason = resolveReason;
                return false;
            }

            identifiedAsMember = true;

            // 허용 목록은 PIT 의 것 하나뿐이다(인계 §3-8: 한 판단에 집은 하나). NormalizeCombatTactic 은
            // 모르는 값과 빈 값을 PIT 의 기본 전술로 떨어뜨리므로, 여기서 따로 목록을 적지 않는다.
            var normalized = FriendlyTeammateService.NormalizeCombatTactic(tactic);

            // 등록을 새로 만들거나 되살리지 않는다: Update 는 등록이 없으면 false 이고 아무것도 쓰지 않는다.
            if (!store.Update(sessionId, member.ProfileId, new StmProfileMemberChange(null, null, normalized)))
            {
                unavailableReason = "Unavailable";
                logger.Warning($"[STM] 전술: 프로필 '{member.ProfileId}'(Aid '{accountId}')의 등록을 찾지 못해 저장하지 않았다.");
                return false;
            }

            logger.Info($"[STM] 전술: 프로필 '{member.ProfileId}'(Aid '{accountId}')에 '{normalized}' 저장.");
            return true;
        }
        catch (Exception ex)
        {
            unavailableReason = identifiedAsMember ? "Unreadable" : null;
            logger.Error($"[STM] 전술 저장: '{accountId}'(세션 '{sessionId}') 처리 실패 ({(identifiedAsMember ? "프로필 팀원, 읽을 수 없다고 보고" : "기본 경로로 넘긴다")}): {ex}");
            return false;
        }
    }
}
