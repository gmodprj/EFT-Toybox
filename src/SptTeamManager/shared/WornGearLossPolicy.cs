using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // One inventory item reduced to the three fields this judgment needs. The caller builds these from
    // SPT's Item; nothing here knows that type exists.
    public sealed record WornGearItem(string? Id, string? ParentId, string? SlotId);

    // Decides WHICH items a lost raid costs a launcher-profile teammate. This is the highest-stakes
    // judgment in the plan: everything it names is permanently deleted from a profile the player
    // actually plays, and there is no undo. It therefore names only what it recognizes - an
    // unrecognized slot name is never selected - and it never guesses the equipment root.
    //
    // Pure string work over primitives, which is why it lives here and not in the server project: the
    // deletion mechanism (SPT's InventoryHelper.RemoveItem) stays there, the decision is testable here.
    // Deliberately has NO catch-all: the guards below cover every throw the inputs can reach, and a
    // catch would make them unobservable - delete a guard and the test pinning it would still pass,
    // because the catch would swallow the throw and return an empty selection.
    public static class WornGearLossPolicy
    {
        // The slots a dead PMC loses. An ALLOW list, not "every known slot minus the kept ones": a slot
        // name this policy does not know - a typo, a future EquipmentSlots member, anything - is then
        // simply not deleted, which is the closed side.
        // These are SPT's EquipmentSlots names (see SptTeamManager/server/refs/4.1.0, enum EquipmentSlots:
        // Headwear, Earpiece, FaceCover, ArmorVest, Eyewear, ArmBand, TacticalVest, Pockets, Backpack,
        // SecuredContainer, FirstPrimaryWeapon, SecondPrimaryWeapon, Holster, Scabbard). The duplication
        // of the vocabulary is deliberate, exactly as in SpawnGearPolicy.KnownSlots - referencing the
        // enum would drag this judgment back into the server project, where it could not be unit-tested -
        // but the two must agree, so change both or neither.
        private static readonly HashSet<string> LostOnDeathSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Headwear", "Earpiece", "FaceCover", "ArmorVest", "Eyewear", "TacticalVest", "Backpack",
            "FirstPrimaryWeapon", "SecondPrimaryWeapon", "Holster",
        };

        // Reachable so the suite can enforce the cross-policy contract instead of leaving it to a
        // comment: every slot here must be one SpawnGearPolicy recognizes, and none may be a slot every
        // profile carries anyway (WornGearLossPolicyTests). Read-only on purpose - nothing writes it.
        internal static IReadOnlySet<string> LostOnDeathSlotNames => LostOnDeathSlots;

        // Absent from the list above, each for its own reason:
        //   SecuredContainer - the brief's explicit exception, contents included (설계 §6-4, 타르코프 규칙);
        //   Pockets          - the cargo inside is lost (below), but the container item itself stays:
        //                      removing it from a played profile is structural damage, not a loss of gear
        //                      (PIT keeps a follower's pockets for the same reason, EnsureFollowerHasPockets);
        //   Scabbard, ArmBand - PIT's own model of what a death does not take
        //                      (FriendlyTeammateService.EquipmentRetention.cs, IsPermanentTeammateEquipmentSlot).
        private const string PocketsSlot = "Pockets";

        // Not EquipmentSlots values at all, so they are already outside the allow list. Named anyway
        // because pocket cargo is matched by parent id rather than by slot name, and PIT re-parents
        // special slots under the pockets container; a dogtag or special slot must survive either way.
        private static readonly string[] PermanentSlotFragments = { "Dogtag", "SpecialSlot" };

        // The items to delete, as ids. Each is a tree root: the caller's deletion takes the descendants
        // with it, and CollectSubtreeIds below names them for the write-back check.
        //
        // Two rules, and nothing else is ever selected:
        //   1. a DIRECT child of the equipment root whose slot is in LostOnDeathSlots. Stash items and
        //      container contents have other parents, so the stash can never be reached from here;
        //   2. the cargo directly inside a kept Pockets container, minus dogtag/special-slot trees.
        //
        // equipmentRootId must be the profile's real equipment root. Blank selects nothing: a guessed
        // root would delete a different subtree, and for a deletion "no idea" must mean "nothing".
        // A SET, so a corrupt profile carrying the same id twice cannot yield the same root twice - which
        // would over-count the log line and hand the same id to the deletion a second time.
        public static HashSet<string> SelectLostRootIds(IEnumerable<WornGearItem>? items, string? equipmentRootId)
        {
            var selected = new HashSet<string>(StringComparer.Ordinal);
            if (items == null || string.IsNullOrWhiteSpace(equipmentRootId))
            {
                return selected;
            }

            // About PARENT IDS only, and nothing else: the caller's root argument is trimmed once here,
            // while an item's own ParentId is never trimmed and is compared Ordinal (so is the pocket
            // parent match below). Slot NAMES are a separate matter - those are trimmed and compared
            // case-insensitively, as SpawnGearPolicy does. The asymmetry is deliberate and it is the
            // item's side that matters: a looser parent-id comparison would pull MORE items into a
            // deletion. (Unlike RaidReceiptKeyPolicy, where treating a near miss as the same key was
            // the safe direction.)
            var rootId = equipmentRootId!.Trim();
            var keptPocketIds = new HashSet<string>(StringComparer.Ordinal);
            var cargoCandidates = new List<WornGearItem>();

            foreach (var item in items)
            {
                if (item == null) continue;
                // An item with no id cannot be named to a deletion, so it is not selected.
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                if (string.IsNullOrWhiteSpace(item.SlotId)) continue;

                var slot = item.SlotId!.Trim();

                if (!string.Equals(item.ParentId, rootId, StringComparison.Ordinal))
                {
                    // Not worn equipment. Remember it only as possible pocket cargo (rule 2).
                    cargoCandidates.Add(item);
                    continue;
                }

                if (string.Equals(slot, PocketsSlot, StringComparison.OrdinalIgnoreCase))
                {
                    keptPocketIds.Add(item.Id!);
                    continue;
                }

                if (!LostOnDeathSlots.Contains(slot)) continue;

                selected.Add(item.Id!);
            }

            if (keptPocketIds.Count == 0)
            {
                return selected;
            }

            foreach (var item in cargoCandidates)
            {
                // Direct children of the kept pockets container only. Something deeper inside a pocket is
                // a descendant of one of these and goes with it.
                if (item.ParentId == null || !keptPocketIds.Contains(item.ParentId)) continue;
                // The trimmed slot name, as in the root-child path above.
                if (IsPermanentSlot(item.SlotId!.Trim())) continue;
                selected.Add(item.Id!);
            }

            return selected;
        }

        // Every id the deletion should take with it, roots included: what the write-back check looks for.
        // Reads parent links only. An id already in the set is not added twice, so a parent cycle in a
        // corrupt profile terminates instead of looping forever.
        public static HashSet<string> CollectSubtreeIds(IEnumerable<WornGearItem>? items, IEnumerable<string>? rootIds)
        {
            var all = new HashSet<string>(StringComparer.Ordinal);
            if (rootIds != null)
            {
                foreach (var rootId in rootIds)
                {
                    if (string.IsNullOrWhiteSpace(rootId)) continue;
                    all.Add(rootId);
                }
            }

            if (items == null || all.Count == 0)
            {
                return all;
            }

            var itemList = new List<WornGearItem>();
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.ParentId)) continue;
                itemList.Add(item);
            }

            var grew = true;
            while (grew)
            {
                grew = false;
                foreach (var item in itemList)
                {
                    if (!all.Contains(item.ParentId!)) continue;
                    if (all.Add(item.Id!)) grew = true;
                }
            }

            return all;
        }

        private static bool IsPermanentSlot(string? slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) return false;

            foreach (var fragment in PermanentSlotFragments)
            {
                if (slotId!.Contains(fragment, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
    }
}
