using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// Thin adapter: translates SaveServer's profiles into plain snapshots for ProfileRosterPolicy.
// Read-only, and holds no profile data between calls - a profile created in the launcher
// afterwards must show up on the very next call.
[Injectable(InjectionType.Singleton)]
public class StmProfileCatalog(SaveServer saveServer, ISptLogger<StmProfileCatalog> logger)
{
    public IReadOnlyList<ProfileCandidate> GetCandidates(string currentSessionId)
    {
        var snapshots = new List<ProfileSnapshot>();
        // Fresh read on every call. Never store this dictionary or the snapshots in a field.
        foreach (var (id, profile) in saveServer.GetProfiles())
            snapshots.Add(ToSnapshot(id, profile));
        return ProfileRosterPolicy.Select(snapshots, currentSessionId);
    }

    private ProfileSnapshot ToSnapshot(MongoId id, SptProfile? profile)
    {
        string profileId = id.ToString();
        try
        {
            var pmc = profile?.CharacterData?.PmcData;
            var info = pmc?.Info;
            if (info == null) return new ProfileSnapshot(profileId, 0, null, null, null, false);
            return new ProfileSnapshot(profileId, pmc!.Aid ?? 0, info.Nickname, info.Level, info.Side, false);
        }
        catch (Exception ex)
        {
            // One bad profile is marked unreadable; the rest of the enumeration continues.
            logger.Warning($"[STM] Profile '{profileId}' could not be read and is listed as unavailable: {ex.Message}");
            return new ProfileSnapshot(profileId, 0, null, null, null, true);
        }
    }
}
