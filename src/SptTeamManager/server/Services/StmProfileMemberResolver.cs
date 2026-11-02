using System.Globalization;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

/// WARNING: Pmc is the LIVE object SaveServer holds in memory, not a copy. Clone it before use
/// (Task 3) and never mutate it: changing it alters the player's stored profile with no SaveProfile call in sight.
/// (Doc comment on purpose, not a plain one: this is the field whose misuse corrupts a profile the
/// player plays, so the warning has to reach an IDE tooltip at the point of use.)
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
    //
    // 등록과 카탈로그를 읽는 곳은 이 클래스에서 단 한 군데(Lookup)이고, "읽기가 실패했다" 는 신호가 사라지는 경계도 없다.
    // 읽은 결과를 ResolveFrom 에 넘기므로 그 뒤에는 읽다 던질 길이 없다 - 이전에는 호출자가 미리 읽어 확인해도 해석기가 다시
    // 읽다 던지면 삼켜 "사유 없는 false"(= 확인된 비팀원과 같은 모양)가 됐다.
    public ProfileLookupOutcome Lookup(MongoId sessionId, string? accountId, out StmResolvedMember? member, out string? unavailableReason)
    {
        member = null;
        unavailableReason = null;
        IReadOnlyList<string> registered;
        IReadOnlyList<ProfileCandidate> candidates;
        try
        {
            // Fresh reads on every call. Never store these in a field.
            registered = store.List(sessionId).Select(m => m.ProfileId).ToList();
            candidates = catalog.GetCandidates(sessionId.ToString());
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] Resolving account '{accountId}': the registrations or the profile catalog could not be read, so it is unknown whether this is a profile member (reported as a read failure, not as a non-member): {ex}");
            return ProfileViewPolicy.ClassifyLookup(readFailed: true, resolved: false, unavailableReason: null);
        }

        var resolved = ResolveFrom(sessionId, accountId, registered, candidates, out member, out unavailableReason);
        return ProfileViewPolicy.ClassifyLookup(readFailed: false, resolved, unavailableReason);
    }

    // 기존 호출자(초대·스폰·결과 정산·auto-join)용. 읽기 실패는 예전처럼 "사유 없는 false" 로 돌린다 - 그 호출자들의 PIT 길에는
    // SPT 응답 꼬리가 없고, 이 작업의 범위가 아니다. 읽기는 Lookup 한 곳에서만 일어난다.
    public bool TryResolve(MongoId sessionId, string? accountId, out StmResolvedMember? member, out string? unavailableReason)
    {
        var outcome = Lookup(sessionId, accountId, out member, out unavailableReason);
        if (outcome == ProfileLookupOutcome.ReadFailed)
        {
            member = null;
            unavailableReason = null;
        }

        return outcome == ProfileLookupOutcome.Resolved && member != null;
    }

    private bool ResolveFrom(MongoId sessionId, string? accountId, IReadOnlyList<string> registered, IReadOnlyList<ProfileCandidate> candidates,
        out StmResolvedMember? member, out string? unavailableReason)
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
            // Read failed, or the profile no longer carries the Aid the catalog said: unusable. Only the
            // Aid is re-compared; Level and Side are checked for PRESENCE, not for agreement with the
            // catalog - they are read from this profile right here, so the fresher value is this one and
            // there is nothing to disagree with. What they may not be is missing: no substitute values,
            // because Task 3 builds gear from Level and Side.
            // Aid collision note: PIT's mercenary-aid allocator excludes used aids by reading
            // profile.ProfileInfo?.Aid (GetUniqueAccountId, FriendlyTeammateService.cs:4329 and :4366) while this matches on
            // pmc.Aid - the same value in practice, but the two reads are different fields, so that
            // closure is not guaranteed. It no longer has to be: every call site asks this resolver
            // BEFORE PIT's own lookup (invite and follower-generate in FriendlyTeammateMatchCallbacks,
            // outcome in FriendlyPostRaidCallbacks, progress in this file's callers), so an Aid claimed
            // by both resolves to the registered PROFILE - which is what the player's friend list showed
            // them. A collision costs PIT one unreachable mercenary, not an invisible spawn.
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
