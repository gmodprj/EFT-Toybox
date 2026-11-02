using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace pitTeam.Server.Services;

// 상세 화면의 용병 전용 조작(이름·외형·공격성·숙련·전술·보험·프리셋)이 프로필 팀원의 Aid 로 불렸을 때 PIT 에 닿기 전에
// 거부한다. 조용히 성공한 척하지 않는다: PIT 로 넘기면 (a) 용병이 없어 not-found 로 던지거나 (b) 같은 Aid 의 용병이
// 있으면 화면에 보이지 않는 용병이 바뀐다.
//
// 해석기를 PIT 보다 먼저 묻는다(불변식 3) - 같은 Aid 를 용병과 프로필이 나눠 가지면 사용자가 본 것은 프로필이다.
// 등록됐지만 지금 쓸 수 없는 프로필(로그인 중·읽을 수 없음)도 거부다: PIT 에는 그 Aid 의 용병이 없다.
// 던지지 않는다(PIT 콜백 안에서 불린다).
[Injectable(InjectionType.Singleton)]
public class StmProfileOperationGuard(
    StmProfileMemberResolver resolver,
    FriendlyTeammateService teammateService,
    FriendlyRecruitService recruitService,
    ISptLogger<StmProfileOperationGuard> logger)
{
    // PIT 가 이 Aid 를 자기 용병으로 아는가(대기 중·합류한 용병·모집 대상). 조회 라우트의 PitKnowsAid 와 같은 세 곳을 묻는다.
    private bool PitOwnsAid(MongoId sessionId, string? accountId) =>
        (teammateService.TryGetPendingTeammateProfile(sessionId, accountId, out var pending) && pending != null)
        || (teammateService.TryGetTeammateProfile(sessionId, accountId, out var mercenary) && mercenary != null)
        || (recruitService.TryGetRecruitProfile(sessionId, accountId, out var recruit) && recruit != null);

    // true  = 프로필 팀원이거나, 프로필 팀원인지 알 수 없다. 호출자는 PIT 를 부르지 않고 message 로 오류 응답을 돌린다.
    // false = 프로필 팀원이 아님이 확인됐다. 호출자는 PIT 경로를 그대로 탄다.
    // 가드는 모를 때 거부 쪽으로 닫는다. 단 하나의 탈출구가 있다(ProfileViewPolicy.RouteEditAfterLookup): 등록을 읽을 수 없어도 PIT 가
    // 자기 용병으로 아는 Aid 면 PIT 로 넘긴다. 이게 없으면 등록 문서 하나의 손상이 PIT 용병 조작 열한 가지를 전부 막는다(릴리스 차단).
    // 읽을 수 없고 PIT 의 용병도 아닌 Aid 는 등록된 프로필일 수 있어 계속 거절한다 - 보이지 않는 용병을 고치면 장비가 사라질 수 있다.
    public bool TryRefuse(MongoId sessionId, string? accountId, ProfileMemberOperation operation, out string message)
    {
        message = string.Empty;
        try
        {
            // 읽기는 해석기(Lookup)가 한 번만 하고 결과를 한 번만 분류한다. 확인된 비팀원만 PIT 로 보낸다.
            var outcome = resolver.Lookup(sessionId, accountId, out _, out _);
            var route = ProfileViewPolicy.RouteEditAfterLookup(outcome, () => PitOwnsAid(sessionId, accountId));
            if (route == ProfileEditRoute.PassToPit)
            {
                if (outcome == ProfileLookupOutcome.ReadFailed)
                {
                    logger.Warning($"[STM] 등록을 읽을 수 없지만 Aid '{accountId}' 는 PIT 용병이라 {operation} 요청을 PIT 로 넘긴다.");
                }

                return false;
            }

            // 읽기 실패(판별 불가)는 "아직 모른다" 는 문구로, 그 밖(팀원·쓸 수 없는 팀원)은 이 조작이 왜 뜻이 없는지로 거부한다.
            message = route == ProfileEditRoute.RefuseUndetermined
                ? ProfileMemberOperationPolicy.UndeterminedReason(operation)
                : ProfileMemberOperationPolicy.RefusalReason(operation);
            logger.Info($"[STM] 프로필 팀원(Aid '{accountId}')에게 온 {operation} 요청을 거부했다.");
            return true;
        }
        catch (Exception ex)
        {
            message = ProfileMemberOperationPolicy.UndeterminedReason(operation);
            logger.Error($"[STM] 프로필 팀원 여부를 확인하지 못해(Aid '{accountId}', {operation}) 조작을 거부한다: {ex}");
            return true;
        }
    }
}
