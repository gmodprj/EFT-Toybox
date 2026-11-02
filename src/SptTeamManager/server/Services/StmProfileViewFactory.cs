using System;
using System.Collections.Generic;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils.Cloners;

namespace pitTeam.Server.Services;

// Answers "who is this teammate?" for a launcher-profile teammate: the fourth call site of the same
// hole plan 2 closed in three others (invite, follower-generate, raid outcome). The client asks this
// when it restores last raid's squad (auto-join, RaidStartPatch.BuildGroupPlayer); before this class
// existed PIT's own lookup answered, found no mercenary with that Aid, threw, and the client dropped
// that teammate from the restored squad with only a warning in the log.
//
// member.Pmc is the LIVE object SaveServer holds (see StmProfileMemberResolver). Nothing here writes
// through it: the response is built from cloner.Clone, a deep clone (SPT's ICloner is FastCloner), so
// the trimming below cannot reach the stored profile. Reading the live object to clone it mutates
// nothing; everything after the clone touches the copy only.
[Injectable(InjectionType.Singleton)]
public class StmProfileViewFactory(
    StmProfileMemberResolver resolver,
    // Same capture as StmSpawnProfileFactory's: FriendlyTeammateService is Transient, so this
    // singleton holds one instance for the server's lifetime. Only ToOtherProfileResponse is used and
    // it reads its argument and injected services only - no PIT instance state.
    FriendlyTeammateService teammateService,
    ICloner cloner,
    ISptLogger<StmProfileViewFactory> logger)
{
    // true  = view is set, and PIT's own lookup must NOT run.
    // false + unavailableReason == null  = not a profile teammate (caller continues with PIT's path).
    // false + unavailableReason != null  = a registered teammate we could not render; PIT is not asked
    //                                     either, because PIT has no mercenary under this Aid.
    // The three-value contract and the STM-before-PIT order are the resolver's (invariant 3): asking
    // PIT first would answer with an invisible mercenary whenever a mercenary and a profile share an Aid.
    //
    // Never throws: this sits inside a PIT callback, and a throw here would turn a cosmetic failure
    // into a failed request. The resolver call is inside the try too, so the guarantee does not rest
    // on another class keeping its catch-all (same shape as the plan-2 call sites).
    public bool TryBuildView(MongoId sessionId, string? accountId, out GetOtherProfileResponse? view, out string? unavailableReason)
    {
        view = null;
        unavailableReason = null;
        var identifiedAsMember = false;
        try
        {
            if (!resolver.TryResolve(sessionId, accountId, out var member, out var resolveReason) || member == null)
            {
                unavailableReason = resolveReason;
                if (!string.IsNullOrWhiteSpace(resolveReason))
                {
                    logger.Warning($"[STM] 프로필 조회: 등록된 프로필 팀원 '{accountId}'(세션 '{sessionId}')을 쓸 수 없다: {resolveReason}");
                }

                return false;
            }

            identifiedAsMember = true;
            var clone = cloner.Clone(member.Pmc)
                ?? throw new InvalidOperationException("cloner.Clone returned null");
            TrimToWornEquipment(clone, member);
            view = teammateService.ToOtherProfileResponse(clone);
            return true;
        }
        catch (Exception ex)
        {
            view = null;
            // Identified and then failed is "a teammate we could not read", never "not a teammate":
            // handing the Aid to PIT would make it throw its own not-found, which is the bug this
            // class exists to fix. Only a failure BEFORE identification falls through to PIT.
            unavailableReason = identifiedAsMember ? "Unreadable" : null;
            logger.Error($"[STM] 프로필 조회: '{accountId}'(세션 '{sessionId}') 처리 실패 ({(identifiedAsMember ? "프로필 팀원, 읽을 수 없다고 보고" : "기본 경로로 넘긴다")}): {ex}");
            return false;
        }
    }

    // A profile teammate's Inventory.Items is the whole launcher profile - stash included, which for a
    // played character is thousands of items. PIT's mapper hands Inventory.Items straight to
    // Equipment.Items because a mercenary owns nothing but its gear; for a real profile that would both
    // ship a stash over an inspect screen and misstate what the teammate is wearing. So the clone keeps
    // the equipment tree and drops the rest.
    //
    // WHICH ids belong to that tree is WornGearLossPolicy.CollectSubtreeIds' judgment (pure, tested,
    // and already the authority on this profile's equipment tree in StmRaidOutcomeService). This method
    // only translates: SPT Items in, (id, parent, slot) triples out.
    private void TrimToWornEquipment(SPTarkov.Server.Core.Models.Eft.Common.Tables.BotBase clone, StmResolvedMember member)
    {
        var items = clone.Inventory?.Items;
        if (items == null || items.Count == 0) return;

        var equipmentRootId = clone.Inventory?.Equipment?.ToString();
        // No guessing at the equipment root - the same refusal StmRaidOutcomeService makes, for the
        // milder reason: a wrong root would show gear the teammate is not wearing, and the whole stash
        // is worse than an empty inspect screen. Closed side is "show no items".
        if (string.IsNullOrWhiteSpace(equipmentRootId))
        {
            logger.Warning($"[STM] 프로필 조회: 프로필 '{member.ProfileId}'의 장비 루트(Inventory.Equipment)가 비어 있어 착용 장비를 가려낼 수 없다. 아이템 없이 신원만 보낸다.");
            items.Clear();
            return;
        }

        var triples = new List<WornGearItem>(items.Count);
        foreach (var item in items)
        {
            if (item?.Id == null) continue;
            triples.Add(new WornGearItem(item.Id.ToString(), item.ParentId, item.SlotId));
        }

        var visibleIds = WornGearLossPolicy.CollectSubtreeIds(triples, new[] { equipmentRootId! });
        items.RemoveAll(item => item?.Id == null || !visibleIds.Contains(item.Id.ToString()));
    }
}
