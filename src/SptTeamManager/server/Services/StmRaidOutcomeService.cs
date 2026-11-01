using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// The one place in STM that WRITES to a launcher profile the player actually plays, and the write
// DELETES equipment. There is no undo. Everything here is built so that a doubtful state ends in no
// deletion:
//   - the decision is RaidOutcomePolicy.Decide's, which only ever strips on an explicit Real match;
//   - the raid receipt is RESERVED before anything is deleted, so a second report can never strip twice;
//   - a profile SPT refuses to write (flagged invalid or unloadable) is turned away BEFORE the receipt is
//     spent, so an in-memory-only strip can never be reported as done;
//   - a deletion that cannot be confirmed in memory is reported at Error level and NOT retried (see
//     VerifyDeletionApplied below, whose reach is stated there); a save that fails throws, and the
//     catch-all logs it;
//   - any exception is logged and swallowed, so PIT's own mercenary outcome path keeps running.
// What is deleted is the worn equipment only, and WHICH items that is belongs to
// WornGearLossPolicy (shared/WornGearLossPolicy.cs - pure and tested); this class owns only the
// mechanism. The secure container, the stash, and everything the profile is - level, experience,
// skills, quests - are never touched.
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
    // Only for releasing the protection on gear this service has just deleted - see StripWornGear.
    FriendlyPostRaidService postRaidService,
    ISptLogger<StmRaidOutcomeService> logger)
{
    private const string StripAction = "strip-worn-gear";

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

            // The value the whole deletion turns on, logged where someone can find it afterwards.
            // There is nothing to cross-check it against: no field on the report corroborates a loss.
            // Every candidate is either defaulted by a truncated payload to the same value a genuine loss
            // carries (HealthRatio 0 - client/Modules/NpcMessage.cs:236,
            // client/Modules/FollowerDeathEscapeResolver.GearSnapshot.cs:187; VitalsDestroyed false) or
            // missing from a legitimate loss path (EquipmentItems is null on the live-roll path,
            // client/Modules/FollowerDeathEscapeResolver.cs:224 and :315). So requiring corroboration
            // would refuse real losses without catching a single truncated payload, and this line is the
            // trace a wrong deletion is read back from instead.
            logger.Info($"[STM] 레이드 결과 접수: '{member.Nickname}'(aid {accountId}, 프로필 '{member.ProfileId}', 레이드 '{raidId}') escaped={escaped}.");

            // 2. Refuse a profile SPT will not write, BEFORE the receipt is spent.
            //    SaveProfileAsync opens with `if (IsProfileInvalidOrUnloadable(sessionID)) return 0L;`
            //    (IL_0024-IL_002d of SaveServer/<SaveProfileAsync>d__22.MoveNext, refs/4.1.0): it writes
            //    nothing, throws nothing, and returns a duration of zero. Stripping such a profile would
            //    delete the gear in memory, log a success, satisfy the in-memory re-check, leave the file
            //    untouched - and spend the receipt, so the strip would never happen at all. Checked here
            //    rather than after the deletion so no receipt is spent: once the profile is loadable
            //    again, a later report for the same raid can still apply it.
            //    The resolver does not look at this flag (it reads CharacterData.PmcData), so a flagged
            //    profile does resolve and this guard is the only thing standing in front of it.
            var profileId = new MongoId(member.ProfileId);
            if (saveServer.IsProfileInvalidOrUnloadable(profileId))
            {
                logger.Error($"[STM] 레이드 결과: 프로필 '{member.ProfileId}'(레이드 '{raidId}')이 SPT에서 손상/적재 불가로 표시돼 있어 저장이 되지 않는다. 아무것도 지우지 않고 영수증도 남기지 않는다 - 프로필이 정상으로 돌아오면 다시 적용될 수 있다.");
                return;
            }

            // 3. Reserve first, then act. True means "a new receipt was durably written"; every other
            //    outcome - already applied, blank key, unreadable document, failed write - is false and
            //    nothing may be deleted. Reserving before the deletion is deliberate: a failed verify
            //    must NOT be retried later, because the player may have equipped new gear in the
            //    meantime and a retry would delete that instead (task-8 ruling 2).
            var reserved = receipts.TryReserve(sessionId, raidId, member.ProfileId, StripAction);

            // 4. Decide. The policy is the only thing that may say "strip".
            var gearKind = spawnProfileFactory.LastGearKind(sessionId, member.ProfileId);
            var plan = RaidOutcomePolicy.Decide(escaped, gearKind, alreadyApplied: !reserved);
            if (plan.Action != OutcomeAction.StripWornGear)
            {
                logger.Info($"[STM] 레이드 결과: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}') - {plan.Reason}");
                return;
            }

            StripWornGear(sessionId, member, profileId, raidId, plan.Reason);
        }
        catch (Exception ex)
        {
            // Never throw into PIT's outcome handler. A failure here costs one teammate's gear surviving
            // a raid it should not have; stopping PIT's own persistence would cost much more.
            logger.Error($"[STM] 레이드 결과: 프로필 팀원 '{accountId}'(세션 '{sessionId}', 레이드 '{raidId}') 처리에 실패했다. 영수증은 남아 있으므로 다시 시도하지 않는다: {ex}");
        }
    }

    private void StripWornGear(MongoId sessionId, StmResolvedMember member, MongoId profileId, string? raidId, string reason)
    {
        var pmc = member.Pmc;
        var items = pmc.Inventory?.Items;
        var equipmentRootId = pmc.Inventory?.Equipment?.ToString();
        // These three refusals run AFTER the receipt was reserved, so each one forfeits this raid's
        // strip for good - the receipt is spent and the raid will not be looked at again. Said in the
        // log, because "아무것도 지우지 않는다" on its own reads as harmless and this is final.
        if (items == null || items.Count == 0)
        {
            logger.Error($"[STM] 레이드 결과: 프로필 '{member.ProfileId}'의 인벤토리를 읽을 수 없어 아무것도 지우지 않는다. 영수증은 이미 예약됐으므로 이 레이드는 다시 처리되지 않는다 - 지우려면 사람이 직접 해야 한다.");
            return;
        }

        // No guessing at the equipment root. StmSpawnProfileFactory may fall back to the first item when
        // it only READS slots; a deletion may not - a wrong root would delete the wrong subtree.
        if (string.IsNullOrWhiteSpace(equipmentRootId))
        {
            logger.Error($"[STM] 레이드 결과: 프로필 '{member.ProfileId}'의 장비 루트(Inventory.Equipment)가 비어 있어 착용 장비를 가려낼 수 없다. 아무것도 지우지 않는다. 영수증은 이미 예약됐으므로 이 레이드는 다시 처리되지 않는다 - 지우려면 사람이 직접 해야 한다.");
            return;
        }

        // WHICH items a death costs is WornGearLossPolicy's judgment (pure, tested). This method only
        // translates: SPT Items in, (id, parent, slot) triples out.
        var triples = new List<WornGearItem>(items.Count);
        foreach (var item in items)
        {
            if (item?.Id == null) continue;
            triples.Add(new WornGearItem(item.Id.ToString(), item.ParentId, item.SlotId));
        }

        var rootIds = WornGearLossPolicy.SelectLostRootIds(triples, equipmentRootId);
        if (rootIds.Count == 0)
        {
            logger.Info($"[STM] 레이드 결과: '{member.Nickname}'(프로필 '{member.ProfileId}')에게 지울 착용 장비가 없다. 영수증은 이미 예약됐으므로 이 레이드는 다시 처리되지 않는다.");
            return;
        }

        // The whole set, computed BEFORE the deletion, so the verify below knows what it is looking for.
        var expectedGoneIds = WornGearLossPolicy.CollectSubtreeIds(triples, rootIds);

        // SPT's own deletion primitive, not a hand-written tree walk: InventoryHelper.RemoveItem takes
        // the item and its children out of profile.Inventory.Items and drops the matching InsuredItems
        // entries. output is left null on purpose - a non-null output indexes
        // output.ProfileChanges[sessionId], which is for the live client's item event, not for a write
        // to another launcher profile. With output null the sessionId argument is unused, so the
        // profile's own id is passed rather than the player's session.
        foreach (var rootId in rootIds)
        {
            inventoryHelper.RemoveItem(pmc, new MongoId(rootId), profileId);
        }

        // The only write: the object SaveServer holds, then its own save. No JSON is written here.
        //
        // What the return value is, measured rather than assumed (IL of
        // SaveServer/<SaveProfileAsync>d__22.MoveNext in refs/4.1.0/SPTarkov.Server.Core.dll): the method
        // serializes the profile, hashes it, compares the hash with its own saveMd5 entry, SKIPS
        // FileUtil.WriteFileAsync (IL_0226) when it is unchanged, stops a Stopwatch and returns
        // Stopwatch.ElapsedMilliseconds (IL_02b2). So it is a DURATION, not a byte count.
        // A zero therefore does NOT mean the file was written quickly. It is also what comes back when
        // the write was skipped - the hash was unchanged, or the method returned at its first line
        // because the profile is flagged invalid or unloadable (IL_0024-IL_002d). The second of those is
        // the one that would matter, and it cannot happen here: step 2 above refuses that profile before
        // the receipt is spent. The first cannot happen either, since the deletion just changed the
        // profile. So the duration is logged as a duration and nothing is inferred from it.
        // How a failed WRITE reaches us: WriteFileAsync is awaited, so its exception propagates out of
        // GetAwaiter().GetResult() and lands in Apply's catch-all, which logs it at Error.
        var elapsedMs = saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
        logger.Info(
            $"[STM] 레이드 결과: '{member.Nickname}'(프로필 '{member.ProfileId}', 레이드 '{raidId}')의 착용 장비 {rootIds.Count}개(하위 포함 {expectedGoneIds.Count}개)를 지우고 저장했다({elapsedMs}ms) - {reason}");

        // Hand what we just deleted back to the player, if they carried it out.
        //
        // At raid entry every item of this teammate's spawn inventory was registered as protected, so
        // that surviving gear could not ALSO be delivered to the player - that would copy an item that
        // still exists in the teammate's profile. Once the item is gone from the profile the protection
        // inverts: PIT's RemoveProtectedTeammateItemsFromExtractedProfile strips protected ids from the
        // player's EXTRACTED inventory and saved profile too, so leaving these registered destroys gear
        // the player looted off the corpse and carried home. Measured in game 2026-11-01: 35 items left
        // the teammate's profile and 0 of them reached the player's - all destroyed.
        //
        // This runs here, and only after the save, for two reasons measured from the same raid's log:
        //   - ordering: /raid-outcomes (this call) arrives BEFORE /client/match/local/end, which is where
        //     the player-side stripping happens, so the release lands in time;
        //   - entitlement: the items must already be gone from the profile, or releasing them would let
        //     one rifle exist in two places.
        // Scope is deliberately exactly what was deleted. A surviving teammate strips nothing and so
        // releases nothing - their gear is still theirs. A Temporary-gear teammate also releases nothing
        // here, because nothing was deleted; whether the player should keep a generated loadout looted
        // off a corpse is a separate decision, recorded in doc/WORKLOG.md.
        postRaidService.RemoveProtectedRaidItemIds(
            sessionId,
            expectedGoneIds,
            $"profile teammate death release {member.ProfileId} (raid {raidId})");

        VerifyDeletionApplied(member, profileId, raidId, expectedGoneIds);
    }

    // Re-checks the DELETION, not the save: it asks SaveServer for the profile again and looks for the
    // ids that should be gone. Be clear about the reach - SaveServer hands back the same in-memory
    // object the resolver read, so this catches a RemoveItem that did nothing (an id it could not find,
    // a subtree it did not follow) and CANNOT catch a save that failed - a failed save throws instead
    // (see the save call above). The file itself is never re-parsed here.
    // It does NOT gate a retry either: the receipt already stands, and re-running the deletion later
    // could delete gear the player equipped in the meantime. A failure is loud and left for a human
    // (task-8 ruling 2).
    private void VerifyDeletionApplied(StmResolvedMember member, MongoId profileId, string? raidId, HashSet<string> expectedGoneIds)
    {
        // KEPT AS DEFENSIVE, and in practice unreachable - do not read the message below as something
        // anyone will see. SaveServer.GetProfile never returns null: it THROWS on an empty id, on no
        // profiles being loaded, and on an id it does not hold (three `throw` sites in its IL), and all
        // three land in Apply's catch-all with its generic message instead. What is left is only
        // CharacterData or PmcData having become null since the resolver read them non-null moments ago,
        // which nothing in STM does.
        var reread = saveServer.GetProfile(profileId)?.CharacterData?.PmcData;
        if (reread == null)
        {
            logger.Error(
                $"[STM] 레이드 결과 확인 실패: 프로필 '{member.ProfileId}'(레이드 '{raidId}')의 캐릭터 데이터가 사라져, 착용 장비 {expectedGoneIds.Count}개가 메모리에서 지워졌는지 확인하지 못했다. 영수증이 남아 있으므로 다시 시도하지 않는다 - 프로필을 직접 확인하라.");
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
            $"[STM] 레이드 결과 확인: 프로필 '{member.ProfileId}'(레이드 '{raidId}')에서 아이템 {expectedGoneIds.Count}개가 모두 사라진 것을 메모리에서 다시 확인했다.");
    }
}
