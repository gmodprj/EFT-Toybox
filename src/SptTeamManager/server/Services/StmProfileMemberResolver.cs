using System.Globalization;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// WARNING: Pmc is the LIVE object SaveServer holds in memory, not a copy. Clone it before use
// (Task 3) and never mutate it: changing it alters the player's stored profile with no SaveProfile call in sight.
public sealed record StmResolvedMember(string ProfileId, int Aid, string Nickname, int Level, string Side, PmcData Pmc);

// Bridges the game's numeric Aid to a registered launcher-profile teammate and its PMC character.
// Called from inside PIT's invite/spawn handlers, so it must never throw: any failure is logged and
// returned as false so the caller can react. Read-only, and holds no profile data between calls
// (no fields besides injected services).
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
        // Boundary: false until the lookup has identified a registered Ready member. A failure before
        // that keeps a null reason (PIT's path); a failure after it is "Unreadable" (never handed to PIT).
        var identifiedAsMember = false;
        try
        {
            // Decimal digits only: no sign, no whitespace.
            if (!int.TryParse(accountId, NumberStyles.None, CultureInfo.InvariantCulture, out var aid) || aid <= 0) return false;

            // Fresh reads on every call. Never store these in a field.
            var registered = store.List(sessionId).Select(m => m.ProfileId).ToList();
            var candidates = catalog.GetCandidates(sessionId.ToString());
            var lookup = ProfileMemberLookup.ByAid(aid, registered, candidates);

            switch (lookup.Result)
            {
                case MemberLookupResult.NotAMember:
                    return false;
                case MemberLookupResult.Ready:
                    identifiedAsMember = true;
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
            // Read failed, or the profile no longer matches what the catalog said (Aid, Level, Side):
            // unusable. No substitute values - Task 3 builds gear from Level and Side.
            // Aid collision note: PIT's mercenary-aid allocator excludes used aids by reading
            // profile.ProfileInfo?.Aid (GetUniqueAccountId, FriendlyTeammateService.cs:4325-4328) while this matches on
            // pmc.Aid - the same value in practice, so a mercenary never takes a launcher profile's
            // aid, but the two reads are different fields and that closure is incidental, not guaranteed.
            if (pmc == null || info == null || pmc.Aid != aid || info.Level == null || string.IsNullOrWhiteSpace(info.Side))
            {
                unavailableReason = "Unreadable";
                return false;
            }

            var nickname = string.IsNullOrWhiteSpace(info.Nickname) ? aid.ToString(CultureInfo.InvariantCulture) : info.Nickname;
            member = new StmResolvedMember(lookup.ProfileId, aid, nickname, info.Level.Value, info.Side, pmc);
            return true;
        }
        catch (Exception ex)
        {
            member = null;
            unavailableReason = identifiedAsMember ? "Unreadable" : null;
            logger.Error($"[STM] Resolving account '{accountId}' failed ({(identifiedAsMember ? "profile member, reported unreadable" : "falling back to the default path")}): {ex}");
            return false;
        }
    }
}
