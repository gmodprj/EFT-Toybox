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
using CommonCustomization = SPTarkov.Server.Core.Models.Eft.Common.Tables.Customization;
using CommonInfo = SPTarkov.Server.Core.Models.Eft.Common.Tables.Info;

namespace pitTeam.Server.Services;

// Turns a resolved launcher-profile teammate into the BotBase the spawn path hands to the game.
//
// member.Pmc is the LIVE object SaveServer holds (see StmProfileMemberResolver). Nothing here writes
// through it: the gear facts are read-only, Real builds from cloner.Clone, and Temporary copies
// cloned identity FROM the profile ONTO the generated bot - never the other way round, and never a
// reference into the profile's object graph.
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

        // Either way the raid must see the profile's own identity.
        spawn.Id = new MongoId(member.ProfileId);
        spawn.Aid = member.Aid;
        EnsurePmcRole(spawn, member.Side);

        // The minimum a spawned follower needs in order not to die on the client. PIT's
        // mercenary-only tweaks (secure-container supplies, the scabbard knife, magazine refills)
        // are NOT applied - they rewrite real player gear.
        teammateService.EnsureFollowerHasPockets(spawn);
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

    // Temporary: PIT's generator supplies the loadout; the profile supplies who is wearing it.
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

        // Identity is copied FROM the profile ONTO the bot, and cloned, so no part of the live
        // profile's object graph leaves this method.
        bot.Info = cloner.Clone(member.Pmc?.Info) ?? new CommonInfo();
        bot.Customization = cloner.Clone(member.Pmc?.Customization) ?? new CommonCustomization();

        // Nothing this teammate carries is insured: the gear is a generated loaner, and the
        // profile's own insured items are not in this raid at all. Keeping InsuredItems would assert
        // the opposite about item ids that no longer exist in the inventory, so it is emptied.
        // This cannot reach the player's stored profile: `bot` is the generator's object and its
        // Info/Customization are clones, so member.Pmc is untouched - and on the Real path below
        // InsuredItems is deliberately left alone, where the real items genuinely are in the raid.
        // Teammate insurance does not live here anyway; it is kept in
        // FriendlyTeammateSettings.InsuredItems (server/Models/FriendlyTeammateSettings.cs:8),
        // so emptying this costs the insurance feature nothing.
        bot.InsuredItems = [];
        return bot;
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
