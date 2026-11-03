using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Path = System.IO.Path;

namespace pitTeam.Server.Services;

// 계획 5 Task B. 편집기가 끝난 뒤의 **그 팀원 인벤토리 전체**를 받아 한 번에 쓴다.
//
// 왜 "이동 목록"(StmGearEditService)이 아니라 이것인가: 사용자 요구가 "플레이어 화면에서 되는 동작은
// 하나도 빠짐없이" 이고, 그 화면에는 아이템 id 가 **생기고 사라지는** 조작(묶음 나누기·합치기·탄박스
// 열기·버리기)이 있다. "이 아이템을 이 부모의 이 슬롯으로" 의 목록으로는 그것을 적을 수 없다.
// 그래서 조작의 종류를 세지 않고 **최종 상태**를 받아 정당성만 본다(ProfileInventoryApplyPolicy).
//
// 지키는 것은 이동 목록 경로와 같다: 출처는 그 팀원의 것뿐, **쓰기 한 번**, 디스크 재독 대조, 실패하면
// 되돌리기, 구조를 이루는 자리 보호. 바뀐 것은 "무엇을 적을 수 있는가" 뿐이다.
[Injectable(InjectionType.Singleton)]
public class StmInventoryApplyService(
    StmProfileMemberResolver resolver,
    StmProfileMemberStore store,
    StmTransferJournal journal,
    SaveServer saveServer,
    ICloner cloner,
    StmGearSlotFactsBuilder slotFactsBuilder,
    JsonUtil jsonUtil,
    ISptLogger<StmInventoryApplyService> logger)
{
    private const string JournalBlockedReason =
        "이 팀원에게 끝나지 않은 전송이 있어 장비를 고치지 않습니다. 상세 화면에서 그 기록을 먼저 닫으세요.";

    private static string ProfileDirectory => Path.Combine(AppContext.BaseDirectory, "user", "profiles");

    public bool TryApply(
        MongoId sessionId,
        string? accountId,
        string? requestedProfileId,
        List<Item>? submitted,
        IReadOnlyCollection<string>? loadedItemIds,
        out string? rejectionReason)
    {
        rejectionReason = null;
        BotBaseInventory? liveInventory = null;
        List<Item>? originalItems = null;
        MongoId? liveProfileId = null;

        try
        {
            if (submitted == null || submitted.Count == 0)
            {
                rejectionReason = "저장할 인벤토리를 읽지 못해 저장하지 않습니다.";
                return false;
            }

            // 레이드 중에는 고치지 않는다(이동 목록 경로와 같은 이유 - 죽으면 방금 넣은 것이 지워진다).
            if (FriendlyModeRequestGate.HasActiveRaid)
            {
                rejectionReason = GearEditEntryPolicy.RaidInProgressReason;
                return false;
            }

            if (journal.HasOpenEntry(sessionId, requestedProfileId ?? string.Empty))
            {
                rejectionReason = JournalBlockedReason;
                return false;
            }

            var outcome = resolver.Lookup(sessionId, accountId, out var member, out _);
            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                rejectionReason = "이 팀원은 프로필 팀원이 아니어서 이 경로로 장비를 고칠 수 없습니다.";
                return false;
            }

            if (!string.Equals(member.ProfileId, requestedProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 인벤토리 저장 거부: 요청의 프로필 '{requestedProfileId}' 과(와) Aid '{accountId}' 로 해석된 프로필 '{member.ProfileId}' 이(가) 다르다.");
                rejectionReason = "요청한 프로필과 이 팀원의 프로필이 달라 저장하지 않습니다.";
                return false;
            }

            var profileId = new MongoId(member.ProfileId);
            if (saveServer.IsProfileInvalidOrUnloadable(profileId))
            {
                logger.Error($"[STM] 인벤토리 저장 거부: 프로필 '{member.ProfileId}' 이(가) SPT 에서 손상/적재 불가로 표시돼 있다.");
                rejectionReason = "이 프로필이 손상으로 표시돼 있어 저장할 수 없습니다.";
                return false;
            }

            var live = member.Pmc;
            var inventory = live.Inventory;
            if (inventory?.Items == null)
            {
                rejectionReason = "이 프로필의 인벤토리를 읽을 수 없어 저장하지 않습니다.";
                return false;
            }

            var stashRootId = inventory.Stash?.ToString();
            var equipmentRootId = inventory.Equipment?.ToString();
            if (string.IsNullOrWhiteSpace(stashRootId) || string.IsNullOrWhiteSpace(equipmentRootId))
            {
                rejectionReason = "이 프로필의 스태시·장비 루트를 읽을 수 없어 저장하지 않습니다.";
                return false;
            }

            // 살아 있는 객체는 읽기만 한다. 고치는 것은 복제본이다(불변식 1).
            var original = cloner.Clone(inventory.Items) ?? throw new InvalidOperationException("StmInventoryApplyCloneFailed");
            var originalIndex = new Dictionary<string, Item>(StringComparer.Ordinal);
            foreach (var item in original)
            {
                if (item?.Id != null) originalIndex[item.Id.ToString()] = item;
            }

            // 프로필에는 두 트리 **밖**의 아이템도 있다(퀘스트 보관함·정렬 테이블·하이드아웃 보관함 등 -
            // Buck 의 프로필에서 실측 23개). 편집기는 그것을 보지도 보내지도 않으므로, 제출본과 대조하는
            // before 도 **두 트리만**이어야 하고, 쓸 때는 나머지를 그대로 붙여 준다. 이것을 빼먹으면
            // 제출본에 없는 그 23개가 "사라진 아이템" 이 되어 통째로 지워진다.
            var twoTrees = new List<Item>();
            var untouched = new List<Item>();
            foreach (var item in original)
            {
                if (IsInTree(originalIndex, item, stashRootId!) || IsInTree(originalIndex, item, equipmentRootId!))
                {
                    twoTrees.Add(item);
                }
                else
                {
                    untouched.Add(item);
                }
            }

            var before = ToStates(twoTrees);
            var after = ToStates(submitted);

            // 자리의 어휘(슬롯·탄 자리·격자·필터·용량). **원래 아이템과 제출본을 함께** 넣는다:
            // 부모가 될 수 있는 것은 원래 아이템이고, 나누기로 생긴 더미의 종류는 제출본에만 있다.
            // 읽히지 않으면 빈 사실이 오고, 그때 이 경로는 자리를 보지 않고 보존만 본다
            // (ProfileInventoryApplyPolicy.CheckPlacements 의 극성 주석).
            var slotFacts = slotFactsBuilder.Build(original.Concat(submitted));
            var verdict = ProfileInventoryApplyPolicy.Validate(before, after, stashRootId, equipmentRootId, loadedItemIds, slotFacts);
            if (!verdict.Allowed)
            {
                rejectionReason = verdict.Reason;
                return false;
            }

            // 제출본을 그대로 쓰지 않는다. 원래 있던 아이템은 **원래 객체**를 쓰고 자리(부모·슬롯·좌표)와
            // 수량만 받는다 - 내구도·충전량 같은 다른 Upd 필드를 클라이언트가 보낸 값으로 덮지 않기 위해서다.
            // 새로 생긴 아이템(묶음 나누기)은 **서버가 id 를 만든다**: 클라이언트가 준 id 를 쓰면 다른
            // 프로필의 아이템 id 와 겹치게 보낼 수 있고, 그 순간 복제처럼 보이는 상태가 된다.
            var originalById = originalIndex;

            var applied = new List<Item>(submitted.Count);
            var created = 0;
            foreach (var item in submitted)
            {
                if (item?.Id == null) continue;
                var id = item.Id.ToString();
                if (originalById.TryGetValue(id, out var existing))
                {
                    existing.ParentId = item.ParentId;
                    existing.SlotId = item.SlotId;
                    existing.Location = item.Location;
                    var count = item.Upd?.StackObjectsCount;
                    if (count is > 0)
                    {
                        existing.Upd ??= new Upd();
                        existing.Upd.StackObjectsCount = count;
                    }

                    applied.Add(existing);
                    continue;
                }

                created++;
                applied.Add(new Item
                {
                    Id = new MongoId(),
                    Template = item.Template,
                    ParentId = item.ParentId,
                    SlotId = item.SlotId,
                    Location = item.Location,
                    Upd = new Upd { StackObjectsCount = item.Upd?.StackObjectsCount is > 0 ? item.Upd!.StackObjectsCount : 1 },
                });
            }

            // 새 id 를 서버가 만들었으므로, 그 새 아이템을 부모로 가리키던 제출본의 참조는 깨진다.
            // 지금은 **나누기로 생긴 더미**만 새 아이템이고 그것은 누구의 부모도 아니다. 혹시라도 부모로
            // 쓰였다면 그 아이템은 위 트리 검사를 통과하지 못하므로 여기까지 오지 않는다.

            // 두 트리 밖의 아이템을 그대로 붙인다 - 편집기는 그것을 본 적이 없다.
            var finalItems = new List<Item>(applied.Count + untouched.Count);
            finalItems.AddRange(applied);
            finalItems.AddRange(untouched);

            // 유일한 쓰기.
            liveInventory = inventory;
            liveProfileId = profileId;
            originalItems = original;
            inventory.Items = finalItems;
            var elapsedMs = saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();

            if (!VerifyFromDisk(member.ProfileId, finalItems, stashRootId!, equipmentRootId!, before, loadedItemIds, slotFacts, out var mismatch))
            {
                logger.Error($"[STM] 인벤토리 저장 확인 실패(프로필 '{member.ProfileId}'): {mismatch} 원래 상태로 되돌린다.");
                var restored = TryRestore(profileId, inventory, original);
                rejectionReason = restored
                    ? "저장한 내용을 디스크에서 다시 읽어 확인하지 못해 원래대로 되돌렸습니다. 서버 로그를 확인하세요."
                    : "저장한 내용을 확인하지 못했고 되돌리기도 실패했습니다. 이 프로필의 장비와 스태시를 직접 확인하세요.";
                return false;
            }

            var lost = verdict.LostItemIds.Count == 0 ? "없음" : string.Join(", ", verdict.LostItemIds);
            logger.Info($"[STM] 인벤토리 저장: 프로필 '{member.ProfileId}'(aid {member.Aid}) 두 트리 {twoTrees.Count} → {applied.Count}개(두 트리 밖 {untouched.Count}개는 그대로), 새로 생긴 더미 {created}개, 사라진 아이템: {lost} ({elapsedMs}ms)");

            bool flagged;
            try
            {
                flagged = store.Update(sessionId, member.ProfileId, new StmProfileMemberChange(AutoJoin: null, HandEdited: true));
            }
            catch (Exception ex)
            {
                logger.Error($"[STM] 인벤토리 저장: 프로필 '{member.ProfileId}' 은(는) 저장됐지만 '손으로 고침' 표시를 쓰다 실패했다: {ex}");
                flagged = false;
            }

            if (!flagged)
            {
                logger.Error($"[STM] 인벤토리 저장: 프로필 '{member.ProfileId}' 은(는) 저장됐지만 '손으로 고침' 표시를 쓰지 못했다.");
                rejectionReason = "장비는 저장됐지만 '손으로 고침' 표시를 저장하지 못했습니다. 다시 한 번 저장해 주세요.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 인벤토리 저장이 실패했다(프로필 '{requestedProfileId}'): {ex}");
            if (liveInventory != null && originalItems != null && liveProfileId != null)
            {
                var restored = TryRestore(liveProfileId.Value, liveInventory, originalItems);
                rejectionReason = restored
                    ? "저장 중 오류가 나서 원래대로 되돌렸습니다. 서버 로그를 확인하세요."
                    : "저장 중 오류가 났고 되돌리기도 실패했습니다. 이 프로필의 장비와 스태시를 직접 확인하세요.";
                return false;
            }

            rejectionReason = "저장 중 오류가 났습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    // 디스크에서 다시 읽어 **우리가 쓴 그대로인지**와 **보존이 깨지지 않았는지**를 본다. 메모리를 다시
    // 보는 것은 확인이 아니다(방금 우리가 고친 그 객체다).
    private bool VerifyFromDisk(
        string profileId,
        IReadOnlyList<Item> intended,
        string stashRootId,
        string equipmentRootId,
        IReadOnlyList<InventoryItemState> before,
        IReadOnlyCollection<string>? loadedItemIds,
        GearSlotFacts? slotFacts,
        out string mismatch)
    {
        mismatch = string.Empty;
        var filePath = Path.Combine(ProfileDirectory, profileId + ".json");
        List<Item>? onDisk;
        try
        {
            onDisk = jsonUtil.DeserializeFromFile<SptProfile>(filePath)?.CharacterData?.PmcData?.Inventory?.Items;
        }
        catch (Exception ex)
        {
            mismatch = $"프로필 파일 '{filePath}' 을(를) 다시 읽지 못했다: {ex.Message}";
            return false;
        }

        if (onDisk == null)
        {
            mismatch = $"프로필 파일 '{filePath}' 에서 인벤토리를 읽지 못했다.";
            return false;
        }

        var diskById = new Dictionary<string, Item>(StringComparer.Ordinal);
        foreach (var item in onDisk)
        {
            if (item?.Id != null) diskById[item.Id.ToString()] = item;
        }

        if (diskById.Count != intended.Count)
        {
            mismatch = $"디스크의 아이템 수가 {diskById.Count} 로 기대({intended.Count})와 다르다.";
            return false;
        }

        foreach (var item in intended)
        {
            if (item?.Id == null) continue;
            var id = item.Id.ToString();
            if (!diskById.TryGetValue(id, out var saved))
            {
                mismatch = $"쓴 아이템 '{id}' 이(가) 디스크에 없다.";
                return false;
            }

            if (!string.Equals(saved.ParentId, item.ParentId, StringComparison.Ordinal)
                || !string.Equals(saved.SlotId, item.SlotId, StringComparison.Ordinal))
            {
                mismatch = $"디스크의 아이템 '{id}' 이(가) 기대와 다르다(부모 '{saved.ParentId}' 기대 '{item.ParentId}', 슬롯 '{saved.SlotId}' 기대 '{item.SlotId}').";
                return false;
            }
        }

        // 저장된 상태도 같은 보존 판정을 지나야 한다 - 쓰는 쪽이 실수해도 여기서 걸린다. 두 트리 밖은
        // 손대지 않았으므로 판정에서도 뺀다(제출본과 같은 범위여야 비교가 성립한다).
        var diskIndex = new Dictionary<string, Item>(StringComparer.Ordinal);
        foreach (var item in onDisk)
        {
            if (item?.Id != null) diskIndex[item.Id.ToString()] = item;
        }

        var diskTwoTrees = new List<Item>();
        foreach (var item in onDisk)
        {
            if (IsInTree(diskIndex, item, stashRootId) || IsInTree(diskIndex, item, equipmentRootId)) diskTwoTrees.Add(item);
        }

        // 같은 사실로 본다. 새로 생긴 더미는 **서버가 매긴 id** 라 before 에 없으므로 자리 검사가
        // 그것을 "이번에 옮긴 자리" 로 보고 그대로 판정한다 - 쓰는 쪽이 실수해도 여기서 걸린다.
        var diskVerdict = ProfileInventoryApplyPolicy.Validate(before, ToStates(diskTwoTrees), stashRootId, equipmentRootId, loadedItemIds, slotFacts);
        if (!diskVerdict.Allowed)
        {
            mismatch = $"디스크의 상태가 보존 판정을 통과하지 못했다: {diskVerdict.Reason}";
            return false;
        }

        return true;
    }

    private bool TryRestore(MongoId profileId, BotBaseInventory inventory, List<Item> original)
    {
        try
        {
            inventory.Items = original;
            saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 인벤토리 저장 되돌리기가 실패했다(프로필 '{profileId}'): {ex}");
            return false;
        }
    }

    // 이 아이템이 그 뿌리의 트리 안인가. 부모를 따라 올라가며 보고, 걸음을 끊어 고리에서 돌지 않는다.
    private static bool IsInTree(IReadOnlyDictionary<string, Item> index, Item? item, string rootId)
    {
        var current = item;
        for (var steps = 0; current?.Id != null && steps < 64; steps++)
        {
            if (string.Equals(current.Id.ToString(), rootId, StringComparison.Ordinal)) return true;
            if (string.IsNullOrWhiteSpace(current.ParentId)) return false;
            if (!index.TryGetValue(current.ParentId!, out var parent)) return false;
            current = parent;
        }

        return false;
    }

    private static List<InventoryItemState> ToStates(IEnumerable<Item> items)
    {
        var states = new List<InventoryItemState>();
        foreach (var item in items)
        {
            if (item?.Id == null) continue;
            var count = item.Upd?.StackObjectsCount;
            states.Add(new InventoryItemState(
                item.Id.ToString(),
                item.Template.ToString(),
                item.ParentId,
                item.SlotId,
                count is > 0 ? (int)count.Value : 1));
        }

        return states;
    }
}
