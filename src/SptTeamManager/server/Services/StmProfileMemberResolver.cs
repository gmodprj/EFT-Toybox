using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

public sealed record StmResolvedMember(string ProfileId, int Aid, string Nickname, int Level, string Side, PmcData Pmc);

// Bridges the game's numeric Aid to a registered launcher-profile teammate and its PMC character.
// Called from inside PIT's invite/spawn handlers, so it must never throw: any failure is logged and
// returned as false so the caller falls through to PIT's existing path.
// Read-only, and holds no profile data between calls (no fields besides injected services).
[Injectable(InjectionType.Singleton)]
public class StmProfileMemberResolver(
    StmProfileCatalog catalog,
    StmProfileMemberStore store,
    SaveServer saveServer,
    ISptLogger<StmProfileMemberResolver> logger)
{
    // true  = resolved, member is set.
    // false + unavailableReason == null  = not a profile teammate (caller continues with PIT's path).
    // false + unavailableReason != null  = a profile teammate that must not be taken (or could not be read).
    public bool TryResolve(MongoId sessionId, string? accountId, out StmResolvedMember? member, out string? unavailableReason)
    {
        member = null;
        unavailableReason = null;
        try
        {
            if (!int.TryParse(accountId, out var aid) || aid <= 0) return false;

            // Fresh reads on every call. Never store these in a field.
            var registered = store.List(sessionId).Select(m => m.ProfileId).ToList();
            var candidates = catalog.GetCandidates(sessionId.ToString());
            var lookup = ProfileMemberLookup.ByAid(aid, registered, candidates);

            switch (lookup.Result)
            {
                case MemberLookupResult.NotAMember:
                    return false;
                case MemberLookupResult.Ready:
                    break;
                default:
                    // NotAvailable, or any value this code does not know: fail closed.
                    unavailableReason = lookup.Availability?.ToString() ?? "Unavailable";
                    return false;
            }

            if (string.IsNullOrWhiteSpace(lookup.ProfileId))
            {
                unavailableReason = "Unreadable";
                return false;
            }

            var profile = saveServer.GetProfile(new MongoId(lookup.ProfileId));
            var pmc = profile?.CharacterData?.PmcData;
            var info = pmc?.Info;
            if (pmc == null || info == null || pmc.Aid != aid)
            {
                // Read failed or the profile no longer matches what the catalog said: unusable, no guessing.
                unavailableReason = "Unreadable";
                return false;
            }

            member = new StmResolvedMember(lookup.ProfileId, aid, info.Nickname ?? string.Empty, info.Level ?? 0, info.Side ?? string.Empty, pmc);
            return true;
        }
        catch (Exception ex)
        {
            member = null;
            // Unknown state: not claimed as a profile teammate, so PIT's own path runs unchanged.
            unavailableReason = null;
            logger.Error($"[STM] Resolving account '{accountId}' failed; falling back to the default path: {ex.Message}");
            return false;
        }
    }
}
