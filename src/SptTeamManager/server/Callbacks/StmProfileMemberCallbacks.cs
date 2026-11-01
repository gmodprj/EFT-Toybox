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
    ISptLogger<StmProfileMemberCallbacks> logger)
{
    public ValueTask<string> List(string url, EmptyRequestData request, MongoId sessionId) =>
        Run("list", () => store.List(sessionId));

    // Roster screen data. The registration list is joined against a fresh catalog read on every
    // call (store.List does not reconcile). Nothing is cached or written: a copy of this join
    // would be the "ghost asset" entrance (design 4).
    public ValueTask<string> Roster(string url, EmptyRequestData request, MongoId sessionId) =>
        Run("roster", () =>
        {
            var registeredIds = store.List(sessionId).Select(m => m.ProfileId).ToList();
            var candidates = catalog.GetCandidates(sessionId.ToString());
            return new StmRosterResponse
            {
                Members = ProfileRosterPolicy.JoinRegistered(candidates, registeredIds)
                    .Select(r => new StmRosterRow
                    {
                        ProfileId = r.ProfileId, Aid = r.Aid, Nickname = r.Nickname, Level = r.Level,
                        Side = r.Side, Status = r.Status.ToString(),
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
