using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Server.Constants;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Generators.Bot;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.RaidSettings;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Services.Profile;
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
    BotGenerator botGenerator,
    ProfileActivityService profileActivityService,
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

    private static readonly HashSet<string> EquipmentSlotNames =
        new(Enum.GetNames(typeof(EquipmentSlots)), StringComparer.OrdinalIgnoreCase);

    // "필수 무기 슬롯": any one of these holding an item satisfies it. A knife (Scabbard) does not.
    private static readonly string[] RequiredWeaponSlotNames =
    [
        nameof(EquipmentSlots.FirstPrimaryWeapon),
        nameof(EquipmentSlots.SecondPrimaryWeapon),
        nameof(EquipmentSlots.Holster),
    ];

    // healthMultiplier mirrors PIT's spawn path (FriendlyTeammateService.TryGetSpawnProfile); the
    // brief's three-argument signature stays callable because it is optional.
    public BotBase Build(MongoId sessionId, StmResolvedMember member, out SpawnGearKind kind, double? healthMultiplier = null)
    {
        if (member is null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        var (hasAnyEquipment, hasWeaponInRequiredSlot) = ReadGearFacts(member.Pmc);
        var decision = SpawnGearPolicy.Decide(hasAnyEquipment, hasWeaponInRequiredSlot);
        kind = decision.Kind;

        var spawn = decision.Kind == SpawnGearKind.Real
            ? BuildFromProfile(member)
            : BuildFromGenerator(sessionId, member);

        // Either way the raid must see the profile's own identity.
        spawn.Id = new MongoId(member.ProfileId);
        spawn.Aid = member.Aid;
        EnsurePmcRole(spawn, member.Side);
        EnsurePockets(spawn);
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

        var bot = GenerateBot(sessionId, details)
            ?? throw new FriendlyTeammateException($"Bot generation for profile '{member.ProfileId}' returned nothing");

        // Identity is copied FROM the profile ONTO the bot, and cloned, so no part of the live
        // profile's object graph leaves this method.
        bot.Info = cloner.Clone(member.Pmc?.Info) ?? new CommonInfo();
        bot.Customization = cloner.Clone(member.Pmc?.Customization) ?? new CommonCustomization();
        return bot;
    }

    // Same call as PIT's FriendlyTeammateService.GenerateTeammateBot: the generator reads the
    // session's raid configuration, which is absent outside a raid, so a throwaway one stands in
    // and the previous value is always put back.
    private BotBase? GenerateBot(MongoId sessionId, BotGenerationDetails details)
    {
        var raidData = profileActivityService.GetProfileActivityRaidData(sessionId);
        var previousRaidConfiguration = raidData.RaidConfiguration;
        var createdTemporaryRaidConfiguration = previousRaidConfiguration == null;

        if (createdTemporaryRaidConfiguration)
        {
            raidData.RaidConfiguration = CreateGenerationRaidConfiguration();
        }

        try
        {
            return botGenerator.PrepareAndGenerateBot(sessionId, details);
        }
        finally
        {
            if (createdTemporaryRaidConfiguration)
            {
                raidData.RaidConfiguration = previousRaidConfiguration;
            }
        }
    }

    private static GetRaidConfigurationRequestData CreateGenerationRaidConfiguration()
    {
        return new GetRaidConfigurationRequestData
        {
            Location = GenerationLocation,
            TimeVariant = DateTimeEnum.CURR,
            IsNightRaid = false,
            RaidMode = RaidMode.Local,
            Side = SideType.Pmc,
            PlayersSpawnPlace = PlayersSpawnPlace.SamePlace,
            TransitionType = TransitionType.NONE,
            WavesSettings = new WavesSettings
            {
                BotAmount = BotAmount.AsOnline,
                BotDifficulty = BotDifficulty.Hard,
                IsBosses = true,
                IsTaggedAndCursed = false,
            },
            BotSettings = new BotSettings
            {
                BotAmount = BotAmount.AsOnline,
                IsScavWars = false,
            },
            TimeAndWeatherSettings = new TimeAndWeatherSettings
            {
                IsRandomTime = false,
                IsRandomWeather = false,
            },
            IsLocationTransition = false,
            CanShowGroupPreview = false,
            OnlinePveRaidStates = [],
        };
    }

    // Read-only. Never assigns Inventory.Equipment, unlike PIT's GetEquipmentRootId, because the
    // profile passed in may be the live object.
    private static (bool HasAnyEquipment, bool HasWeaponInRequiredSlot) ReadGearFacts(BotBase? profile)
    {
        var items = profile?.Inventory?.Items;
        if (items == null || items.Count == 0)
        {
            return (false, false);
        }

        // An equipment-slot item is a direct child of the equipment root whose SlotId is one of
        // SPT's EquipmentSlots names. Stash items and items inside pockets are not.
        var rootId = ReadEquipmentRootId(profile!);
        var hasAnyEquipment = false;
        var hasWeaponInRequiredSlot = false;

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

            if (RequiredWeaponSlotNames.Any(slot => string.Equals(item.SlotId, slot, StringComparison.OrdinalIgnoreCase)))
            {
                hasWeaponInRequiredSlot = true;
            }

            // Every profile carries a pockets container even when nothing is worn, so the container
            // itself is not evidence of equipment.
            if (!string.Equals(item.SlotId, nameof(EquipmentSlots.Pockets), StringComparison.OrdinalIgnoreCase))
            {
                hasAnyEquipment = true;
            }
        }

        return (hasAnyEquipment, hasWeaponInRequiredSlot);
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

    // The minimum a spawned follower needs in order not to die on the client: a pockets container,
    // with the special slots hanging off it. PIT's mercenary-only tweaks (secure-container supplies,
    // the scabbard knife, magazine refills) are NOT applied - they rewrite real player gear.
    private void EnsurePockets(BotBase spawn)
    {
        spawn.Inventory ??= new BotBaseInventory { Items = [] };
        spawn.Inventory.Items ??= [];
        if (spawn.Inventory.Items.Count == 0)
        {
            return;
        }

        var rootId = ReadEquipmentRootId(spawn);
        if (string.IsNullOrWhiteSpace(rootId))
        {
            return;
        }

        var pockets = spawn.Inventory.Items.FirstOrDefault(item =>
            string.Equals(item?.SlotId, nameof(EquipmentSlots.Pockets), StringComparison.OrdinalIgnoreCase));
        if (pockets == null)
        {
            pockets = new Item
            {
                Id = new MongoId(),
                Template = new MongoId(FriendlyItemTemplateIds.EquipmentContainer.Pockets),
                ParentId = rootId,
                SlotId = nameof(EquipmentSlots.Pockets),
                Location = null,
                Upd = new Upd
                {
                    StackObjectsCount = 1,
                    SpawnedInSession = false,
                },
            };
            spawn.Inventory.Items.Add(pockets);
            logger.Info($"[STM] 주머니 컨테이너가 없어 '{spawn.Aid}' 에게 넣어 준다.");
        }

        var pocketsId = pockets.Id.ToString();
        foreach (var specialSlot in spawn.Inventory.Items.Where(item =>
                     item?.SlotId != null &&
                     item.SlotId.Contains("SpecialSlot", StringComparison.OrdinalIgnoreCase)))
        {
            specialSlot.ParentId = pocketsId;
        }
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
