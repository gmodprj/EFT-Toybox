using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// The one place in STM that WRITES to a launcher profile the player actually plays, and the write
// DELETES equipment. There is no undo. Everything here is built so that a doubtful state ends in no
// deletion:
//   - the decision is RaidOutcomePolicy.Decide's, which only ever strips on an explicit Real match;
//   - the raid receipt is RESERVED before anything is deleted, so a second report can never strip twice;
//   - a write-back that cannot be verified is reported at Error level and NOT retried (see Verify below);
//   - any exception is logged and swallowed, so PIT's own mercenary outcome path keeps running.
// What is deleted is the worn equipment only (SelectWornGearRoots). The secure container, the stash,
// and everything the profile is - level, experience, skills, quests - are never touched.
// We never read what the player brought home. STM only subtracts; reconciling a haul is how one rifle
// becomes two (brief, 설계 §6-6 6번).
[Injectable(InjectionType.Singleton)]
public class StmRaidOutcomeService(
    StmProfileMemberResolver resolver,
    StmSpawnProfileFactory spawnProfileFactory,
    StmRaidReceiptStore receipts,
    // SPT declares InventoryHelper [Injectable(InjectionType.Transient)], so this singleton captures one
    // instance for the server's lifetime. Legal in this container (only Scoped -> Singleton is not), and
    // safe here: the only member used is RemoveItem, which touches nothing but its arguments and the
    // helper's own injected logger/localisation.
    InventoryHelper inventoryHelper,
    SaveServer saveServer,
    ISptLogger<StmRaidOutcomeService> logger)
{
    private const string StripAction = "strip-worn-gear";

    // SPT's own slot-name vocabulary (SPTarkov.Server.Core.Models.Enums.EquipmentSlots). Only a direct
    // child of the equipment root carrying one of these names is worn equipment; stash contents and
    // items inside a container are not.
    private static readonly HashSet<string> EquipmentSlotNames =
        new(Enum.GetNames<EquipmentSlots>(), StringComparer.OrdinalIgnoreCase);

    // escaped is the caller's judgment and must mean "survived or extracted". The caller does not call
    // this at all when it cannot tell (FriendlyPostRaidCallbacks.ApplyProfileTeammateOutcomes): a wrong
    // "did not escape" deletes equipment, so uncertainty must never arrive here as false.
    // raidId / accountId may be null or blank; a blank key is refused by TryReserve and nothing is deleted.
    public void Apply(MongoId sessionId, string? raidId, string? accountId, bool escaped)
    {
        try
        {
            // 1. Resolve. Not a profile teammate -> nothing to do here and PIT's path stands.
            if (!resolver.TryResolve(sessionId, accountId, out var member, out var unavailableReason) || member == null)
            {
                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    logger.Warning($"[STM] 레이드 결과: 프로필 팀원 '{accountId}'(세션 '{sessionId}')을 읽을 수 없어({unavailableReason}) 아무것도 지우지 않는다.");
                }

                return;
            }

            // 2. Reserve first, then act. True means "a new receipt was durably written"; every other
            //    outcome - already applied, blank key, unreadable document, failed write - is false and
            //    nothing may be deleted. Reserving before the deletion is deliberate: a failed verify
            //    must NOT be retried later, because the player may have equipped new gear in the
            //    meantime and a retry would delete that instead (task-8 ruling 2).
            var reserved = receipts.TryReserve(sessionId, raidId, member.ProfileId, StripAction);

            // 3. Decide. The policy is the only thing that may say "strip".
            var gearKind = spawnProfileFactory.LastGearKind(sessionId, member.ProfileId);
            var plan = RaidOutcomePolicy.Decide(escaped, gearKind, alreadyApplied: !reserved);
            if (plan.Action != OutcomeAction.StripWornGear)
            {
                logger.Info($"[STM] 레이드 결과: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason}");
                return;
            }

            StripWornGear(member, raidId, plan.Reason);
        }
        catch (Exception ex)
        {
            // Never throw into PIT's outcome handler. A failure here costs one teammate's gear surviving
            // a raid it should not have; stopping PIT's own persistence would cost much more.
            logger.Error($"[STM] 레이드 결과: 프로필 팀원 '{accountId}'(세션 '{sessionId}', 레이드 '{raidId}') 처리에 실패했다. 영수증은 남아 있으므로 다시 시도하지 않는다: {ex}");
        }
    }

    private void StripWornGear(StmResolvedMember member, string? raidId, string reason)
    {
        var pmc = member.Pmc;
        var items = pmc.Inventory?.Items;
        var equipmentRootId = pmc.Inventory?.Equipment?.ToString();
        if (items == null || items.Count == 0)
        {
            logger.Error($"[STM] 레이드 결과: 프로필 '{member.ProfileId}'의 인벤토리를 읽을 수 없어 아무것도 지우지 않는다.");
            return;
        }

        // No guessing at the equipment root. StmSpawnProfileFactory may fall back to the first item when
        // it only READS slots; a deletion may not - a wrong root would delete the wrong subtree.
        if (string.IsNullOrWhiteSpace(equipmentRootId))
        {
            logger.Error($"[STM] 레이드 결과: 프로필 '{member.ProfileId}'의 장비 루트(Inventory.Equipment)가 비어 있어 착용 장비를 가려낼 수 없다. 아무것도 지우지 않는다.");
            return;
        }

        var rootIds = SelectWornGearRoots(items, equipmentRootId);
        if (rootIds.Count == 0)
        {
            logger.Info($"[STM] 레이드 결과: '{member.Nickname}'(프로필 '{member.ProfileId}')에게 지울 착용 장비가 없다.");
            return;
        }

        // The whole set, computed BEFORE the deletion, so the verify below knows what it is looking for.
        var expectedGoneIds = CollectSubtreeIds(items, rootIds);

        // SPT's own deletion primitive, not a hand-written tree walk: InventoryHelper.RemoveItem takes
        // the item and its children out of profile.Inventory.Items and drops the matching InsuredItems
        // entries. output is left null on purpose - a non-null output indexes
        // output.ProfileChanges[sessionId], which is for the live client's item event, not for a write
        // to another launcher profile. With output null the sessionId argument is unused, so the
        // profile's own id is passed rather than the player's session.
        var profileId = new MongoId(member.ProfileId);
        foreach (var rootId in rootIds)
        {
            inventoryHelper.RemoveItem(pmc, rootId, profileId);
        }

        // The only write: the object SaveServer holds, then its own save. No JSON is written here.
        saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
        logger.Info(
            $"[STM] 레이드 결과: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}')의 착용 장비 {rootIds.Count}개(하위 포함 {expectedGoneIds.Count}개)를 지우고 저장했다 - {reason}");

        Verify(member, raidId, expectedGoneIds);
    }

    // Reads the profile back and reports what it found. It does NOT gate a retry: the receipt already
    // stands, and re-running the deletion later could delete gear the player equipped in the meantime.
    // A failure here is loud and left for a human (task-8 ruling 2).
    private void Verify(StmResolvedMember member, string? raidId, HashSet<string> expectedGoneIds)
    {
        var reread = saveServer.GetProfile(new MongoId(member.ProfileId))?.CharacterData?.PmcData;
        if (reread == null)
        {
            logger.Error(
                $"[STM] 레이드 결과 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}')을 다시 읽을 수 없어, 착용 장비 {expectedGoneIds.Count}개가 실제로 지워졌는지 확인하지 못했다. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
            return;
        }

        var stillPresent = (reread.Inventory?.Items ?? [])
            .Where(item => item?.Id != null && expectedGoneIds.Contains(item.Id.ToString()))
            .Select(item => item.Id.ToString())
            .ToList();
        if (stillPresent.Count > 0)
        {
            logger.Error(
                $"[STM] 레이드 결과 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}') - 사라져야 할 아이템 {expectedGoneIds.Count}개 가운데 {stillPresent.Count}개가 그대로 있다: {string.Join(", ", stillPresent.Take(10))}. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
            return;
        }

        logger.Info(
            $"[STM] 레이드 결과 확인: 프로필 '{member.ProfileId}'(레이드 '{raidId}')에서 아이템 {expectedGoneIds.Count}개가 모두 사라진 것을 다시 읽어 확인했다.");
    }

    // Which worn items a lost raid costs. A judgment about slot names and parent ids, kept pure so it
    // can be read on its own:
    //   * only DIRECT children of the equipment root whose SlotId is one of SPT's EquipmentSlots names -
    //     stash items and container contents have other parents, so the stash is never reached;
    //   * minus the slots a dead PMC keeps. SecuredContainer is the brief's explicit exception
    //     (contents preserved, 설계 §6-4); Pockets, Scabbard and ArmBand follow PIT's own model of the
    //     Tarkov rule (FriendlyTeammateService.EquipmentRetention.cs:70-80,
    //     IsPermanentTeammateEquipmentSlot) - and deleting the Pockets container itself would leave a
    //     played profile without one, which is a structural change, not a loss of gear;
    //   * plus the cargo inside those kept Pockets, which Tarkov does lose and which the brief counts as
    //     a descendant of a worn slot - except Dogtag/SpecialSlot trees, which PIT treats as permanent.
    // Dogtag and SpecialSlot* are not EquipmentSlots names, so they are already outside the first rule.
    private static List<MongoId> SelectWornGearRoots(List<Item> items, string equipmentRootId)
    {
        // Item ids are compared Ordinal, exactly as SPT's own ItemExtensions.GetItemWithChildren does
        // (it compares item.ParentId to baseItemId.ToString() with string equality), so the set this
        // picks cannot be wider than the set InventoryHelper.RemoveItem actually deletes.
        var roots = new List<MongoId>();
        var keptPocketIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            if (item?.SlotId == null || item.ParentId == null) continue;
            if (!string.Equals(item.ParentId, equipmentRootId, StringComparison.Ordinal)) continue;
            if (!EquipmentSlotNames.Contains(item.SlotId)) continue;

            if (IsSlotKeptOnDeath(item.SlotId))
            {
                if (string.Equals(item.SlotId, nameof(EquipmentSlots.Pockets), StringComparison.OrdinalIgnoreCase))
                {
                    keptPocketIds.Add(item.Id.ToString());
                }

                continue;
            }

            roots.Add(item.Id);
        }

        if (keptPocketIds.Count == 0)
        {
            return roots;
        }

        foreach (var item in items)
        {
            if (item?.ParentId == null || !keptPocketIds.Contains(item.ParentId)) continue;
            if (item.SlotId != null && IsPermanentSlotName(item.SlotId)) continue;
            roots.Add(item.Id);
        }

        return roots;
    }

    private static bool IsSlotKeptOnDeath(string slotId) =>
        IsPermanentSlotName(slotId)
        || string.Equals(slotId, nameof(EquipmentSlots.SecuredContainer), StringComparison.OrdinalIgnoreCase)
        || string.Equals(slotId, nameof(EquipmentSlots.Pockets), StringComparison.OrdinalIgnoreCase)
        || string.Equals(slotId, nameof(EquipmentSlots.Scabbard), StringComparison.OrdinalIgnoreCase)
        || string.Equals(slotId, nameof(EquipmentSlots.ArmBand), StringComparison.OrdinalIgnoreCase);

    private static bool IsPermanentSlotName(string slotId) =>
        slotId.Contains("Dogtag", StringComparison.OrdinalIgnoreCase)
        || slotId.Contains("SpecialSlot", StringComparison.OrdinalIgnoreCase);

    // Every id that the deletion should take with it, so the verify can look for all of them. Reads the
    // parent links only; it changes nothing.
    private static HashSet<string> CollectSubtreeIds(List<Item> items, List<MongoId> rootIds)
    {
        var all = new HashSet<string>(rootIds.Select(id => id.ToString()), StringComparer.Ordinal);
        var grew = true;
        while (grew)
        {
            grew = false;
            foreach (var item in items)
            {
                if (item?.Id == null || string.IsNullOrWhiteSpace(item.ParentId)) continue;
                if (!all.Contains(item.ParentId)) continue;
                if (all.Add(item.Id.ToString())) grew = true;
            }
        }

        return all;
    }
}
