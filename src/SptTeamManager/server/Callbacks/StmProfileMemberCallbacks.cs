using pitTeam.Server.Models;
using pitTeam.Server.Services;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class StmProfileMemberCallbacks(HttpResponseUtil httpResponse, StmProfileMemberStore store, StmProfileCatalog catalog,
    StmTransferJournal journal, StmGearEditService gearEdits, StmGearBuildCatalog gearBuilds,
    StmGearKitQuoteService kitQuotes, StmGearKitPurchaseService kitPurchases,
    ISptLogger<StmProfileMemberCallbacks> logger)
{
    public ValueTask<string> List(string url, EmptyRequestData request, MongoId sessionId) =>
        Run("list", () => store.List(sessionId));

    // Roster screen data. The registration list is joined against a fresh catalog read on every
    // call (store.List does not reconcile). Nothing is cached or written: a copy of this join
    // would be the "ghost asset" entrance (design 4).
    public ValueTask<string> Roster(string url, EmptyRequestData request, MongoId sessionId) =>
        Run("roster", () =>
        {
            var registered = store.List(sessionId);
            var registeredIds = registered.Select(m => m.ProfileId).ToList();
            // 로스터 줄의 autoJoin·handEdited 는 등록 정보에서 채운다. 카탈로그(프로필 파일)는 이 둘을 모른다.
            var autoJoinIds = registered.Where(m => m.AutoJoin).Select(m => m.ProfileId).ToHashSet(StringComparer.Ordinal);
            var handEditedIds = registered.Where(m => m.HandEdited).Select(m => m.ProfileId).ToHashSet(StringComparer.Ordinal);
            var candidates = catalog.GetCandidates(sessionId.ToString());
            // 레이드에 무엇을 입고 나가는지의 판정. **두 조각이 서로 다른 곳에 있어** 여기서 합친다:
            // 손으로 고쳤는지는 등록 정보에, 입은 장비와 무기는 프로필(카탈로그)에 있다.
            // 손으로 고쳤는지만 보면 틀린다 - 손대지 않아도 장비와 무기가 다 있으면 실물로 나간다.
            var gearByProfileId = candidates
                .Where(c => c?.Gear != null)
                .GroupBy(c => c.ProfileId, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First().Gear!, StringComparer.Ordinal);

            string GearKindOf(string profileId)
            {
                if (!gearByProfileId.TryGetValue(profileId, out var facts)) return string.Empty;
                return SpawnGearPolicy
                    .Decide(facts.HasAnyEquipment, facts.HasWeaponInRequiredSlot, handEditedIds.Contains(profileId))
                    .Kind.ToString();
            }
            return new StmRosterResponse
            {
                Members = ProfileRosterPolicy.JoinRegistered(candidates, registeredIds)
                    .Select(r => new StmRosterRow
                    {
                        ProfileId = r.ProfileId, Aid = r.Aid, Nickname = r.Nickname, Level = r.Level,
                        Side = r.Side, Status = r.Status.ToString(), AutoJoin = autoJoinIds.Contains(r.ProfileId),
                        HandEdited = handEditedIds.Contains(r.ProfileId),
                        Gear = GearKindOf(r.ProfileId),
                    }).ToList(),
                Candidates = ProfileRosterPolicy.SelectUnregisteredReady(candidates, registeredIds)
                    .Select(c => new StmRosterRow
                    {
                        ProfileId = c.ProfileId, Aid = c.Aid, Nickname = c.Nickname, Level = c.Level,
                        Side = c.Side, Status = RosterRowStatus.Ready.ToString(),
                    }).ToList(),
            };
        });

    public ValueTask<string> Add(string url, StmProfileMemberRequest request, MongoId sessionId) =>
        Run("add", () =>
        {
            store.Add(sessionId, request.ProfileId ?? string.Empty);
            return store.List(sessionId);
        });

    public ValueTask<string> Remove(string url, StmProfileMemberRequest request, MongoId sessionId) =>
        Run("remove", () =>
        {
            store.Remove(sessionId, request.ProfileId ?? string.Empty);
            return store.List(sessionId);
        });

    // 멈추고 보고한 전송 항목을 플레이어가 직접 닫는다(상세 화면의 닫기 확인창 뒤에만 불린다). 이 길은 STM 의 것이고
    // EFT 의 기존 라우트를 가로채지 않는다. 닫을 수 있는지의 판정은 서버가 다시 한다(TransferEntryClosePolicy):
    // acknowledged 가 true 여도 기동 때 보고되지 않은 항목이나 지금 일관된 항목은 거절된다. 거절 사유는 플레이어가
    // 읽는 말이고, 실패는 성공처럼 보이지 않는다(오류 본문).
    public ValueTask<string> CloseTransfer(string url, StmCloseTransferRequest request, MongoId sessionId)
    {
        if (journal.TryCloseByPlayer(sessionId, request.JournalId ?? string.Empty, request.ProfileId ?? string.Empty, request.Acknowledged, out var message))
        {
            return new ValueTask<string>(httpResponse.NullResponse());
        }

        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // 읽을 수 없는 저널 문서를 플레이어가 통째로 버린다(확인창 뒤에만). 읽히는 문서는 서버가 거절한다.
    public ValueTask<string> DiscardUnreadableJournal(string url, StmDiscardJournalRequest request, MongoId sessionId)
    {
        if (journal.TryDiscardUnreadableByPlayer(sessionId, request.Acknowledged, out var message))
        {
            return new ValueTask<string>(httpResponse.NullResponse());
        }

        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // 프로필 팀원의 장비를 한 아이템 옮긴다(§6-4). STM 자신의 길이고 EFT·PIT 의 기존 라우트를 가로채지 않는다 -
    // PIT 의 default-equipment 는 플레이어 스태시를 출처로 쓰므로 프로필 팀원에게 쓸 수 없고, 그쪽은 사유와 함께
    // 거부된다(FriendlyTeammateCallbacks.SaveDefaultEquipment). 3값 계약이 아니라 2값이다: 이 라우트는 프로필
    // 팀원 전용이라 "PIT 경로로 흘려보낸다" 는 답이 없다 - 프로필 팀원이 아니면 사유와 함께 거부다.
    // 실패는 성공처럼 보이지 않는다(오류 본문 + 플레이어가 읽는 사유).
    public ValueTask<string> EditGear(string url, StmGearEditRequest request, MongoId sessionId)
    {
        // 와이어의 건들을 순수 레코드로 옮긴다. ProfileId 는 봉투에 하나뿐이므로 모든 건이 같은 프로필을
        // 가리킨다(정책이 그 약속을 다시 확인한다). 순서는 클라이언트가 보낸 그대로 - 판정이 순서대로
        // 누적해 보므로 "탄창을 꽂고 그 탄창에 탄을 넣는" 연쇄가 그 순서로 통과한다.
        var edits = (request.Edits ?? [])
            .Where(e => e != null)
            .Select(e => new GearEditRequest(
                request.ProfileId ?? string.Empty,
                e.ItemId ?? string.Empty,
                e.TargetSlotId ?? string.Empty,
                e.TargetParentId))
            .ToList();

        if (gearEdits.TryApply(sessionId, request.Aid, edits, out var rejectionReason))
        {
            return new ValueTask<string>(httpResponse.NullResponse());
        }

        // TryApply 는 false 일 때 사유를 비우지 않지만, 비면 성공으로 읽히지 않게 중립 문구를 쓴다.
        var message = string.IsNullOrWhiteSpace(rejectionReason)
            ? "장비를 고치지 못했습니다. 서버 로그를 확인하세요."
            : rejectionReason!;
        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // 편집기를 열기 전에 그 팀원 자신의 스태시를 읽어 간다(읽기 전용). 열 수 없는 사유는 저장 때와 같은 문장이다.
    // 실패는 성공처럼 보이지 않는다(오류 본문 + 플레이어가 읽는 사유).
    public ValueTask<string> GearStash(string url, StmGearStashRequest request, MongoId sessionId)
    {
        if (gearEdits.TryReadStash(sessionId, request.Aid, request.ProfileId, out var stash, out var refusalReason) && stash != null)
        {
            return new ValueTask<string>(httpResponse.GetBody(stash));
        }

        var message = string.IsNullOrWhiteSpace(refusalReason)
            ? "스태시를 읽지 못했습니다. 서버 로그를 확인하세요."
            : refusalReason!;
        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // 계획 4 Task 1. 읽기 전용이고, 거절 사유는 플레이어가 읽는 말이다(빈 목록으로 조용히 성공하지 않는다 -
    // 빈 목록은 "저장해 둔 빌드가 없다" 는 사실이라 실패와 같은 모양이면 안 된다).
    public ValueTask<string> GearBuilds(string url, StmGearBuildsRequest request, MongoId sessionId)
    {
        if (gearBuilds.TryList(sessionId, request.Aid, request.ProfileId, out var builds, out var refusalReason) && builds != null)
        {
            // 줄마다 값을 매겨 함께 보낸다. 화면이 값을 보여 주려면 목록을 받을 때 알아야 하고,
            // **구매 창과 같은 계산**이어야 한다(다른 계산이면 보여 준 값과 청구가 어긋난다).
            PriceRows(sessionId, request, builds.TeammateBuilds, StmGearKitQuoteService.OwnerTeammate);
            PriceRows(sessionId, request, builds.PlayerBuilds, StmGearKitQuoteService.OwnerPlayer);
            return new ValueTask<string>(httpResponse.GetBody(builds));
        }

        var message = string.IsNullOrWhiteSpace(refusalReason)
            ? "빌드 목록을 읽지 못했습니다. 서버 로그를 확인하세요."
            : refusalReason!;
        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // 목록의 줄마다 값을 매긴다. 한 줄이 실패해도 그 줄만 -1 로 두고 나머지는 그대로 낸다 -
    // 값을 못 매기는 빌드가 하나 있다고 목록 전체를 못 보여 줄 이유는 없다(사지 못할 뿐이다).
    private void PriceRows(MongoId sessionId, StmGearBuildsRequest request, List<StmGearBuildRow>? rows, string owner)
    {
        if (rows == null || rows.Count == 0) return;

        foreach (var row in rows)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.Id)) continue;
            try
            {
                var quoteRequest = new StmGearKitQuoteRequest
                {
                    Aid = request.Aid,
                    ProfileId = request.ProfileId,
                    BuildId = row.Id,
                    Owner = owner,
                };
                row.Price = kitQuotes.TryQuote(sessionId, quoteRequest, out var quote, out _) && quote != null
                    ? quote.Total
                    : -1;
            }
            catch (Exception ex)
            {
                row.Price = -1;
                logger.Warning($"[STM] 빌드 '{row.Id}' 의 값을 매기지 못해 목록에 값 없이 싣는다: {ex.Message}");
            }
        }
    }

    // 계획 4 Task 2. 읽기 전용 견적 - 돈도 물건도 건드리지 않는다.
    public ValueTask<string> KitQuote(string url, StmGearKitQuoteRequest request, MongoId sessionId)
    {
        if (kitQuotes.TryQuote(sessionId, request, out var quote, out var refusalReason) && quote != null)
        {
            return new ValueTask<string>(httpResponse.GetBody(quote));
        }

        var message = string.IsNullOrWhiteSpace(refusalReason)
            ? "값을 매기지 못했습니다. 서버 로그를 확인하세요."
            : refusalReason!;
        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // 계획 4 Task 3. **쓰는 경로다** - 돈이 나가고 물건이 들어간다. 거절은 전부 "아무것도 바뀌지 않았다" 이거나,
    // 바뀐 것이 있으면 그 사실을 사유에 담는다(조용한 반쯤 성공은 없다).
    public ValueTask<string> KitBuy(string url, StmGearKitPurchaseRequest request, MongoId sessionId)
    {
        if (kitPurchases.TryBuy(sessionId, request, out var bought, out var refusalReason) && bought != null)
        {
            return new ValueTask<string>(httpResponse.GetBody(bought));
        }

        var message = string.IsNullOrWhiteSpace(refusalReason)
            ? "장비를 사지 못했습니다. 서버 로그를 확인하세요."
            : refusalReason!;
        return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
    }

    // Failure never reads as success: the client gets an error body with a key, not an empty list.
    private ValueTask<string> Run<T>(string action, Func<T> work)
    {
        try { return new ValueTask<string>(httpResponse.GetBody(work())); }
        catch (Exception ex)
        {
            logger.Warning($"[STM] Profile member '{action}' failed: {ex}");
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError,
                errmsg: ex is InvalidOperationException ? ex.Message : "StmProfileMemberFailed"));
        }
    }
}
