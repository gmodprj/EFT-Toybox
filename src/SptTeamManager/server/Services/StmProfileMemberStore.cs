using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace pitTeam.Server.Services;

// Persists which launcher profiles the user registered as teammates, in PIT's encrypted
// per-session database. Only the registration fact is stored; profile data is read from the
// profile itself (via StmProfileCatalog) every time it is needed.
[Injectable(InjectionType.Singleton)]
public class StmProfileMemberStore(FriendlyTeammateStorage storage, StmProfileCatalog catalog)
{
    // Not a "{aid}.json" name, so TeammateDatabase.IsProfileDocument() never lists it as a teammate.
    private const string Document = "stm-profile-members.json";
    // Same defaults PIT gives a new teammate (FriendlyTeammateService.cs CombatTactic / AutoJoinEnabled).
    private const string DefaultTactic = "Rifleman";
    private const bool DefaultAutoJoin = false;

    private readonly object sync = new();

    public IReadOnlyList<StmProfileMember> List(MongoId sessionId)
    {
        lock (sync) return Load(sessionId);
    }

    public void Add(MongoId sessionId, string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            throw new InvalidOperationException("StmProfileIdMissing");
        profileId = profileId.Trim();
        // Judged against a fresh read of the launcher profiles. Anything that is not Ready
        // (empty, unreadable, the logged-in profile, or simply unknown) is refused.
        var candidate = catalog.GetCandidates(sessionId.ToString())
            .FirstOrDefault(c => string.Equals(c.ProfileId, profileId, StringComparison.Ordinal));
        if (candidate == null || candidate.Availability != ProfileAvailability.Ready)
            throw new InvalidOperationException("StmProfileNotAvailable");

        lock (sync)
        {
            var members = Load(sessionId).ToList();
            if (members.Any(m => m.ProfileId == candidate.ProfileId)) return;
            members.Add(new StmProfileMember(candidate.ProfileId, DefaultTactic, DefaultAutoJoin, false));
            Save(sessionId, members);
        }
    }

    public void Remove(MongoId sessionId, string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            throw new InvalidOperationException("StmProfileIdMissing");
        profileId = profileId.Trim();
        lock (sync)
        {
            var members = Load(sessionId).ToList();
            // Removal needs no availability check: a profile that went bad must still be removable.
            if (members.RemoveAll(m => m.ProfileId == profileId) == 0) return;
            Save(sessionId, members);
        }
    }

    // Changes fields of an existing registration. Returns false - and writes nothing - when the
    // profile is not registered, so a stale click can never create or resurrect a registration.
    public bool Update(MongoId sessionId, string profileId, StmProfileMemberChange change)
    {
        if (string.IsNullOrWhiteSpace(profileId)) return false;
        profileId = profileId.Trim();
        lock (sync)
        {
            var members = Load(sessionId).ToList();
            var index = members.FindIndex(m => m.ProfileId == profileId);
            if (index < 0) return false;
            var current = members[index];
            var updated = ProfileMemberUpdatePolicy.Apply(
                new StmProfileMemberFields(current.ProfileId, current.Tactic, current.AutoJoin, current.HandEdited), change);
            // Fail closed: a policy that cannot judge changes nothing.
            if (updated == null) return false;
            members[index] = new StmProfileMember(updated.ProfileId, updated.Tactic, updated.AutoJoin, updated.HandEdited);
            Save(sessionId, members);
            return true;
        }
    }

    // Per logged-in profile (keyed by sessionId) and independent of the Guns for Hire / Allegiance mode: the
    // team does not change with the mode. (Upstream calls this "account-wide"; it is not shared across profiles.)
    private List<StmProfileMember> Load(MongoId sessionId) =>
        storage.ReadSharedDocument<StmProfileMembersDocument>(sessionId, Document)?.Members ?? [];

    private void Save(MongoId sessionId, List<StmProfileMember> members) =>
        storage.WriteSharedDocument(sessionId, Document, new StmProfileMembersDocument { Members = members });
}
