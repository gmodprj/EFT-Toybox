using System;
using System.Collections.Generic;
using Path = System.IO.Path;
using System.Linq;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace pitTeam.Server.Services;

// 계획 4 Task 3 — 고른 장비 빌드를 팀원에게 실제로 입힌다. **프로필 두 개를 한 번에 고치는 첫 조작**이고,
// 그래서 전송 저널(StmTransferJournal)의 첫 사용자다(인계 열린 항목 6).
//
// 사용자가 정한 셋(2026-11-02)이 위험을 많이 깎아 준다:
//   · 물건은 **전부 새로 산다** - 플레이어 스태시에서 꺼내지 않으므로 같은 아이템이 두 프로필에 걸치는 창이 없다.
//   · 헌 장비는 **그 팀원 스태시로** - 프로필 사이를 넘지 않는다.
//   · 돈만 플레이어에게서 나간다.
// 그래서 프로필을 넘는 것은 돈 하나이고, 저널이 지키는 것은 "돈은 나갔는데 물건이 안 들어간 창" 이다.
//
// 쓰기 순서(저널의 계약 그대로):
//   Prepare → BeginSourceWrite → (플레이어 저장) → RecordDestinationItemIds → BeginDestinationWrite
//   → (팀원 저장) → MarkCommitted → Verify(디스크 재독 대조)
//
// **터질 수 있는 것은 전부 저널을 열기 전에 끝낸다.** 값 매기기도, 돈 계산도, 새 아이템 만들기도, 스태시
// 자리 찾기도 복제본 위에서 먼저 해 본다. 저널이 열린 뒤에 남는 일은 두 번의 저장뿐이다.
[Injectable(InjectionType.Singleton)]
public class StmGearKitPurchaseService(
    StmProfileMemberResolver resolver,
    StmGearKitQuoteService quotes,
    StmGearEditService gearEdits,
    StmTransferJournal journal,
    StmProfileMemberStore store,
    FriendlyTeammateService teammateService,
    SaveServer saveServer,
    ItemHelper itemHelper,
    JsonUtil jsonUtil,
    FileUtil fileUtil,
    ICloner cloner,
    ISptLogger<StmGearKitPurchaseService> logger)
{
    private const string JournalKind = "kit-purchase";

    // 헌 장비가 돌아갈 자리. 플레이어 스태시의 주 격자와 같은 이름이고, 장비 편집이 벗기기에 쓰는 것과 같다.
    private const string StashGridName = "hideout";

    private static string ProfileDirectory => Path.Combine(AppContext.BaseDirectory, "user", "profiles");

    public bool TryBuy(MongoId sessionId, StmGearKitPurchaseRequest request, out StmGearKitPurchaseResponse? result, out string? refusalReason)
    {
        result = null;
        refusalReason = null;

        // 저장 전에 되돌릴 거리. 살아 있는 객체를 바꾼 뒤 터지면 여기로 되돌린다.
        PmcData? livePlayer = null;
        List<Item>? playerOriginal = null;
        PmcData? liveTeammate = null;
        List<Item>? teammateOriginal = null;
        MongoId? liveTeammateProfileId = null;
        string? journalId = null;

        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.BuildId))
            {
                refusalReason = "고른 빌드가 없어 사지 않았습니다.";
                return false;
            }

            // 1. 값을 **지금 다시** 매긴다. 화면이 보여 준 값과 견주는 것이 관문의 핵심이다.
            var quoteRequest = new StmGearKitQuoteRequest
            {
                Aid = request.Aid,
                ProfileId = request.ProfileId,
                BuildId = request.BuildId,
                Owner = request.Owner,
            };
            if (!quotes.TryQuote(sessionId, quoteRequest, out var fresh, out var quoteRefusal) || fresh == null)
            {
                refusalReason = quoteRefusal ?? "값을 매기지 못해 사지 않았습니다.";
                return false;
            }

            // 2. 해석(불변식 3). 견적이 이미 같은 판정을 했지만, 이 경로는 **쓰기** 라 자기 손으로 다시 묻는다.
            if (!resolver.TryResolve(sessionId, request.Aid, out var member, out var unavailableReason) || member == null)
            {
                refusalReason = $"이 프로필을 지금 쓸 수 없어 사지 않았습니다({unavailableReason ?? "Unavailable"}).";
                return false;
            }

            if (!string.Equals(member.ProfileId, request.ProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 꾸러미 구매 거부: 요청의 프로필 '{request.ProfileId}' 과(와) 해석된 '{member.ProfileId}' 이(가) 다르다.");
                refusalReason = "요청한 프로필과 이 팀원의 프로필이 달라 사지 않았습니다.";
                return false;
            }

            // 3. 관문. 순수 정책 하나가 정한다.
            var gate = GearKitPurchasePolicy.Decide(
                FriendlyModeRequestGate.HasActiveRaid,
                journal.HasOpenEntry(sessionId, member.ProfileId),
                fresh.ItemsToBuy,
                request.ExpectedTotal,
                fresh.Total,
                fresh.PlayerRoubles);
            if (gate != GearKitPurchaseGate.Allowed)
            {
                refusalReason = GearKitPurchasePolicy.Describe(gate, fresh.Total, fresh.PlayerRoubles);
                logger.Info($"[STM] 꾸러미 구매 거부({gate}): 팀원 '{member.ProfileId}', 빌드 '{request.BuildId}'.");
                return false;
            }

            // 4. 두 프로필이 저장 가능한 상태인가. 메모리만 바뀌고 디스크는 그대로인 유령 재산을 만들지 않는다.
            var teammateProfileId = new MongoId(member.ProfileId);
            if (saveServer.IsProfileInvalidOrUnloadable(sessionId) || saveServer.IsProfileInvalidOrUnloadable(teammateProfileId))
            {
                refusalReason = "프로필이 손상으로 표시돼 있어 저장할 수 없습니다. 아무것도 사지 않았습니다.";
                return false;
            }

            var player = saveServer.GetProfile(sessionId)?.CharacterData?.PmcData;
            if (player?.Inventory?.Items == null)
            {
                refusalReason = "플레이어 인벤토리를 읽을 수 없어 사지 않았습니다.";
                return false;
            }

            var teammate = member.Pmc;
            var teammateInventory = teammate?.Inventory;
            var stashRootId = teammateInventory?.Stash?.ToString();
            var equipmentRootId = teammateInventory?.Equipment?.ToString();
            if (teammateInventory?.Items == null || string.IsNullOrWhiteSpace(stashRootId) || string.IsNullOrWhiteSpace(equipmentRootId))
            {
                refusalReason = "이 팀원의 인벤토리 루트를 읽을 수 없어 사지 않았습니다.";
                return false;
            }

            // 5. **복제본 위에서 전부 해 본다.** 하나라도 실패하면 살아 있는 객체는 그대로다.
            var playerDraft = cloner.Clone(player) ?? throw new InvalidOperationException("StmKitClonePlayerFailed");
            try
            {
                // 돈 규칙의 집은 PIT 하나다. 모자라면 여기서 던지고, 아직 아무것도 쓰지 않았다.
                teammateService.DeductRoublesFromPlayerStash(playerDraft, fresh.Total);
            }
            catch (Exception ex)
            {
                logger.Info($"[STM] 꾸러미 구매 거부: 스태시의 돈이 모자란다({fresh.Total}₽). {ex.Message}");
                refusalReason = $"스태시에 있는 돈이 모자랍니다. {fresh.Total:N0} ₽ 가 필요합니다.";
                return false;
            }

            var build = FindBuild(fresh.Owner == StmGearKitQuoteService.OwnerTeammate ? member.ProfileId : sessionId.ToString(), fresh.BuildId);
            if (build?.Items == null)
            {
                refusalReason = "고른 빌드를 다시 읽지 못해 사지 않았습니다.";
                return false;
            }

            var plan = GearBuildPlanPolicy.Plan(
                build.Items.Select(ToRow), build.Root.ToString(),
                teammateInventory.Items.Select(ToRow), equipmentRootId,
                AttachmentSlotsOf);

            var teammateDraft = cloner.Clone(teammateInventory.Items) ?? throw new InvalidOperationException("StmKitCloneTeammateFailed");

            // 5-a. 헌 장비를 그 팀원 스태시로. 자리를 못 찾으면 전체를 거절한다 - 반쯤 벗은 상태를 남기지 않는다.
            foreach (var slot in plan.Slots.Where(s => s.ItemsToStash.Count > 0))
            {
                var topId = slot.ItemsToStash[0].Id;
                var target = teammateDraft.FirstOrDefault(i => string.Equals(i.Id.ToString(), topId, StringComparison.Ordinal));
                if (target == null)
                {
                    refusalReason = "벗길 장비를 찾지 못해 사지 않았습니다.";
                    return false;
                }

                target.ParentId = stashRootId;
                target.SlotId = StashGridName;
                target.Location = null;
                if (!gearEdits.TryPlaceInContainer(teammateDraft, target, stashRootId!, StashGridName, out var placeFailure))
                {
                    logger.Info($"[STM] 꾸러미 구매 거부: 헌 장비를 스태시에 둘 자리가 없다({slot.SlotId}). {placeFailure}");
                    refusalReason = "이 팀원의 스태시에 빈 자리가 없어 헌 장비를 둘 수 없습니다. 스태시를 정리한 뒤 다시 하세요.";
                    return false;
                }
            }

            // 5-b. 산 물건을 만든다. **새 id 로 만든다** - 빌드의 id 를 그대로 쓰면 다른 프로필의 아이템과
            //      같은 id 가 생겨 복구가 "복제" 로 읽는다.
            var created = CreatePurchasedItems(plan, build.Items, equipmentRootId!, out var createFailure);
            if (created == null)
            {
                logger.Error($"[STM] 꾸러미 구매 거부: 살 물건을 만들지 못했다. {createFailure}");
                refusalReason = "살 물건을 만들지 못해 사지 않았습니다. 서버 로그를 확인하세요.";
                return false;
            }

            teammateDraft.AddRange(created);
            var newItemIds = created.Select(i => i.Id.ToString()).ToList();

            // 6. 여기서부터가 저널의 창이다. 위에서 터질 수 있는 것은 다 끝났다.
            journalId = journal.Prepare(sessionId, JournalKind, sessionId.ToString(), member.ProfileId, newItemIds);

            if (!journal.BeginSourceWrite(sessionId, journalId))
            {
                journal.Abort(sessionId, journalId);
                refusalReason = "전송 기록을 열지 못해 사지 않았습니다.";
                return false;
            }

            // 6-a. 플레이어 저장(돈). 이 줄부터 살아 있는 객체가 바뀐다.
            livePlayer = player;
            playerOriginal = cloner.Clone(player.Inventory.Items);
            player.Inventory.Items = playerDraft.Inventory!.Items!;
            saveServer.SaveProfileAsync(sessionId).GetAwaiter().GetResult();

            // 6-b. 팀원 저장(물건).
            if (!journal.RecordDestinationItemIds(sessionId, journalId, newItemIds)
                || !journal.BeginDestinationWrite(sessionId, journalId))
            {
                logger.Error($"[STM] 꾸러미 구매: 저널이 목적지 쓰기를 허락하지 않았다. 돈은 이미 나갔다 - 항목 '{journalId}' 를 열어 둔다.");
                refusalReason = "돈은 빠져나갔지만 장비를 넣는 단계를 열지 못했습니다. 서버 로그와 전송 기록을 확인하세요.";
                return false;
            }

            liveTeammate = teammate;
            liveTeammateProfileId = teammateProfileId;
            teammateOriginal = cloner.Clone(teammateInventory.Items);
            teammateInventory.Items = teammateDraft;
            saveServer.SaveProfileAsync(teammateProfileId).GetAwaiter().GetResult();

            journal.MarkCommitted(sessionId, journalId);

            // 7. 확인: **디스크에서 다시 읽는다**(§6-8 3단계, 불변식 6). 메모리를 다시 보는 것은 확인이 아니다.
            var verified = journal.Verify(sessionId, journalId, () => VerifyFromDisk(sessionId, teammateProfileId, newItemIds, fresh.Total, playerOriginal!));
            if (!verified)
            {
                // 저널 항목은 열린 채로 남는다(이 팀원의 편집이 막힌다). 되돌리지 않는다: 두 저장이 끝났고
                // 디스크 상태를 추측으로 뒤집는 쓰기를 더하는 것이 더 위험하다. 사람에게 알린다.
                logger.Error($"[STM] 꾸러미 구매: 저장은 끝났지만 디스크 재독 대조가 통과하지 못했다. 항목 '{journalId}' 를 열어 둔다.");
                refusalReason = "장비를 샀지만 저장 내용을 디스크에서 확인하지 못했습니다. 두 프로필의 장비·스태시·돈을 직접 확인하세요.";
                return false;
            }

            // 8. 손으로 고친 팀원으로 표시한다(§6-4, 한 방향). 그러지 않으면 다음 레이드의 임시 장비가 이 구매를 덮는다.
            bool flagged;
            try
            {
                flagged = store.Update(sessionId, member.ProfileId, new StmProfileMemberChange(null, true));
            }
            catch (Exception ex)
            {
                logger.Error($"[STM] 꾸러미 구매: 장비는 들어갔지만 '손으로 고침' 표시를 쓰다 실패했다: {ex}");
                flagged = false;
            }

            if (!flagged)
            {
                refusalReason = "장비는 샀지만 '손으로 고침' 표시를 저장하지 못했습니다. 장비 편집을 한 번 저장해 주세요.";
                return false;
            }

            result = new StmGearKitPurchaseResponse
            {
                BuildId = fresh.BuildId,
                BuildName = fresh.BuildName,
                Paid = fresh.Total,
                ItemsBought = newItemIds.Count,
                ItemsStashed = plan.AllItemsToStash.Count,
            };
            logger.Info($"[STM] 꾸러미 구매 완료: 팀원 '{member.ProfileId}' 에 빌드 '{fresh.BuildId}' - {fresh.Total}₽, 산 물건 {result.ItemsBought}개, 스태시로 {result.ItemsStashed}개 (항목 '{journalId}').");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 꾸러미 구매 실패 (Aid '{request?.Aid}', 세션 '{sessionId}'): {ex}");

            // 저널을 열기 전에 터졌으면 살아 있는 객체는 그대로다. 연 뒤라면 되돌릴 수 있는 만큼 되돌리고
            // 항목은 열어 둔다 - 복구가 실물을 보고 정한다.
            Restore(livePlayer, playerOriginal, sessionId, "플레이어");
            if (liveTeammate != null && teammateOriginal != null && liveTeammateProfileId != null)
            {
                Restore(liveTeammate, teammateOriginal, liveTeammateProfileId.Value, "팀원");
            }

            refusalReason = journalId == null
                ? "장비를 사는 중 오류가 났습니다. 아무것도 바뀌지 않았습니다."
                : "장비를 사는 중 오류가 났습니다. 두 프로필의 장비·스태시·돈을 직접 확인하세요.";
            return false;
        }
    }

    // 빌드의 하위 묶음을 **새 id** 로 다시 만든다. 부모 사슬은 새 id 로 이어 붙이고, 뿌리는 팀원의 장비 루트에 건다.
    private List<Item>? CreatePurchasedItems(GearBuildPlan plan, List<Item> buildItems, string equipmentRootId, out string failure)
    {
        failure = string.Empty;
        var byId = buildItems.Where(i => i?.Id != null).ToDictionary(i => i.Id.ToString(), StringComparer.Ordinal);
        var created = new List<Item>();
        var newIdByOldId = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var slot in plan.Slots.Where(s => s.ItemsToBuy.Count > 0))
        {
            foreach (var row in slot.ItemsToBuy)
            {
                if (!byId.TryGetValue(row.Id, out var source))
                {
                    failure = $"빌드에서 아이템 '{row.Id}' 를 찾지 못했다.";
                    return null;
                }

                var copy = cloner.Clone(source);
                if (copy == null)
                {
                    failure = $"아이템 '{row.Id}' 를 복제하지 못했다.";
                    return null;
                }

                var newId = new MongoId();
                newIdByOldId[row.Id] = newId.ToString();
                copy.Id = newId;

                // 격자 좌표는 **격자에 놓인 짐에만** 뜻이 있다. 장비 칸·부착물 칸에 남겨 두면 SPT 가
                // 자리 계산을 엉뚱하게 하고, 거꾸로 짐에서 지우면 조끼 안이 겹치거나 빈 채로 들어간다
                // (2026-11-04: 격자 짐을 사기 시작하면서 생긴 자리다). 부모 템플릿에 그 이름의
                // 격자가 있는지로 가른다.
                string? parentTpl = source.ParentId != null && byId.TryGetValue(source.ParentId.ToString(), out var parentSource)
                    ? parentSource.Template.ToString()
                    : null;
                bool inGrid = parentTpl != null && IsGridSlot(parentTpl, source.SlotId);
                copy.Location = inGrid ? source.Location : null;
                created.Add(copy);
            }
        }

        // 부모 다시 잇기. 뿌리(부모가 빌드 루트인 것)는 팀원의 장비 루트로 간다.
        var createdByNewId = created.ToDictionary(c => c.Id.ToString(), StringComparer.Ordinal);
        foreach (var pair in newIdByOldId)
        {
            if (!byId.TryGetValue(pair.Key, out var source)) continue;
            if (!createdByNewId.TryGetValue(pair.Value, out var copy)) continue;

            var oldParent = source.ParentId?.ToString();
            if (oldParent != null && newIdByOldId.TryGetValue(oldParent, out var newParent))
            {
                copy.ParentId = newParent;
                continue;
            }

            // 빌드 안에 부모가 없다 = 장비 칸에 바로 걸리는 뿌리다.
            copy.ParentId = equipmentRootId;
        }

        return created;
    }

    // 디스크에서 두 프로필을 다시 읽어 셋을 본다:
    //   (a) 산 물건의 새 id 가 팀원 프로필에 전부 있다,
    //   (b) 플레이어의 루블 합이 정확히 낸 만큼 줄었다,
    //   (c) 플레이어 쪽에 새 id 가 끼어들지 않았다(돈만 나가야 한다).
    private bool VerifyFromDisk(MongoId sessionId, MongoId teammateProfileId, List<string> newItemIds, int paid, List<Item> playerBefore)
    {
        var teammateSaved = ReadProfile(teammateProfileId);
        var playerSaved = ReadProfile(sessionId);
        if (teammateSaved == null || playerSaved == null)
        {
            logger.Error("[STM] 꾸러미 구매 확인: 저장된 프로필을 디스크에서 읽지 못했다.");
            return false;
        }

        var teammateIds = (teammateSaved.Inventory?.Items ?? []).Select(i => i.Id.ToString()).ToHashSet(StringComparer.Ordinal);
        var missing = newItemIds.Where(id => !teammateIds.Contains(id)).ToList();
        if (missing.Count > 0)
        {
            logger.Error($"[STM] 꾸러미 구매 확인: 산 물건 {missing.Count}개가 팀원 프로필에 없다(앞 3: {string.Join(", ", missing.Take(3))}).");
            return false;
        }

        var playerIds = (playerSaved.Inventory?.Items ?? []).Select(i => i.Id.ToString()).ToHashSet(StringComparer.Ordinal);
        var leaked = newItemIds.Where(playerIds.Contains).ToList();
        if (leaked.Count > 0)
        {
            logger.Error($"[STM] 꾸러미 구매 확인: 산 물건 {leaked.Count}개가 플레이어 쪽에도 있다(복제).");
            return false;
        }

        var before = playerBefore.Where(FriendlyTeammateService.IsPaymentMoney).Sum(i => (long)(i.Upd?.StackObjectsCount ?? 1));
        var after = (playerSaved.Inventory?.Items ?? []).Where(FriendlyTeammateService.IsPaymentMoney).Sum(i => (long)(i.Upd?.StackObjectsCount ?? 1));
        if (before - after != paid)
        {
            logger.Error($"[STM] 꾸러미 구매 확인: 플레이어의 돈이 {before - after}₽ 줄었는데 청구는 {paid}₽ 였다.");
            return false;
        }

        return true;
    }

    private PmcData? ReadProfile(MongoId profileId)
    {
        try
        {
            var path = Path.Combine(ProfileDirectory, $"{profileId}.json");
            return jsonUtil.Deserialize<SptProfile>(fileUtil.ReadFile(path))?.CharacterData?.PmcData;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 꾸러미 구매 확인: 프로필 '{profileId}' 을(를) 디스크에서 읽지 못했다: {ex.Message}");
            return null;
        }
    }

    private void Restore(PmcData? live, List<Item>? original, MongoId profileId, string whose)
    {
        if (live?.Inventory == null || original == null) return;
        try
        {
            live.Inventory.Items = original;
            saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();
            logger.Info($"[STM] 꾸러미 구매: {whose} 프로필을 원래대로 되돌렸다.");
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 꾸러미 구매: {whose} 프로필을 되돌리지 못했다: {ex}");
        }
    }

    private EquipmentBuild? FindBuild(string profileId, string buildId)
    {
        try
        {
            return saveServer.GetProfile(new MongoId(profileId))?.UserBuildData?.EquipmentBuilds?
                .FirstOrDefault(b => b != null && string.Equals(b.Id.ToString(), buildId, StringComparison.Ordinal));
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 꾸러미 구매: 프로필 '{profileId}' 의 빌드를 읽지 못했다: {ex.Message}");
            return null;
        }
    }

    private static GearItemRow ToRow(Item item) =>
        new(item.Id.ToString(), item.Template.ToString(), item.ParentId?.ToString(), item.SlotId,
            // 묶음 수량. 없으면 한 점이다 - 탄 묶음만 이 값이 1 보다 크다.
            (int?)item.Upd?.StackObjectsCount ?? 1);

    // 하위로 따라 내려갈 자리 **전부**의 이름. 어휘가 좁으면 그만큼이 빌드에서 조용히 빠진다
    // (2026-11-04 사용자 실측: 산 탄창이 비어 있고 조끼 안이 텅 비었다).
    //
    // `Slots` 만 읽던 것이 그 결함이었다. 자리는 템플릿에서 **네 군데**에 나뉘어 있다 -
    // `Slots`(부착물·방탄판), `Cartridges`(탄창의 탄), `StackSlots`(탄박스), `Chambers`(약실),
    // 그리고 `Grids`(가방·조끼 안의 짐). 같은 함정을 2026-11-03 에 삽탄에서 한 번 겪었고
    // (`StmGearSlotFactsBuilder` 가 그때 넷을 다 읽게 됐다) **이 경로만 옛 어휘로 남아 있었다.**
    //
    // 격자(`Grids`)를 넣는 것은 판단이 바뀐 것이다. 옛 주석은 "짐은 장비가 아니고 사 주는 대상도
    // 아니다" 였는데, 사용자가 고른 꾸러미에 **그 짐까지 들어 있다** - 화면이 보여 준 것과 산 것이
    // 다르면 그것이 결함이다. 값은 같은 목록(`plan.AllItemsToBuy`)으로 매기므로 산 만큼 낸다.
    internal static IEnumerable<string> ChildSlotNamesOf(TemplateItem? template)
    {
        var props = template?.Properties;
        if (props == null)
        {
            return [];
        }

        var names = new List<string>();
        void Add(string? name)
        {
            if (!string.IsNullOrEmpty(name)) names.Add(name!);
        }

        foreach (var slot in props.Slots ?? []) Add(slot?.Name);
        foreach (var slot in props.Cartridges ?? []) Add(slot?.Name);
        foreach (var slot in props.StackSlots ?? []) Add(slot?.Name);
        foreach (var slot in props.Chambers ?? []) Add(slot?.Name);
        foreach (var grid in props.Grids ?? []) Add(grid?.Name);
        return names;
    }

    private IEnumerable<string> AttachmentSlotsOf(string tpl)
    {
        try
        {
            return ChildSlotNamesOf(itemHelper.GetItem(new MongoId(tpl)).Value);
        }
        catch
        {
            return [];
        }
    }

    // 그 자리가 **격자**인가. 격자에 놓인 짐은 좌표(`Location`)가 뜻을 가지므로 지우면 안 된다.
    private bool IsGridSlot(string tpl, string? slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId)) return false;
        try
        {
            foreach (var grid in itemHelper.GetItem(new MongoId(tpl)).Value?.Properties?.Grids ?? [])
            {
                if (string.Equals(grid?.Name, slotId, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
