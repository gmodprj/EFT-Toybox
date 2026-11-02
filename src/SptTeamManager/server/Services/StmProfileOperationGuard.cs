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
public class StmProfileOperationGuard(StmProfileMemberResolver resolver, ISptLogger<StmProfileOperationGuard> logger)
{
    // true  = 프로필 팀원이거나, 프로필 팀원인지 알 수 없다. 호출자는 PIT 를 부르지 않고 message 로 오류 응답을 돌린다.
    // false = 프로필 팀원이 아님이 확인됐다. 호출자는 PIT 경로를 그대로 탄다.
    // 가드는 모를 때 거부 쪽으로 닫는다: 해석기는 등록·카탈로그를 못 읽으면 던졌다 잡고 사유 없이 "팀원 아님" 을 돌려주는데,
    // 그대로 믿으면 Aid 가 겹칠 때 PIT 가 보이지 않는 용병을 고친다. 그래서 읽기가 되는지를 먼저 직접 확인한다.
    public bool TryRefuse(MongoId sessionId, string? accountId, ProfileMemberOperation operation, out string message)
    {
        message = string.Empty;
        try
        {
            // 읽기는 해석기(Lookup)가 한 번만 하고 결과를 한 번만 분류한다. 확인된 비팀원만 PIT 로 보낸다.
            var outcome = resolver.Lookup(sessionId, accountId, out _, out _);
            if (outcome == ProfileLookupOutcome.NonMember)
            {
                return false;
            }

            // 읽기 실패(판별 불가)는 "아직 모른다" 는 문구로, 그 밖(팀원·쓸 수 없는 팀원)은 이 조작이 왜 뜻이 없는지로 거부한다.
            message = outcome == ProfileLookupOutcome.ReadFailed
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
