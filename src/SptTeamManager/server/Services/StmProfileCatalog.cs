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
        // Fail closed independently of the policy: without a session id nothing may be offered.
        if (string.IsNullOrWhiteSpace(currentSessionId))
        {
            logger.Error("[STM] GetCandidates called with a blank session id; the logged-in profile is unknown, so no candidates are offered.");
            return Array.Empty<ProfileCandidate>();
        }
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
            // A null profile is not "empty": we could not read it, so it fails closed as unreadable.
            if (profile == null) return new ProfileSnapshot(profileId, 0, null, null, null, true);
            var pmc = profile.CharacterData?.PmcData;
            var info = pmc?.Info;
            if (pmc == null || info == null) return new ProfileSnapshot(profileId, 0, null, null, null, false);
            // A played profile always has an Aid; its absence means it did not read cleanly. No substitute value.
            if (pmc.Aid == null) return new ProfileSnapshot(profileId, 0, info.Nickname, info.Level, info.Side, true);
            // 레이드에 무엇을 입고 나가는지는 입은 슬롯에서 나온다. 읽기만 한다 - 살아 있는 프로필 객체다.
            // 슬롯을 읽는 코드는 스폰 공장의 것을 그대로 쓴다(집이 둘이 되면 두 화면이 다른 말을 한다).
            var facts = SpawnGearPolicy.FromOccupiedSlots(StmSpawnProfileFactory.ReadOccupiedEquipmentSlots(pmc));
            var gear = new ProfileGear(facts.HasAnyEquipment, facts.HasWeaponInRequiredSlot);
            return new ProfileSnapshot(profileId, pmc.Aid.Value, info.Nickname, info.Level, info.Side, false, gear);
        }
        catch (Exception ex)
        {
            // One bad profile is marked unreadable; the rest of the enumeration continues.
            logger.Warning($"[STM] Profile '{profileId}' could not be read and is listed as unavailable: {ex.Message}");
            return new ProfileSnapshot(profileId, 0, null, null, null, true);
        }
    }
}
