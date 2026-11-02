using System;
using System.Collections.Generic;
using pitTeam.Server.Models;
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
    StmTransferJournal journal,
    StmProfileMemberStore store,
    StmProfileCatalog catalog,
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
            // 읽기 실패는 해석기(Lookup)가 한 번만 읽고 한 번만 분류해 알려 준다. 읽다 실패한 것은 "팀원이 아님" 이 아니다.
            // 호출자가 PIT 용병 먼저/거절 중 어느 길을 탈지 고르도록 별도 신호(ReadFailedReason)로 돌린다.
            var outcome = resolver.Lookup(sessionId, accountId, out var member, out var resolveReason);
            if (outcome == ProfileLookupOutcome.ReadFailed)
            {
                unavailableReason = ProfileViewPolicy.ReadFailedReason;
                return false;
            }

            if (outcome != ProfileLookupOutcome.Resolved || member == null)
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

    // 상세 화면이 /profile/options 로 묻는 것의 프로필 팀원 판. 세 값 계약은 TryBuildView 와 같다:
    //   true                       = options 가 채워졌다. PIT 의 옵션 조회는 부르지 않는다.
    //   false + reason == null     = 프로필 팀원이 아니다. 호출자는 PIT 경로를 그대로 탄다.
    //   false + reason != null     = 등록된 프로필 팀원인데 쓸 수 없다. reason 은 로스터 와이어의 상태 이름과 같은 어휘
    //                                (CurrentSession·Unreadable·Empty…)이라 화면이 그대로 설명으로 바꾼다.
    // 던지지 않는다(PIT 콜백 안에서 불린다). 식별된 뒤의 실패는 "Unreadable" 이지 "팀원이 아님" 이 아니다.
    //
    // PIT 용병 필드(보험·공격성·숙련·소유 의상)는 프로필 팀원에게 뜻이 없어 비워 둔다 - 클라이언트가 Loadouts 가 비면
    // 화면 구성을 포기하므로 장비 항목 하나는 채운다. 이 응답이 하는 새 일은 Stm 에 실리는 것이다.
    public bool TryBuildOptions(MongoId sessionId, string? accountId, out FriendlyTeammateProfileOptionsResponse? options, out string? unavailableReason)
    {
        options = null;
        unavailableReason = null;
        var identifiedAsMember = false;
        try
        {
            var outcome = resolver.Lookup(sessionId, accountId, out var member, out var resolveReason);
            if (outcome == ProfileLookupOutcome.ReadFailed)
            {
                unavailableReason = ProfileViewPolicy.ReadFailedReason;
                return false;
            }

            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                unavailableReason = resolveReason;
                if (!string.IsNullOrWhiteSpace(resolveReason))
                {
                    logger.Warning($"[STM] 프로필 옵션: 등록된 프로필 팀원 '{accountId}'(세션 '{sessionId}')을 쓸 수 없다: {resolveReason}");
                }

                return false;
            }

            identifiedAsMember = true;
            var tactic = store.List(sessionId).FirstOrDefault(m => m.ProfileId == member.ProfileId)?.Tactic;
            if (string.IsNullOrWhiteSpace(tactic)) tactic = "Rifleman";
            options = new FriendlyTeammateProfileOptionsResponse
            {
                CurrentLoadoutId = FriendlyTeammateService.DefaultLoadoutId,
                CurrentTactic = tactic!,
                Loadouts = [new FriendlyTeammateLoadoutOption { Id = FriendlyTeammateService.DefaultLoadoutId, Name = FriendlyTeammateService.DefaultLoadoutName }],
                Tactics = [new FriendlyTeammateTacticOption { Id = tactic!, Name = tactic! }],
                Stm = BuildDetail(sessionId, member),
            };
            return true;
        }
        catch (Exception ex)
        {
            options = null;
            unavailableReason = identifiedAsMember ? "Unreadable" : null;
            logger.Error($"[STM] 프로필 옵션: '{accountId}'(세션 '{sessionId}') 처리 실패 ({(identifiedAsMember ? "프로필 팀원, 읽을 수 없다고 보고" : "기본 경로로 넘긴다")}): {ex}");
            return false;
        }
    }

    // 이 팀원의 편집이 전송 저널에 막혀 있으면 그 사실과 닫는 길을 싣는다. 막힘 여부는 HasOpenEntry 와 같은 판정
    // (TransferJournalPolicy.BlocksEditing)이고, 저널을 못 읽으면 막힌 것으로 친다.
    private StmProfileDetail BuildDetail(MongoId sessionId, StmResolvedMember member)
    {
        var detail = new StmProfileDetail { ProfileId = member.ProfileId };
        var blocking = journal.GetBlockingEntries(sessionId, member.ProfileId);
        if (!blocking.JournalReadable)
        {
            detail.Blocked = true;
            detail.BlockedNotice = TransferEntryClosePolicy.JournalUnreadableNotice;
            detail.CanDiscardJournal = true;
            detail.DiscardWarning = TransferEntryClosePolicy.DiscardUnreadableJournalWarning;
            return detail;
        }

        if (blocking.Entries.Count == 0) return detail;

        // 한 번에 하나씩 보여 준다. 여럿이면 가장 먼저 닫을 수 있는 것부터(없으면 첫 항목) - 하나를 닫으면 다음이 나온다.
        var shown = blocking.Entries.FirstOrDefault(e => e.Verdict == TransferCloseVerdict.MayClose) ?? blocking.Entries[0];
        var entry = shown.Entry;
        var itemCount = entry.ItemIds?.Count ?? 0;
        detail.Blocked = true;
        detail.BlockedNotice = TransferEntryClosePolicy.BlockedNotice(
            DisplayName(sessionId, entry.SourceProfileId), DisplayName(sessionId, entry.DestinationProfileId), itemCount, shown.Verdict);
        if (shown.Verdict == TransferCloseVerdict.MayClose)
        {
            detail.CanClose = true;
            detail.JournalId = entry.JournalId;
            detail.CloseWarning = TransferEntryClosePolicy.CloseWarning(
                DisplayName(sessionId, entry.SourceProfileId), DisplayName(sessionId, entry.DestinationProfileId), itemCount);
        }

        return detail;
    }

    // 플레이어가 아는 이름. 못 찾거나 읽다 실패하면 null -> 정책이 "알 수 없는 프로필" 로 적는다(Mongo id 를 보여 주지 않는다).
    private string? DisplayName(MongoId sessionId, string profileId)
    {
        try
        {
            return catalog.GetCandidates(sessionId.ToString())
                .FirstOrDefault(c => string.Equals(c.ProfileId?.Trim(), profileId?.Trim(), StringComparison.OrdinalIgnoreCase))?.Nickname;
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 프로필 '{profileId}' 의 표시 이름을 읽지 못했다: {ex.Message}");
            return null;
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
