using pitTeam.Server.Models;
using pitTeam.Server.Services;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class StmProfileMemberCallbacks(HttpResponseUtil httpResponse, StmProfileMemberStore store, StmProfileCatalog catalog,
    StmTransferJournal journal, ISptLogger<StmProfileMemberCallbacks> logger)
{
    public ValueTask<string> List(string url, EmptyRequestData request, MongoId sessionId) =>
        Run("list", () => store.List(sessionId));

    // Roster screen data. The registration list is joined against a fresh catalog read on every
    // call (store.List does not reconcile). Nothing is cached or written: a copy of this join
    // would be the "ghost asset" entrance (design 4).
    public ValueTask<string> Roster(string url, EmptyRequestData request, MongoId sessionId) =>
        Run("roster", () =>
        {
            var registered = store.List(sessionId);
            var registeredIds = registered.Select(m => m.ProfileId).ToList();
            // 로스터 줄의 autoJoin 은 등록 정보에서 채운다(handEdited 는 이 줄이 채우지 않는다 - 뒤 작업의 몫).
            var autoJoinIds = registered.Where(m => m.AutoJoin).Select(m => m.ProfileId).ToHashSet(StringComparer.Ordinal);
            var candidates = catalog.GetCandidates(sessionId.ToString());
            return new StmRosterResponse
            {
                Members = ProfileRosterPolicy.JoinRegistered(candidates, registeredIds)
                    .Select(r => new StmRosterRow
                    {
                        ProfileId = r.ProfileId, Aid = r.Aid, Nickname = r.Nickname, Level = r.Level,
                        Side = r.Side, Status = r.Status.ToString(), AutoJoin = autoJoinIds.Contains(r.ProfileId),
                    }).ToList(),
                Candidates = ProfileRosterPolicy.SelectUnregisteredReady(candidates, registeredIds)
                    .Select(c => new StmRosterRow
                    {
                        ProfileId = c.ProfileId, Aid = c.Aid, Nickname = c.Nickname, Level = c.Level,
                        Side = c.Side, Status = RosterRowStatus.Ready.ToString(),
                    }).ToList(),
            };
        });

    public ValueTask<string> Add(string url, StmProfileMemberRequest request, MongoId sessionId) =>
        Run("add", () =>
        {
            store.Add(sessionId, request.ProfileId ?? string.Empty);
            return store.List(sessionId);
        });

    public ValueTask<string> Remove(string url, StmProfileMemberRequest request, MongoId sessionId) =>
        Run("remove", () =>
        {
            store.Remove(sessionId, request.ProfileId ?? string.Empty);
            return store.List(sessionId);
        });

    // 멈추고 보고한 전송 항목을 플레이어가 직접 닫는다(상세 화면의 닫기 확인창 뒤에만 불린다). 이 길은 STM 의 것이고
    // EFT 의 기존 라우트를 가로채지 않는다. 닫을 수 있는지의 판정은 서버가 다시 한다(TransferEntryClosePolicy):
    // acknowledged 가 true 여도 기동 때 보고되지 않은 항목이나 지금 일관된 항목은 거절된다. 거절 사유는 플레이어가
    // 읽는 말이고, 실패는 성공처럼 보이지 않는다(오류 본문).
    public ValueTask<string> CloseTransfer(string url, StmCloseTransferRequest request, MongoId sessionId)
    {
        if (journal.TryCloseByPlayer(sessionId, request.JournalId ?? string.Empty, request.ProfileId ?? string.Empty, request.Acknowledged, out var message))
        {
            return new ValueTask<string>(httpResponse.NullResponse());
        }

        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // Failure never reads as success: the client gets an error body with a key, not an empty list.
    private ValueTask<string> Run<T>(string action, Func<T> work)
    {
        try { return new ValueTask<string>(httpResponse.GetBody(work())); }
        catch (Exception ex)
        {
            logger.Warning($"[STM] Profile member '{action}' failed: {ex}");
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError,
                errmsg: ex is InvalidOperationException ? ex.Message : "StmProfileMemberFailed"));
        }
    }
}
