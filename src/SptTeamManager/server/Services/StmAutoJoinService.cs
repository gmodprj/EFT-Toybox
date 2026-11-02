using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace pitTeam.Server.Services;

// auto-join 이 프로필 팀원에게 동작하게 하는 두 길의 STM 쪽: 토글 저장(SetAutoJoin)과 목록 조회(ListAutoJoin).
// PIT 의 두 경로는 용병만 안다. 출격 화면(MatchMakerAcceptScreen.Show)에서 켜진 팀원을 분대에 넣는 기능이고
// "지난 분대 복원" 이 아니다. 둘 다 PIT 콜백 안에서 불리므로 어떤 경우에도 던지지 않는다 - 던지면 PIT 자기
// 용병의 auto-join 까지 같이 죽는다(PIT 가 깨지는 것이 이 프로젝트의 실패 조건이다).
[Injectable(InjectionType.Singleton)]
public class StmAutoJoinService(
    StmProfileMemberResolver resolver,
    StmProfileMemberStore store,
    StmProfileCatalog catalog,
    ISptLogger<StmAutoJoinService> logger)
{
    // true  = STM 이 처리했다(저장됨). 호출자는 PIT 를 부르지 않는다.
    // false + unavailableReason == null  = 프로필 팀원이 아니다. 호출자는 PIT 경로를 그대로 탄다.
    // false + unavailableReason != null  = 등록된 프로필 팀원인데 처리하지 못했다. PIT 에는 이 Aid 의 용병이
    //                                     없으므로 PIT 도 부르지 않는다.
    // 해석기를 PIT 보다 먼저 묻는다(불변식 3): 용병과 프로필이 Aid 를 나눠 가지면 PIT 를 먼저 묻는 쪽이
    // 보이지 않는 용병을 고른다.
    public bool TrySetAutoJoin(MongoId sessionId, string? accountId, bool enabled, out string? unavailableReason)
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
            // 등록을 새로 만들거나 되살리지 않는다: Update 는 등록이 없으면 false 이고 아무것도 쓰지 않는다.
            if (!store.Update(sessionId, member.ProfileId, new StmProfileMemberChange(enabled, null)))
            {
                unavailableReason = "Unavailable";
                logger.Warning($"[STM] auto-join: 프로필 '{member.ProfileId}'(Aid '{accountId}')의 등록을 찾지 못해 저장하지 않았다.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // 식별된 뒤의 실패는 "읽을 수 없는 프로필 팀원" 이다 - PIT 로 넘기면 PIT 가 자기 not-found 를 던진다.
            // 식별 전의 실패만 PIT 경로로 흘린다.
            unavailableReason = identifiedAsMember ? "Unreadable" : null;
            logger.Error($"[STM] auto-join 저장: '{accountId}'(세션 '{sessionId}') 처리 실패 ({(identifiedAsMember ? "프로필 팀원, 읽을 수 없다고 보고" : "기본 경로로 넘긴다")}): {ex}");
            return false;
        }
    }

    // PIT 목록을 앞에 그대로 두고 그 뒤에 프로필 팀원 Aid 를 붙인다(기존 순서 불변). STM 읽기가 예기치 않게
    // 실패하면 Error 를 남기고 PIT 목록을 그대로 돌린다. 등록을 믿을 수 없는 상태(RegistrationsUsable == false)는
    // 예기치 않은 실패가 아니므로 로그 없이 PIT 목록을 돌린다(원인은 시작 검사가 이미 남겼다).
    public List<string> AppendAccountIds(MongoId sessionId, List<string> pitAccountIds)
    {
        try
        {
            // 등록을 믿을 수 없으면(시작 검사가 원인을 한 번 Error 로 남겼다) 호출마다 던졌다 잡아 로그를 되풀이하지
            // 않고 PIT 목록을 그대로 돌린다. Load 는 이 상태에서 던지므로 List 를 부르기 전에 묻는다.
            if (!store.RegistrationsUsable) return pitAccountIds;

            // 등록이 하나도 없으면(PIT 용병만 쓰는 사용자) 런처 프로필 카탈로그를 읽지 않고 PIT 목록을 그대로 돌린다.
            var registered = store.List(sessionId);
            if (registered.Count == 0) return pitAccountIds;

            var fields = registered.Select(m => new StmProfileMemberFields(m.ProfileId, m.Tactic, m.AutoJoin, m.HandEdited));
            var selected = AutoJoinRosterPolicy.SelectAccountIds(fields, catalog.GetCandidates(sessionId.ToString()));
            var result = new List<string>(pitAccountIds);
            var seen = new HashSet<string>(pitAccountIds, StringComparer.Ordinal);
            foreach (var id in selected)
            {
                if (seen.Add(id)) result.Add(id);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] auto-join 목록: 프로필 팀원을 붙이지 못해 PIT 목록만 돌린다 (세션 '{sessionId}'): {ex}");
            return pitAccountIds;
        }
    }
}
