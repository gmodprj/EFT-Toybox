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
    // true  = 프로필 팀원이다. 호출자는 PIT 를 부르지 않고 message 로 오류 응답을 돌린다.
    // false = 프로필 팀원이 아니다(또는 식별 전에 실패했다). 호출자는 PIT 경로를 그대로 탄다.
    public bool TryRefuse(MongoId sessionId, string? accountId, ProfileMemberOperation operation, out string message)
    {
        message = string.Empty;
        try
        {
            var resolved = resolver.TryResolve(sessionId, accountId, out var member, out var unavailableReason);
            if (!(resolved && member != null) && string.IsNullOrWhiteSpace(unavailableReason))
            {
                return false;
            }

            message = ProfileMemberOperationPolicy.RefusalReason(operation);
            logger.Info($"[STM] 프로필 팀원(Aid '{accountId}')에게 온 {operation} 요청을 거부했다.");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 프로필 팀원 여부를 확인하지 못했다(Aid '{accountId}', {operation}). 기본 경로로 넘긴다: {ex}");
            return false;
        }
    }
}
