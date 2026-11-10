using System;
using System.Collections.Generic;
using Path = System.IO.Path;
using System.Linq;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace pitTeam.Server.Services;

// 계획 6 B2 — 플레이어 스태시 ↔ 프로필 팀원 스태시 전송. 요청 2.
//
// 저널(StmTransferJournal)의 **두 번째** 호출자이고 **첫 양방향** 호출자다. 꾸러미 구매는 돈만
// 프로필을 넘었고(물건은 새로 샀다), 여기서는 **실물이 프로필 사이를 건넌다** — 설계 §6-5 가
// "프로필 사이를 넘나드는 가장 위험한 조작" 이라 부르고 저널이 실전에서 검증될 때까지 미뤄 둔 바로
// 그것이다(계획 6 §1 에서 그 전제가 풀렸다).
//
// 쓰기 순서는 저널의 계약 그대로고 꾸러미 구매와 같다:
//   Prepare → BeginSourceWrite → (빼는 쪽 저장) → RecordDestinationItemIds → BeginDestinationWrite
//   → (넣는 쪽 저장) → MarkCommitted → Verify(디스크 재독 대조)
//
// **빼는 쪽이 먼저다**(설계 §6-8). 끊기면 복제 대신 소실로 기운다 — 복제는 경제를 망가뜨리고
// 소실은 한 사람의 손해다.
//
// **터질 수 있는 것은 저널을 열기 전에 끝낸다.** 판정도, 자리 찾기도, id 충돌 검사도 전부 복제본
// 위에서 먼저 해 본다. 저널이 열린 뒤에 남는 일은 두 번의 저장뿐이다.
[Injectable(InjectionType.Singleton)]
public class StmStashTransferService(
    StmProfileMemberResolver resolver,
    StmGearEditService gearEdits,
    StmTransferJournal journal,
    StmProfileMemberStore store,
    SaveServer saveServer,
    // 아이템 이름은 서버가 붙인다 — 꾸러미 견적이 쓰는 것과 같은 자리다(B3 의 조회).
    LocaleService localeService,
    JsonUtil jsonUtil,
    FileUtil fileUtil,
    ICloner cloner,
    ISptLogger<StmStashTransferService> logger)
{
    private const string JournalKind = "stash-transfer";

    // 스태시 주 격자. 장비 편집·꾸러미 구매가 쓰는 것과 같은 이름이다.
    private const string StashGridName = "hideout";

    private static string ProfileDirectory => Path.Combine(AppContext.BaseDirectory, "user", "profiles");

    public bool TryTransfer(MongoId sessionId, StmStashTransferRequest request, out StmStashTransferResponse? result, out string? refusalReason)
    {
        result = null;
        refusalReason = null;

        PmcData? liveSource = null;
        List<Item>? sourceOriginal = null;
        PmcData? liveDestination = null;
        List<Item>? destinationOriginal = null;
        MongoId? sourceProfileId = null;
        MongoId? destinationProfileId = null;
        string? journalId = null;

        try
        {
            if (request?.ItemIds == null || request.ItemIds.Count == 0)
            {
                refusalReason = "고른 것이 없어 옮기지 않았습니다.";
                return false;
            }

            if (!resolver.TryResolve(sessionId, request.Aid ?? request.ProfileId, out var member, out var unavailable) || member == null)
            {
                refusalReason = string.IsNullOrWhiteSpace(unavailable)
                    ? "그 팀원을 찾지 못해 옮기지 않았습니다."
                    : $"그 팀원을 쓸 수 없어 옮기지 않았습니다({unavailable}).";
                return false;
            }

            var player = saveServer.GetProfile(sessionId)?.CharacterData?.PmcData;
            if (player?.Inventory?.Items == null)
            {
                refusalReason = "플레이어 프로필을 읽지 못해 옮기지 않았습니다.";
                return false;
            }

            var teammate = member.Pmc;
            if (teammate?.Inventory?.Items == null)
            {
                refusalReason = "팀원 프로필을 읽지 못해 옮기지 않았습니다.";
                return false;
            }

            var teammateProfileId = new MongoId(member.ProfileId);

            // 저장되지 않을 프로필에는 쓰지 않는다. SaveProfileAsync 가 아무것도 쓰지 않고 돌아가므로
            // 메모리만 바뀌고 디스크는 그대로다 — 그 상태로 저널을 닫으면 한쪽이 통째로 사라진다.
            foreach (var (id, what) in new[] { (sessionId, "플레이어"), (teammateProfileId, "팀원") })
            {
                if (saveServer.IsProfileInvalidOrUnloadable(id))
                {
                    logger.Error($"[STM] 스태시 전송 거부: {what} 프로필 '{id}' 가 SPT 에서 손상/적재 불가로 표시돼 있다.");
                    refusalReason = $"{what} 프로필을 저장할 수 없는 상태라 옮기지 않았습니다.";
                    return false;
                }
            }

            // 저널이 이미 막아 둔 팀원이면 여기서 멈춘다 — 끊긴 전송이 닫히기 전에 또 쓰지 않는다.
            if (journal.HasOpenEntry(sessionId, member.ProfileId))
            {
                refusalReason = "이 팀원에게 끝나지 않은 전송 기록이 있어 옮기지 않았습니다. 그 기록을 닫은 뒤 다시 하세요.";
                return false;
            }

            var toTeammate = request.ToTeammate;
            var source = toTeammate ? player : teammate;
            var destination = toTeammate ? teammate : player;
            sourceProfileId = toTeammate ? sessionId : teammateProfileId;
            destinationProfileId = toTeammate ? teammateProfileId : sessionId;

            var sourceStashRoot = source.Inventory?.Stash?.ToString();
            var destinationStashRoot = destination.Inventory?.Stash?.ToString();
            if (string.IsNullOrWhiteSpace(sourceStashRoot) || string.IsNullOrWhiteSpace(destinationStashRoot))
            {
                refusalReason = "스태시를 찾지 못해 옮기지 않았습니다.";
                return false;
            }

            // 1. 판정(순수). 무엇이 옮겨지는지는 여기서 한 번만 정한다 — 양쪽이 같은 목록을 쓴다.
            var sourceItems = source.Inventory!.Items!;
            var byId = new Dictionary<string, StashItem>(StringComparer.Ordinal);
            foreach (var item in sourceItems)
            {
                if (item?.Id == null) continue;
                byId[item.Id.ToString()] = new StashItem(
                    item.Id.ToString(), item.ParentId, item.SlotId, item.Template.ToString());
            }

            var plan = StashTransferPolicy.Decide(
                new StashTransferRequest(sourceProfileId.Value.ToString(), destinationProfileId.Value.ToString(), request.ItemIds),
                byId,
                sourceStashRoot,
                blockedTemplateIds: null);

            if (!plan.Accepted)
            {
                logger.Info($"[STM] 스태시 전송 거부: {plan.Reason}");
                refusalReason = plan.Reason;
                return false;
            }

            var movingIds = new HashSet<string>(plan.MovingItemIds, StringComparer.Ordinal);

            // 2. **id 충돌 검사.** 아이템은 id 를 그대로 들고 건너간다(새로 만들지 않는다 — 만들면
            //    그게 생성기다). 넣는 쪽에 같은 id 가 이미 있으면 복구가 "복제" 로 읽으므로 거절한다.
            var destinationIds = destination.Inventory!.Items!
                .Where(i => i?.Id != null).Select(i => i.Id.ToString()).ToHashSet(StringComparer.Ordinal);
            var clash = plan.MovingItemIds.Where(destinationIds.Contains).ToList();
            if (clash.Count > 0)
            {
                logger.Error($"[STM] 스태시 전송 거부: 같은 id 가 넣는 쪽에 이미 있다({clash.Count}개, 앞 3: {string.Join(", ", clash.Take(3))}).");
                refusalReason = "옮기려는 아이템과 같은 식별자가 받는 쪽에 이미 있어 옮기지 않았습니다.";
                return false;
            }

            // 3. 복제본 위에서 **먼저 해 본다**. 여기서 실패하면 살아 있는 객체는 손대지 않은 채 끝난다.
            var sourceDraft = cloner.Clone(sourceItems) ?? throw new InvalidOperationException("원본 복제 실패");
            var destinationDraft = cloner.Clone(destination.Inventory!.Items!) ?? throw new InvalidOperationException("대상 복제 실패");

            var moving = sourceDraft.Where(i => i?.Id != null && movingIds.Contains(i.Id.ToString())).ToList();
            if (moving.Count != plan.MovingItemIds.Count)
            {
                refusalReason = "옮길 아이템을 모두 찾지 못해 옮기지 않았습니다.";
                return false;
            }

            sourceDraft.RemoveAll(i => i?.Id != null && movingIds.Contains(i.Id.ToString()));
            destinationDraft.AddRange(moving);

            // 고른 것(바닥에 놓인 것)만 새 스태시에 자리를 찾아 준다. 안에 든 것은 부모를 따라간다.
            var pickedIds = request.ItemIds.Select(id => id.Trim()).ToHashSet(StringComparer.Ordinal);
            var placed = 0;
            // 끌어다 놓은 그 한 개는 **놓은 자리에 그대로** 놓는다. 자리가 안 맞으면(막혔거나
            // 좌표가 없으면) 조용히 빈 자리 찾기로 떨어진다 — 사람이 놓은 곳이 안 되면 어딘가에는
            // 놓여야지, 전송 자체가 거절되면 더 나쁘다.
            var anchorId = request.AnchorItemId?.Trim();
            var anchorGrid = string.IsNullOrWhiteSpace(request.AnchorGrid) ? StashGridName : request.AnchorGrid!.Trim();

            foreach (var item in moving.Where(i => pickedIds.Contains(i.Id.ToString())))
            {
                item.ParentId = destinationStashRoot;
                item.SlotId = StashGridName;
                item.Location = null;

                if (!string.IsNullOrEmpty(anchorId)
                    && string.Equals(item.Id.ToString(), anchorId, StringComparison.Ordinal)
                    && request.AnchorX >= 0 && request.AnchorY >= 0)
                {
                    item.SlotId = anchorGrid;
                    item.Location = new ItemLocation
                    {
                        X = request.AnchorX,
                        Y = request.AnchorY,
                        R = (ItemRotation)(request.AnchorRotation == 0 ? 0 : 1),
                    };

                    if (gearEdits.IsContainerSpotFree(destinationDraft, item, destinationStashRoot!, anchorGrid))
                    {
                        placed++;
                        continue;
                    }

                    logger.Info($"[STM] 스태시 전송: 놓은 자리가 막혀 있어 빈 자리를 찾는다({item.Id}, {anchorGrid} {request.AnchorX},{request.AnchorY}).");
                    item.SlotId = StashGridName;
                    item.Location = null;
                }

                // 나머지도 **놓은 자리부터** 차례로 채운다 — 화면의 녹색 덩어리가 거기서 시작한다.
                // 놓은 자리를 모르면(목록형·좌표 없음) 격자 처음부터 찾는다(예전 동작).
                if (request.AnchorX >= 0 && request.AnchorY >= 0
                    && gearEdits.TryPlaceFrom(destinationDraft, item, destinationStashRoot!, anchorGrid, request.AnchorX, request.AnchorY, out _))
                {
                    placed++;
                    continue;
                }

                if (!gearEdits.TryPlaceInContainer(destinationDraft, item, destinationStashRoot!, StashGridName, out var failure))
                {
                    logger.Info($"[STM] 스태시 전송 거부: 받는 쪽 스태시에 자리가 없다({item.Id}). {failure}");
                    refusalReason = "받는 쪽 스태시에 빈 자리가 없어 옮기지 않았습니다. 정리한 뒤 다시 하세요.";
                    return false;
                }

                placed++;
            }

            // 4. 여기서부터가 저널의 창이다. 위에서 터질 수 있는 것은 다 끝났다.
            journalId = journal.Prepare(
                sessionId, JournalKind,
                sourceProfileId.Value.ToString(), destinationProfileId.Value.ToString(),
                plan.MovingItemIds);

            if (!journal.BeginSourceWrite(sessionId, journalId))
            {
                journal.Abort(sessionId, journalId);
                refusalReason = "전송 기록을 열지 못해 옮기지 않았습니다.";
                return false;
            }

            // 4-a. **빼는 쪽 먼저**(설계 §6-8). 이 줄부터 살아 있는 객체가 바뀐다.
            liveSource = source;
            sourceOriginal = cloner.Clone(sourceItems);
            source.Inventory!.Items = sourceDraft;
            saveServer.SaveProfileAsync(sourceProfileId.Value).GetAwaiter().GetResult();

            // 4-b. 넣는 쪽.
            if (!journal.RecordDestinationItemIds(sessionId, journalId, plan.MovingItemIds)
                || !journal.BeginDestinationWrite(sessionId, journalId))
            {
                logger.Error($"[STM] 스태시 전송: 저널이 목적지 쓰기를 허락하지 않았다. 물건은 이미 빠졌다 - 항목 '{journalId}' 를 열어 둔다.");
                refusalReason = "물건이 빠져나간 뒤 받는 단계를 열지 못했습니다. 서버 로그와 전송 기록을 확인하세요.";
                return false;
            }

            liveDestination = destination;
            destinationOriginal = cloner.Clone(destination.Inventory!.Items!);
            destination.Inventory!.Items = destinationDraft;
            saveServer.SaveProfileAsync(destinationProfileId.Value).GetAwaiter().GetResult();

            journal.MarkCommitted(sessionId, journalId);

            // 5. 확인: **디스크에서 다시 읽는다**(§6-8 3단계). 메모리를 다시 보는 것은 확인이 아니다.
            var verified = journal.Verify(sessionId, journalId,
                () => VerifyFromDisk(sourceProfileId.Value, destinationProfileId.Value, plan.MovingItemIds));
            if (!verified)
            {
                logger.Error($"[STM] 스태시 전송: 저장은 끝났지만 디스크 재독 대조가 통과하지 못했다. 항목 '{journalId}' 를 열어 둔다.");
                refusalReason = "옮겼지만 저장 내용을 디스크에서 확인하지 못했습니다. 두 프로필의 스태시를 직접 확인하세요.";
                return false;
            }

            // 6. 팀원 쪽이 바뀌었으면 '손으로 고침' 으로 표시한다(§6-4, 한 방향). 그러지 않으면 다음
            //    레이드의 임시 장비가 이 전송을 덮는다.
            try
            {
                store.Update(sessionId, member.ProfileId, new StmProfileMemberChange(null, true));
            }
            catch (Exception ex)
            {
                logger.Error($"[STM] 스태시 전송: 물건은 옮겼지만 '손으로 고침' 표시를 쓰다 실패했다: {ex}");
            }

            result = new StmStashTransferResponse
            {
                Picked = placed,
                Moved = plan.MovingItemIds.Count,
                ToTeammate = toTeammate,
            };
            logger.Info(
                $"[STM] 스태시 전송 완료: {(toTeammate ? "플레이어 → 팀원" : "팀원 → 플레이어")} "
                + $"'{member.ProfileId}' - 고른 {placed}개, 안에 든 것까지 {plan.MovingItemIds.Count}개 (항목 '{journalId}').");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 스태시 전송 실패 (Aid '{request?.Aid}', 세션 '{sessionId}'): {ex}");

            // 저널을 열기 전에 터졌으면 살아 있는 객체는 그대로다. 연 뒤라면 되돌릴 수 있는 만큼
            // 되돌리고 항목은 열어 둔다 - 복구가 실물을 보고 정한다.
            Restore(liveSource, sourceOriginal, sourceProfileId, "빼는 쪽");
            Restore(liveDestination, destinationOriginal, destinationProfileId, "넣는 쪽");

            refusalReason = journalId == null
                ? "옮기는 중 오류가 났습니다. 아무것도 바뀌지 않았습니다."
                : "옮기는 중 오류가 났습니다. 두 프로필의 스태시를 직접 확인하세요.";
            return false;
        }
    }

    // 계획 6 B3 의 조회 — 화면이 좌우 두 칸을 그릴 재료. **바닥에 놓인 것만** 싣고 안에 든 것은
    // 수만 센다(전송의 단위가 바닥에 놓인 것이라서다). 이름은 여기서 붙인다 — 서버에 사전이 있다.
    public bool TryReadPair(MongoId sessionId, StmStashPairRequest request, out StmStashPairResponse? result, out string? refusalReason)
    {
        result = null;
        refusalReason = null;

        try
        {
            if (!resolver.TryResolve(sessionId, request?.Aid ?? request?.ProfileId, out var member, out var unavailable) || member == null)
            {
                refusalReason = string.IsNullOrWhiteSpace(unavailable)
                    ? "그 팀원을 찾지 못했습니다."
                    : $"그 팀원을 쓸 수 없습니다({unavailable}).";
                return false;
            }

            var player = saveServer.GetProfile(sessionId)?.CharacterData?.PmcData;
            if (player?.Inventory?.Items == null || member.Pmc?.Inventory?.Items == null)
            {
                refusalReason = "프로필을 읽지 못했습니다.";
                return false;
            }

            result = new StmStashPairResponse
            {
                Player = BuildSide(sessionId.ToString(), player.Info?.Nickname ?? "나", player),
                Teammate = BuildSide(member.ProfileId, member.Nickname, member.Pmc),
            };
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 스태시 조회 실패 (세션 '{sessionId}'): {ex}");
            refusalReason = "스태시를 읽는 중 오류가 났습니다.";
            return false;
        }
    }

    private StmStashSideWire BuildSide(string profileId, string? nickname, PmcData pmc)
    {
        var items = pmc.Inventory!.Items!;
        var rootId = pmc.Inventory.Stash?.ToString() ?? string.Empty;
        var side = new StmStashSideWire
        {
            ProfileId = profileId,
            Nickname = nickname ?? string.Empty,
            StashRootId = rootId,
        };

        if (string.IsNullOrWhiteSpace(rootId)) return side;

        // 자손 수를 세려면 부모 → 자식 색인이 한 번 필요하다. 고리가 있어도 멈추도록 담은 것은 다시 세지 않는다.
        var children = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item?.Id == null || string.IsNullOrEmpty(item.ParentId)) continue;
            if (!children.TryGetValue(item.ParentId!, out var list))
            {
                list = [];
                children[item.ParentId!] = list;
            }

            list.Add(item.Id.ToString());
        }

        // 격자용 트리. **루트가 맨 앞**이어야 한다(`FlatItemsToTree` 가 그 순서를 본다 —
        // gear-stash 가 같은 규칙으로 싣는다). 루트 밖의 것(입은 장비 등)은 넣지 않는다.
        var subtree = new HashSet<string>(StringComparer.Ordinal) { rootId };
        var queue = new Queue<string>();
        queue.Enqueue(rootId);
        while (queue.Count > 0)
        {
            if (!children.TryGetValue(queue.Dequeue(), out var kids)) continue;
            foreach (var kid in kids)
            {
                if (subtree.Add(kid)) queue.Enqueue(kid);
            }
        }

        var treeItems = items.Where(i => i?.Id != null && subtree.Contains(i.Id.ToString())).ToList();
        var root = treeItems.FirstOrDefault(i => string.Equals(i.Id.ToString(), rootId, StringComparison.Ordinal));
        if (root != null)
        {
            treeItems.Remove(root);
            treeItems.Insert(0, root);
            // 화면이 들고 노는 복제본이다 — 살아 있는 프로필 객체를 내보내지 않는다.
            side.Tree = cloner.Clone(treeItems) ?? [];
        }

        var locale = localeService.GetLocaleDb();
        foreach (var item in items)
        {
            if (item?.Id == null) continue;
            if (!string.Equals(item.ParentId, rootId, StringComparison.Ordinal)) continue;
            if (!string.Equals(item.SlotId, StashGridName, StringComparison.Ordinal)) continue;

            var tpl = item.Template.ToString();
            side.Items.Add(new StmStashEntryWire
            {
                Id = item.Id.ToString(),
                Tpl = tpl,
                Name = ResolveName(locale, tpl),
                Stack = (int)(item.Upd?.StackObjectsCount ?? 1),
                Contained = CountDescendants(item.Id.ToString(), children),
            });
        }

        return side;
    }

    private static string ResolveName(IDictionary<string, string>? locale, string tpl)
    {
        if (locale != null && locale.TryGetValue($"{tpl} ShortName", out var shortName) && !string.IsNullOrWhiteSpace(shortName))
        {
            return shortName;
        }

        if (locale != null && locale.TryGetValue($"{tpl} Name", out var name) && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        // 사전에 없으면 **그 사실이 보이게** 둔다. 빈 줄을 그리면 사람이 무엇을 고르는지 모른다.
        return tpl;
    }

    private static int CountDescendants(string id, Dictionary<string, List<string>> children)
    {
        var count = 0;
        var seen = new HashSet<string>(StringComparer.Ordinal) { id };
        var queue = new Queue<string>();
        queue.Enqueue(id);
        while (queue.Count > 0)
        {
            if (!children.TryGetValue(queue.Dequeue(), out var kids)) continue;
            foreach (var kid in kids)
            {
                if (!seen.Add(kid)) continue;
                count++;
                queue.Enqueue(kid);
            }
        }

        return count;
    }

    // 총량 보존을 **디스크에서** 본다: 넣는 쪽에 전부 있고, 빼는 쪽에 하나도 없어야 한다.
    // 한쪽만 보면 복제(양쪽 다 있음)를 통과시킨다.
    private bool VerifyFromDisk(MongoId sourceProfileId, MongoId destinationProfileId, IReadOnlyList<string> movingIds)
    {
        var sourceSaved = ReadProfile(sourceProfileId);
        var destinationSaved = ReadProfile(destinationProfileId);
        if (sourceSaved == null || destinationSaved == null)
        {
            logger.Error("[STM] 스태시 전송 확인: 저장된 프로필을 디스크에서 읽지 못했다.");
            return false;
        }

        var destinationIds = (destinationSaved.Inventory?.Items ?? []).Select(i => i.Id.ToString()).ToHashSet(StringComparer.Ordinal);
        var missing = movingIds.Where(id => !destinationIds.Contains(id)).ToList();
        if (missing.Count > 0)
        {
            logger.Error($"[STM] 스태시 전송 확인: 옮긴 것 {missing.Count}개가 받는 쪽에 없다(앞 3: {string.Join(", ", missing.Take(3))}).");
            return false;
        }

        var sourceIds = (sourceSaved.Inventory?.Items ?? []).Select(i => i.Id.ToString()).ToHashSet(StringComparer.Ordinal);
        var leaked = movingIds.Where(sourceIds.Contains).ToList();
        if (leaked.Count > 0)
        {
            logger.Error($"[STM] 스태시 전송 확인: 옮긴 것 {leaked.Count}개가 빼는 쪽에도 남아 있다(복제).");
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
            logger.Error($"[STM] 스태시 전송 확인: 프로필 '{profileId}' 를 디스크에서 읽지 못했다: {ex}");
            return null;
        }
    }

    private void Restore(PmcData? live, List<Item>? original, MongoId? profileId, string what)
    {
        if (live?.Inventory == null || original == null || profileId == null) return;

        try
        {
            live.Inventory.Items = original;
            saveServer.SaveProfileAsync(profileId.Value).GetAwaiter().GetResult();
            logger.Info($"[STM] 스태시 전송: {what} 프로필 '{profileId}' 를 되돌렸다.");
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 스태시 전송: {what} 프로필 '{profileId}' 를 되돌리다 실패했다: {ex}");
        }
    }
}
