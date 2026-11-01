using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Utils.Cloners;
using CommonInfo = SPTarkov.Server.Core.Models.Eft.Common.Tables.Info;

namespace pitTeam.Server.Services;

// Turns a resolved launcher-profile teammate into the BotBase the spawn path hands to the game.
//
// member.Pmc is the LIVE object SaveServer holds (see StmProfileMemberResolver). Nothing here writes
// through it: reading the gear facts mutates nothing, and BOTH paths build on cloner.Clone, so no
// reference into the profile's object graph is ever handed out. Clone is a deep clone: SPT's ICloner
// is FastCloner, which calls FastCloner 3.5.5's DeepClone, so later writes to the clone cannot reach
// the stored profile. Temporary swaps the clone's whole Inventory for a generated bot's, and empties
// InsuredItems because that swap takes the insured items away; nothing else differs from Real.
[Injectable(InjectionType.Singleton)]
public class StmSpawnProfileFactory(
    // FriendlyTeammateService is [Injectable] with SPT's default InjectionType.Transient, so this
    // singleton captures one instance for the server's lifetime. That is deliberate: it is legal in
    // this container (only Scoped -> Singleton is not), and the alternative was ~107 lines of copied
    // scaffolding that had already diverged from PIT's. Only GenerateTeammateBot and
    // EnsureFollowerHasPockets are used, and both must stay free of PIT's mutable instance state
    // (profileRecoveryNotices, startupRecoveryNotices, duplicateRecoveryCheckedSessions -
    // FriendlyTeammateService.cs:74-76): they use only injected services, statics and their
    // arguments today, and a future edit that made either touch those fields would break this.
    FriendlyTeammateService teammateService,
    ICloner cloner,
    ISptLogger<StmSpawnProfileFactory> logger)
{
    // Same generation map PIT uses for menu-created teammates.
    private const string GenerationLocation = "factory4_day";

    // Field, deliberately. Task 2 forbade fields because cached PROFILE data goes stale; this records
    // what WE did at spawn time, which exists nowhere else - the raid-end report does not carry it,
    // so Task 8 cannot tell "died in its own gear" from "died in a generated loadout" without it.
    // Concurrent because SPT handles requests concurrently. Last write for a key wins.
    // A server restart between spawn and raid end loses it; LastGearKind then returns null and the
    // caller's policy falls through to "do nothing" rather than stripping gear on a guess.
    private readonly ConcurrentDictionary<string, SpawnGearKind> lastGearKinds = new(StringComparer.Ordinal);

    // The source of the slot-name strings: SPT's own enum, which is why it lives here and not in
    // the policy. Which of these names counts as worn equipment, or as a weapon slot, is the
    // policy's judgment (SpawnGearPolicy.FromOccupiedSlots).
    private static readonly HashSet<string> EquipmentSlotNames =
        new(Enum.GetNames(typeof(EquipmentSlots)), StringComparer.OrdinalIgnoreCase);

    // healthMultiplier is required, not optional: it arrives from the client's own BepInEx setting on
    // the request (PIT's FriendlyTeammateService.TryGetSpawnProfile), so a caller that forgets it
    // would ship a teammate whose health setting silently does nothing. The compiler forces the call.
    public BotBase Build(MongoId sessionId, StmResolvedMember member, out SpawnGearKind kind, double? healthMultiplier)
    {
        if (member is null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        var facts = SpawnGearPolicy.FromOccupiedSlots(ReadOccupiedEquipmentSlots(member.Pmc));
        var decision = SpawnGearPolicy.Decide(facts.HasAnyEquipment, facts.HasWeaponInRequiredSlot);
        kind = decision.Kind;

        var spawn = decision.Kind == SpawnGearKind.Real
            ? BuildFromProfile(member)
            : BuildFromGenerator(sessionId, member);

        // Both paths are profile clones, so these already hold the profile's values; setting them
        // keeps the guarantee local instead of depending on how `spawn` was built.
        // Must equal the Id the invite announced (FriendlyTeammateMatchCallbacks.BuildProfileMemberAccept); member.ProfileId is the source of truth.
        spawn.Id = new MongoId(member.ProfileId);
        spawn.Aid = member.Aid;
        EnsurePmcRole(spawn, member.Side);

        // PIT applies five tweaks before a mercenary spawns. We take three and skip two, and the reason
        // differs per tweak rather than per gear kind:
        //
        //   taken   - EnsureFollowerHasPockets, EnsureFollowerHasScabbardKnife and
        //             RefillFollowerMagazinesFromInventoryAmmo. All three only ADD to the object handed in,
        //             and `spawn` is a deep clone, so the stored profile cannot be reached. PIT's own
        //             comment on the refill says it is kept "strictly on the raid-spawn clone" so that
        //             profile viewing and loadout editing do not silently change teammate gear
        //             (FriendlyTeammateService.PrepareTeammateForFetch) - the same guarantee we need.
        //   skipped - EnsureManagedSecureContainer and EnsureFollowerHasSecureContainerSupplies, which
        //             EMPTY the secure container and restock it with mercenary supplies. On a real
        //             profile's kit that discards what the player put there, so a profile teammate goes
        //             in with their own container exactly as it stands.
        //
        // The refill is not cosmetic: without it a teammate spawns with whatever its magazines held, and
        // a profile parked between raids usually holds nothing. Measured in game on 2026-11-01 - two
        // Real-gear teammates entered with 0-round primary magazines and could not return fire.
        teammateService.EnsureFollowerHasPockets(spawn);
        teammateService.EnsureFollowerHasScabbardKnife(spawn);
        teammateService.RefillFollowerMagazinesFromInventoryAmmo(spawn);
        ApplyHealthMultiplier(spawn, healthMultiplier);

        lastGearKinds[GearKindKey(sessionId, member.ProfileId)] = decision.Kind;
        logger.Info($"[STM] '{member.Nickname}'(aid {member.Aid}) 를 {decision.Kind} 장비로 내보낸다: {decision.Reason}");
        return spawn;
    }

    // null = we have no record (never spawned, or the server restarted since). Not "Temporary".
    public SpawnGearKind? LastGearKind(MongoId sessionId, string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        return lastGearKinds.TryGetValue(GearKindKey(sessionId, profileId), out var kind) ? kind : null;
    }

    private static string GearKindKey(MongoId sessionId, string profileId) =>
        sessionId.ToString() + "/" + profileId.Trim();

    // Real: the profile's own gear goes in. Clone first - the live object must not be handed out.
    private BotBase BuildFromProfile(StmResolvedMember member)
    {
        return cloner.Clone(member.Pmc)
            ?? throw new FriendlyTeammateException($"Cloning the PMC profile of '{member.ProfileId}' returned nothing");
    }

    // Temporary: the teammate is still the profile's own character. The generated bot contributes
    // nothing but its Inventory.
    private BotBase BuildFromGenerator(MongoId sessionId, StmResolvedMember member)
    {
        var level = Math.Max(1, member.Level);
        var details = new BotGenerationDetails
        {
            IsPmc = true,
            Side = member.Side,
            Role = PmcRoleFor(member.Side),
            PlayerLevel = level,
            PlayerName = member.Nickname,
            BotRelativeLevelDeltaMax = 0,
            BotRelativeLevelDeltaMin = 0,
            BotCountToGenerate = 1,
            BotDifficulty = "hard",
            Location = GenerationLocation,
            // Pinned to the profile's level: the teammate must not outgrow or undercut the person.
            LocationSpecificPmcLevelOverride = new MinMax<int> { Min = level, Max = level },
            IsPlayerScav = false,
            AllPmcsHaveSameNameAsPlayer = false,
        };

        // PIT's own call, which wraps the generator in the raid-configuration scaffolding the
        // generator needs outside a raid. Reused rather than copied.
        var bot = teammateService.GenerateTeammateBot(sessionId, details)
            ?? throw new FriendlyTeammateException($"Bot generation for profile '{member.ProfileId}' returned nothing");

        // What Temporary changes is the Inventory, plus the insurance list below that the swap
        // invalidates - nothing else. The spawn is the same profile clone the Real path builds, so
        // Health, Skills, Stats, Quests and the rest stay the
        // person's - a loaner kit must not turn the teammate into somebody else, and the caller's
        // health multiplier has to mean the same thing on both paths.
        var spawn = BuildFromProfile(member);

        // That one respect is the WHOLE Inventory object, stash included. Everything pointing at
        // item ids lives inside it - Stash, SortingTable, FastPanel, FavoriteItems, QuestRaidItems,
        // QuestStashItems, HideoutAreaStashes - so all of those are replaced wholesale by the
        // generated inventory's own rather than left pointing at items that are gone. The profile's
        // stash therefore does not travel into the raid, which is the intent, not a side effect.
        spawn.Inventory = bot.Inventory;

        // InsuredItems is the one thing OUTSIDE Inventory that stores Inventory.Items ids, so the
        // transplant above just took its items away. Keeping the list would assert "these items in
        // this raid are insured" about items that are not in the raid at all; emptying it asserts
        // what is true - the teammate is wearing a loaner and nothing here is insured.
        // This cannot reach the player's stored profile: `spawn` is a clone, so member.Pmc is
        // untouched. The Real path deliberately leaves the list alone, where the real items
        // genuinely are in the raid. Teammate insurance does not live here anyway; it is kept in
        // FriendlyTeammateSettings.InsuredItems (server/Models/FriendlyTeammateSettings.cs:8),
        // so emptying this costs the insurance feature nothing.
        spawn.InsuredItems = [];
        return spawn;
    }

    // Reads which equipment slots the profile has something in, and nothing more - the judgment is
    // the policy's. Read-only: never assigns Inventory.Equipment, unlike PIT's GetEquipmentRootId,
    // because the profile passed in may be the live object.
    private static List<string> ReadOccupiedEquipmentSlots(BotBase? profile)
    {
        var occupied = new List<string>();
        var items = profile?.Inventory?.Items;
        if (items == null || items.Count == 0)
        {
            return occupied;
        }

        // An equipment-slot item is a direct child of the equipment root whose SlotId is one of
        // SPT's EquipmentSlots names. Stash items and items inside pockets are not.
        var rootId = ReadEquipmentRootId(profile!);

        foreach (var item in items)
        {
            if (item?.SlotId is null || !EquipmentSlotNames.Contains(item.SlotId))
            {
                continue;
            }

            if (rootId != null && !string.Equals(item.ParentId, rootId, StringComparison.Ordinal))
            {
                continue;
            }

            occupied.Add(item.SlotId);
        }

        return occupied;
    }

    private static string? ReadEquipmentRootId(BotBase profile)
    {
        var rootId = profile.Inventory?.Equipment?.ToString();
        if (!string.IsNullOrWhiteSpace(rootId))
        {
            return rootId;
        }

        var rootItem = profile.Inventory?.Items?.FirstOrDefault();
        return rootItem?.Id.ToString();
    }

    // A human's PMC may have no Info.Settings, or a blank Role, and the client dereferences it.
    private static void EnsurePmcRole(BotBase spawn, string side)
    {
        spawn.Info ??= new CommonInfo();
        spawn.Info.Settings ??= new BotInfoSettings();
        if (string.IsNullOrWhiteSpace(spawn.Info.Settings.Role))
        {
            spawn.Info.Settings.Role = PmcRoleFor(side);
        }
    }

    private static string PmcRoleFor(string? side)
    {
        return side switch
        {
            Sides.Usec => Sides.PmcUsec,
            Sides.Bear => Sides.PmcBear,
            _ => throw new FriendlyTeammateException($"Unsupported profile teammate side '{side}'"),
        };
    }

    private static void ApplyHealthMultiplier(BotBase spawn, double? healthMultiplier)
    {
        if (healthMultiplier is null || Math.Abs(healthMultiplier.Value - 1d) < 0.0001d)
        {
            return;
        }

        var bodyParts = spawn.Health?.BodyParts;
        if (bodyParts == null)
        {
            return;
        }

        foreach (var bodyPart in bodyParts.Values)
        {
            var health = bodyPart?.Health;
            if (health?.Maximum == null)
            {
                continue;
            }

            health.Maximum *= healthMultiplier.Value;
            health.Current = health.Maximum;
        }
    }
}
