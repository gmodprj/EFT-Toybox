using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Path = System.IO.Path;

namespace pitTeam.Server.Services;

// 프로필 팀원의 장비를 고친다 - 한 요청의 여러 건을 **쓰기 한 번**으로 적용한다. 방향은 둘이고 둘 다
// 한 프로필 안의 이동이다: 입히기(그 팀원 스태시 → 그 팀원 장비 트리)와 벗기기(그 팀원 장비 트리 →
// 그 팀원 자신의 스태시). 어느 쪽도 아이템을 그 프로필 밖으로 내보내지 않고, 플레이어 스태시에서
// 들여오지도 않는다. 설계 §6-4: "편집하면 실물이 된다" - 중간 상태를 두지 않고
// 그 순간 그 런처 프로필에 박힌다. "출처는 본인 스태시뿐" - 집어넣을 수 있는 것은 그 팀원 스태시에
// 실제로 있는 것뿐이고, 플레이어 스태시에서 흘러드는 경로는 없다(공짜 생성 금지).
//
// ---- 왜 여기에 3단 전송 저널(§6-8)이 없는가 ----
// 저널을 기대하고 이 파일을 열었다면 이 단락이 그 답이다. **지우지 말 것.**
// §6-4 가 출처를 "그 팀원 자신의 스태시" 로 좁힌 결과, 이 편집은 **프로필 한 개**를 고친다 - 팀원의
// 스태시와 장비는 같은 하나의 PmcData.Inventory.Items 안에 있고, 쓰기는 SaveProfile 한 번이다.
// §6-8 의 열거("장비 편집, 전리품 배달, 전적 적립, 나중의 스태시 전송")는 PIT 의 "플레이어 스태시 →
// 팀원" 흐름을 전제로 쓰였고, §6-4 가 그 전제를 뒤에 좁혔다. 그래서 그 열거가 이 기능에는 맞지 않는다.
// 기계적으로도 맞지 않는다: StmTransferJournal.Prepare 는 출처와 목적지가 같은 프로필이면 던지고
// (StmTransferJournal.cs:56-57), 복구 판정은 "출처 프로필의 인벤토리에 있나 / 목적지 프로필의
// 인벤토리에 있나" 로 사실을 모으므로(TransferJournalPolicy.DecideFromInventories) 한 프로필 안의
// 이동이면 두 목록이 같은 목록이다 - **성공한 편집마다** "양쪽 다 가짐 → StopAndReport" 로 읽혀 그
// 팀원의 편집이 영구히 막힌다. 저널은 "프로필 두 개의 쓰기 사이에 생기는 창" 을 복구 가능하게 만드는
// 기계이고, 쓰기가 하나면 그 창이 없다. 없는 위험을 위한 기계는 위험을 줄이지 않고 성공을 실패로 읽는다.
// (2026-11-02 Task 6 결정. 되돌리고 싶으면 위 두 사실을 먼저 확인하라.)
//
// 버리지 않고 **지킨** 것 둘:
//   1. §6-8 의 3단계 "확인" - SaveProfile 뒤 **디스크에서 다시 읽어** 대조한다(VerifyFromDisk).
//      어긋나면 조용히 성공하지 않는다: 원래 상태로 되돌려 다시 저장하고, Error 로 남기고, 거부한다.
//   2. 진입 차단 - 미결 전송 항목이 있으면 편집하지 않는다(§6-9). 다른 기능의 **프로필 두 개** 조작이
//      끊긴 채 남아 있으면 그 팀원의 장비를 건드려선 안 되므로 이 관문은 그대로 둔다.
[Injectable(InjectionType.Singleton)]
public class StmGearEditService(
    StmProfileMemberResolver resolver,
    StmProfileMemberStore store,
    StmTransferJournal journal,
    SaveServer saveServer,
    ICloner cloner,
    ItemHelper itemHelper,
    InventoryHelper inventoryHelper,
    JsonUtil jsonUtil,
    FileUtil fileUtil,
    ISptLogger<StmGearEditService> logger)
{
    // SaveServer 가 프로필 파일을 두는 자리. 그 클래스는 private const profileFilepath = "user/profiles/"
    // 를 **상대 경로로** 쓰므로(refs/4.1.0 의 SaveServer.LoadProfileAsync·SaveProfileAsync) 서버의 작업
    // 디렉터리에 기댄다. 이 모드는 그러지 않는다: 이 서버 모드의 다른 모든 경로처럼 AppContext.BaseDirectory
    // (= 서버 실행 파일이 있는 폴더, 실측 E:\Spt\SPT_Runtime)에 고정한다 - FriendlyLanguageService.cs:28·:104,
    // FriendlyServerSettingsService.cs:98·:229, PitFireTeamServerPlugin.cs:107 이 같은 기준점을 쓴다.
    // 상대 경로로 두면 작업 디렉터리에 따라 "확인" 이 늘 실패해 **모든 편집이 되돌려진다** - 안전하긴 하지만
    // 기능이 통째로 죽고, 그걸 인게임에서 발견하게 된다.
    private static string ProfileDirectory => Path.Combine(AppContext.BaseDirectory, "user", "profiles");

    // 미결 전송이 이 팀원을 막고 있을 때의 사유. 저장(TryApply)과 스태시 읽기(TryReadStash)가 같은 문장을 쓴다.
    private const string JournalBlockedReason = "이 팀원의 장비 편집이 막혀 있습니다. 끝나지 않은 아이템 전송 기록이 남아 있습니다.";

    private static string DescribeLookupFailure(ProfileLookupOutcome outcome, string? unavailableReason) => outcome switch
    {
        ProfileLookupOutcome.ReadFailed => "이 팀원이 프로필 팀원인지 확인할 수 없어 장비를 고치지 않습니다. 잠시 뒤 다시 시도하세요.",
        ProfileLookupOutcome.NonMember => "이 팀원은 프로필 팀원이 아니어서 이 경로로 장비를 고칠 수 없습니다.",
        _ => $"이 프로필을 지금 쓸 수 없어 장비를 고치지 않습니다({unavailableReason ?? "Unavailable"}).",
    };

    // 편집기를 열기 전에 그 팀원 **자신의** 스태시를 읽는다(설계 §6-4: 출처는 본인 스태시뿐 - 화면의 출처 칸이 플레이어
    // 스태시일 수 없다). 읽기 전용이다: member.Pmc 는 SaveServer 의 살아 있는 객체이므로 읽기만 하고, 보내는 것은 깊은
    // 복제본이다. 저장(TryApply)과 같은 관문 - 레이드, 미결 전송, 해석, 프로필 손상 표시 - 을 같은 순서로 서고, 열 수 없는데 편집기를
    // 열어 저장에서 거절당하게 하지 않는다. 던지지 않는다(라우트 안에서 불린다). false 일 때 refusalReason 은 비지 않는다.
    public bool TryReadStash(MongoId sessionId, string? accountId, string? requestedProfileId, out StmGearStashResponse? stash, out string? refusalReason)
    {
        stash = null;
        refusalReason = null;
        try
        {
            if (FriendlyModeRequestGate.HasActiveRaid)
            {
                refusalReason = GearEditEntryPolicy.RaidInProgressReason;
                return false;
            }

            if (journal.HasOpenEntry(sessionId, requestedProfileId ?? string.Empty))
            {
                refusalReason = JournalBlockedReason;
                return false;
            }

            var outcome = resolver.Lookup(sessionId, accountId, out var member, out var unavailableReason);
            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                refusalReason = DescribeLookupFailure(outcome, unavailableReason);
                return false;
            }

            // 요청이 말한 프로필과 해석된 프로필이 다르면 보내지 않는다(저장과 같은 약속).
            if (!string.Equals(member.ProfileId, requestedProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 스태시 읽기 거부: 요청의 프로필 '{requestedProfileId}' 과(와) Aid '{accountId}' 로 해석된 프로필 '{member.ProfileId}' 이(가) 다르다.");
                refusalReason = "요청한 프로필과 이 팀원의 프로필이 달라 스태시를 보내지 않습니다.";
                return false;
            }

            // 저장(TryApply)과 같은 관문: SPT 가 쓰지 않을 프로필이면 편집기를 열어 주지 않는다. 열어 주면 저장에서 거절당한다.
            if (saveServer.IsProfileInvalidOrUnloadable(new MongoId(member.ProfileId)))
            {
                logger.Error($"[STM] 스태시 읽기 거부: 프로필 '{member.ProfileId}' 이(가) SPT 에서 손상/적재 불가로 표시돼 있어 저장이 되지 않는다.");
                refusalReason = "이 프로필이 손상으로 표시돼 있어 편집기를 열 수 없습니다.";
                return false;
            }

            var inventory = member.Pmc.Inventory;
            var stashRootId = inventory?.Stash?.ToString();
            var equipmentRootId = inventory?.Equipment?.ToString();
            if (inventory?.Items == null || string.IsNullOrWhiteSpace(stashRootId) || string.IsNullOrWhiteSpace(equipmentRootId)
                || string.Equals(stashRootId, equipmentRootId, StringComparison.Ordinal))
            {
                refusalReason = "이 프로필의 인벤토리를 읽을 수 없어 스태시를 보내지 않습니다.";
                return false;
            }

            // 스태시 트리의 판정은 CollectSubtreeIds 하나다(저장의 판정과 같은 함수). 장비 트리는 싣지 않는다.
            var stashIds = WornGearLossPolicy.CollectSubtreeIds(ToWornGearItems(inventory.Items), new[] { stashRootId! });
            var stashItems = inventory.Items
                .Where(i => i?.Id != null && stashIds.Contains(i.Id.ToString()))
                .ToList();
            // 루트가 맨 앞이어야 화면이 그것을 스태시 루트로 읽는다. 루트가 목록에 없으면 보낼 수 없다.
            var root = stashItems.FirstOrDefault(i => string.Equals(i.Id.ToString(), stashRootId, StringComparison.Ordinal));
            if (root == null)
            {
                refusalReason = "이 프로필의 스태시 루트를 아이템 목록에서 찾지 못해 스태시를 보내지 않습니다.";
                return false;
            }

            stashItems.Remove(root);
            stashItems.Insert(0, root);

            stash = new StmGearStashResponse
            {
                StashRootId = stashRootId!,
                EquipmentRootId = equipmentRootId!,
                Items = cloner.Clone(stashItems) ?? throw new InvalidOperationException("StmGearStashCloneFailed"),
            };
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 스태시 읽기가 실패했다(프로필 '{requestedProfileId}'): {ex}");
            stash = null;
            refusalReason = "스태시를 읽는 중 오류가 났습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    // true = 적용하고 디스크에서 확인했다. false = 아무것도 바뀌지 않았거나(거부), 바뀐 것을 되돌렸다
    // (확인 실패). false 일 때 rejectionReason 은 절대 비지 않는다 - 호출자가 "사유 없음 = 성공" 으로
    // 읽는 틈을 두지 않는다. 던지지 않는다(라우트 안에서 불린다).
    //
    // 명세와 다른 시그니처(의도적): accountId 를 받는다. 등록·카탈로그를 읽는 곳은 해석기의 Lookup
    // 하나이고 그것이 Aid 로 찾는다(수정 5차의 "읽기 한 곳·분류 한 곳" 구조). ProfileId 로 찾으려면
    // 두 번째 읽기 경로가 생긴다 - 그래서 Aid 로 해석하고, 요청의 ProfileId 는 **믿지 않고 대조한다.**
    // 여러 건을 받아 **한 번에** 적용한다: 판정 전부 → 한 클론에 전부 반영 → SaveProfile 한 번 →
    // 디스크 재독 한 번. 쪼개 부르면 중간에 하나가 실패했을 때 앞의 것은 이미 적용돼 "반쯤 입은" 상태가
    // 남는다. 최종 집합을 **서버가** 만드는 것이 핵심이다 - 클라이언트가 최종 인벤토리를 제출하는 모양은
    // 계약 2 의 보존 검사를 만들게 한 그 구멍이다. 각 건은 자기 혼자 Validate 를 통과해야 하고, 하나라도
    // 거절되면 전체를 거절하며 **아무것도 쓰지 않는다**(ProfileGearEditPolicy.ValidateBatch).
    // **2026-11-03 이후 아무도 부르지 않는다.** 저장은 최종 상태 제출(StmInventoryApplyService)로 바뀌었고
    // 이 경로의 라우트·콜백·와이어 모델은 지웠다. 메서드와 그 아래 헬퍼들(슬롯 어휘·격자 배치·디스크
    // 재독)은 **아직 남겨 둔다**: 격자 배치는 꾸러미 구매가 쓰고(TryPlaceInContainer), 슬롯 어휘는 최종
    // 상태의 **슬롯 적법성**을 서버가 보게 될 때 그대로 쓸 자리다(인계 문서의 열린 항목). 그때까지는
    // 죽은 코드이므로, 다시 살릴 생각이 없다면 이 메서드와 BuildSlotFacts·VerifyFromDisk·TryRestore 를
    // 함께 지워야 한다 - 하나만 지우면 컴파일은 되고 의미가 비어 버린다.
    public bool TryApply(MongoId sessionId, string? accountId, IReadOnlyList<GearEditRequest>? requests, out string? rejectionReason)
    {
        rejectionReason = null;
        // 살아 있는 인벤토리를 바꾼 뒤에만 채워진다. catch-all 이 이것으로 되돌릴지를 판단한다
        // (원래 아이템 목록은 originalItems).
        BotBaseInventory? liveInventory = null;
        MongoId? liveProfileId = null;
        List<Item>? originalItems = null;
        try
        {
            if (requests == null || requests.Count == 0 || requests[0] == null)
            {
                rejectionReason = "고칠 아이템이 없어 편집하지 않습니다.";
                return false;
            }

            var requestedProfileId = requests[0].ProfileId;

            // 0. 레이드 중에는 고치지 않는다. 모드 전환이 같은 이유로 같은 관문을 쓴다
            //    (FriendlyTeammateCallbacks.SetServerSettings 의 SettingsUnavailableDuringRaid).
            //    레이드 중에 스태시 아이템을 착용 트리로 옮기면, 그 팀원이 죽었을 때 착용 장비 삭제
            //    경로(WornGearLossPolicy)가 방금 옮긴 그 아이템을 지운다 - 플레이어는 편집한 줄만 알고
            //    아이템은 사라진다. 그리고 레이드 스폰은 시작 시점의 복제본 위에서 돌므로 편집이 이번
            //    레이드에 반영되지도 않는다.
            if (FriendlyModeRequestGate.HasActiveRaid)
            {
                rejectionReason = GearEditEntryPolicy.RaidInProgressReason;
                return false;
            }

            // 1. 미결 전송이 이 팀원을 막고 있는가(§6-9). 저널 문서를 못 읽어도 true 다 - 편집 한 번을
            //    거절하는 비용이 깨진 저널 위에서 허락하는 비용(아이템)보다 싸다.
            if (journal.HasOpenEntry(sessionId, requestedProfileId ?? string.Empty))
            {
                rejectionReason = JournalBlockedReason;
                return false;
            }

            // 2. 해석은 한 번. 분류도 한 번(ProfileViewPolicy.ClassifyLookup). 여기서 등록·카탈로그를
            //    다시 읽지 않는다.
            var outcome = resolver.Lookup(sessionId, accountId, out var member, out var unavailableReason);
            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                rejectionReason = DescribeLookupFailure(outcome, unavailableReason);
                return false;
            }

            // 요청이 말한 프로필과 해석된 프로필이 다르면 하지 않는다. 요청의 ProfileId 를 믿고 쓰면
            // 화면에서 본 팀원이 아닌 **다른 런처 프로필**의 장비를 고치는 길이 열린다.
            if (!string.Equals(member.ProfileId, requestedProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 장비 편집 거부: 요청의 프로필 '{requestedProfileId}' 과(와) Aid '{accountId}' 로 해석된 프로필 '{member.ProfileId}' 이(가) 다르다.");
                rejectionReason = "요청한 프로필과 이 팀원의 프로필이 달라 장비를 고치지 않습니다.";
                return false;
            }

            var profileId = new MongoId(member.ProfileId);

            // 3. SPT 가 쓰지 않을 프로필이면 **고치기 전에** 거부한다. SaveProfileAsync 는 이 표시가 있으면
            //    첫 줄에서 돌아가며 아무것도 쓰지 않고 던지지도 않는다(StmRaidOutcomeService 가 같은 이유로
            //    같은 검사를 한다). 그대로 두면 메모리만 바뀌고 디스크는 그대로인 유령 재산이 된다.
            if (saveServer.IsProfileInvalidOrUnloadable(profileId))
            {
                logger.Error($"[STM] 장비 편집 거부: 프로필 '{member.ProfileId}' 이(가) SPT 에서 손상/적재 불가로 표시돼 있어 저장이 되지 않는다.");
                rejectionReason = "이 프로필이 손상으로 표시돼 있어 저장할 수 없습니다. 장비를 고치지 않았습니다.";
                return false;
            }

            // 4. member.Pmc 는 SaveServer 가 들고 있는 **살아 있는 객체**다. 읽기만 하고, 고치는 것은
            //    깊은 복제본뿐이다(cloner.Clone = FastCloner).
            var live = member.Pmc;
            var inventory = live.Inventory;
            if (inventory?.Items == null)
            {
                rejectionReason = "이 프로필의 인벤토리를 읽을 수 없어 장비를 고치지 않습니다.";
                return false;
            }

            // 루트는 추측하지 않는다. 비어 있으면 거부다 - PIT 의 폴백(첫 hideout 아이템 찾기)을 쓰지 않는
            // 이유는 이 경로가 **쓰기** 경로이기 때문이다. 틀린 루트로 쓰면 아이템이 엉뚱한 트리로 간다.
            var stashRootId = inventory.Stash?.ToString();
            var equipmentRootId = inventory.Equipment?.ToString();

            var original = cloner.Clone(inventory.Items) ?? throw new InvalidOperationException("StmGearEditCloneFailed");
            var edited = cloner.Clone(inventory.Items) ?? throw new InvalidOperationException("StmGearEditCloneFailed");

            // 5. 판정 **전부 먼저**. 하나라도 거절되면 아무것도 쓰지 않는다. 순서대로 누적해 보므로
            //    "탄창을 총에 꽂고 그 탄창에 탄을 넣는" 연쇄가 통과한다(인계 문서 열린 항목 4).
            var slotFacts = BuildSlotFacts(edited);
            var verdict = ProfileGearEditPolicy.ValidateBatch(
                requests, ToWornGearItems(edited), stashRootId, equipmentRootId, slotFacts);
            if (!verdict.Allowed)
            {
                // 사유는 실패한 그 건의 사유다. 플레이어가 어느 아이템이 왜 막혔는지 알 수 있다.
                rejectionReason = verdict.Reason;
                return false;
            }

            // 판정이 끝난 뒤 한 클론에 전부 반영한다. 정책이 본 것과 같은 순서다.
            var applied = new List<(string ItemId, string ParentId, string SlotId, bool ToStashTree, bool ExpectsCoordinates)>(requests.Count);
            foreach (var request in requests)
            {
                var itemId = request.ItemId.Trim();
                var slotId = request.TargetSlotId.Trim();
                var parentId = string.IsNullOrWhiteSpace(request.TargetParentId) ? equipmentRootId!.Trim() : request.TargetParentId!.Trim();
                var target = edited.FirstOrDefault(i => i?.Id != null && string.Equals(i.Id.ToString(), itemId, StringComparison.Ordinal));
                if (target == null)
                {
                    // 정책이 통과시켰으면 있어야 한다. 그래도 믿고 쓰지 않는다 - 클론은 버리고 아무것도 쓰지 않았다.
                    rejectionReason = $"옮길 아이템 '{itemId}' 을(를) 프로필에서 찾지 못해 전체를 취소했습니다.";
                    return false;
                }

                var toStash = string.Equals(parentId, stashRootId!.Trim(), StringComparison.Ordinal);
                // 용기의 칸도 격자다 - 스태시 격자와 똑같이 SPT 가 빈 자리를 찾아 좌표까지 쓴다.
                var toGrid = toStash || ProfileGearEditPolicy.IsGridDestination(slotId, parentId, slotFacts);
                // 탄을 담는 자리인가. 쓰는 Location 과 **디스크 재독의 기대**가 둘 다 이 값으로 갈린다 -
                // 한쪽만 알면 맞게 저장된 편집이 "기대와 다르다" 로 되돌려진다(실측으로 겪었다).
                var toAmmoSlot = false;
                // 목적지가 스태시 트리인가(장비 트리인가). 재독 대조의 트리 기대가 이 값으로 갈린다.
                var toStashTree = toStash || IsInSubtree(edited, parentId, stashRootId!.Trim());
                if (toGrid)
                {
                    // 벗기기. 격자에는 좌표가 필요하고 좌표를 지어내지 않는다 - SPT 자신의
                    // PlaceItemInContainer 가 빈 자리를 찾아 ParentId·SlotId·Location 을 함께 쓴다
                    // (refs/4.1.0; RestrictedMagazineRecovery.TryPlace 가 같은 두 호출을 쓴다).
                    // 자리가 없거나 격자를 읽지 못하면 거부한다 - 여기까지도 아무것도 쓰지 않았다.
                    if (!TryPlaceInContainer(edited, target, parentId, slotId, out var placementFailure))
                    {
                        logger.Error($"[STM] 장비 편집: 아이템 '{itemId}' 을(를) 프로필 '{member.ProfileId}' 의 '{parentId}'/'{slotId}' 칸에 놓지 못해 전체를 취소한다: {placementFailure}");
                        rejectionReason = $"아이템 '{itemId}' 을(를) 놓을 자리를 찾지 못해 전체를 취소했습니다.";
                        return false;
                    }
                }
                else
                {
                    // 요청의 철자를 프로필에 쓰지 않는다. 어휘 검사는 대소문자를 무시하고 받아 주지만
                    // SPT·EFT 는 슬롯 이름을 대소문자까지 그대로 보므로 "headwear" 로 저장된 헬멧은
                    // 착용으로 인식되지 않는다 - 검사를 통과한 요청이 깨진 프로필을 쓰는 길이었다.
                    // 정식 이름은 부모 아이템 템플릿이 정의한 슬롯 이름이다(데이터베이스 자신의 철자).
                    var canonicalSlot = ProfileGearEditPolicy.CanonicalSlotName(slotId, parentId, equipmentRootId, stashRootId, slotFacts);
                    if (canonicalSlot == null)
                    {
                        logger.Error($"[STM] 장비 편집: 슬롯 '{slotId}'(부모 '{parentId}')의 정식 이름을 찾지 못해 전체를 취소한다 - 철자를 지어내지 않는다.");
                        rejectionReason = $"슬롯 '{slotId}' 의 정식 이름을 확인하지 못해 전체를 취소했습니다.";
                        return false;
                    }

                    // 부모 id 도 요청의 것을 믿지 않고 프로필에서 찾은 그 아이템의 id 를 쓴다. 판정이
                    // Ordinal 로 소속을 확인했으므로 실제로는 같은 문자열이지만, 쓰는 값의 출처를
                    // 프로필 한 곳으로 모아 둔다(장비 루트는 Inventory.Equipment 에서 온 값이다).
                    var parentItem = edited.FirstOrDefault(i => i?.Id != null && string.Equals(i.Id.ToString(), parentId, StringComparison.Ordinal));
                    target.ParentId = parentItem?.Id.ToString() ?? parentId;
                    target.SlotId = canonicalSlot;
                    // 슬롯에 들어간 아이템은 격자 좌표를 갖지 않는다. 스태시 칸에 있던 Location 을 그대로 두면
                    // 슬롯 안에 좌표가 있는 모양이 되고, PIT 의 검증도 그 모양을 수상한 것으로 본다.
                    //
                    // 예외는 탄을 담는 자리다: 거기 들어간 더미는 좌표가 아니라 **색인**을 갖는다
                    // (실측: 프로필 JSON 의 cartridges 항목은 location 이 정수 0 이다. PIT 도
                    // ItemHelper.CreateCartridges(..., 0) 로 같은 값을 쓴다). 색인을 지어내는 것이
                    // 아니다 - 정책 (a) 가 그 자리가 **비어 있음**을 확인했으므로 첫 자리는 0 이다.
                    toAmmoSlot = ProfileGearEditPolicy.SlotAmmoCapacity(canonicalSlot, parentId, slotFacts).HasValue;
                    target.Location = toAmmoSlot ? 0 : null;
                }

                // 기대값은 **실제로 쓴 값**에서 받아 적는다. 요청의 철자를 그대로 쓰면 안 된다:
                // 어휘 검사는 대소문자를 무시하고 받아 주지만(요청이 "Hideout" 이어도 통과) 스태시
                // 배치는 SPT 가 템플릿의 격자 이름("hideout")을 쓰고, 디스크 재독 대조는 Ordinal 이다 -
                // 그러면 맞게 저장된 편집이 "기대와 다르다" 로 되돌려진다(안전하지만 거짓 실패다).
                applied.Add((itemId, target.ParentId ?? parentId, target.SlotId ?? slotId, toStashTree, toGrid || toAmmoSlot));
            }

            // 6. 유일한 쓰기: SaveServer 가 들고 있는 객체를 고치고 그 자신의 저장을 부른다. 프로필
            //    JSON 을 직접 쓰지 않는다(§6-8: SPT 가 로그인하지 않은 프로필도 메모리에 들고 주기적으로
            //    저장하므로 직접 쓰면 조용히 덮어써진다).
            // 이 순간부터 **살아 있는 객체가 바뀌어 있다.** 아래에서 무엇이 터져도 되돌려야 한다:
            // SPT 는 로그인하지 않은 프로필도 주기적으로 저장하므로, 되돌리지 않으면 플레이어에게
            // "아무 일도 없었다" 고 말한 편집이 나중에 조용히 디스크로 내려간다. 그리고 HandEdited 가
            // 꺼진 채라 다음 레이드의 임시 장비가 그것을 덮어 가린다.
            liveInventory = inventory;
            liveProfileId = profileId;
            originalItems = original;
            inventory.Items = edited;
            var elapsedMs = saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();

            // 7. 확인(§6-8 3단계): 디스크에서 다시 읽어 대조한다. 메모리를 다시 보는 것은 확인이 아니다 -
            //    우리가 방금 고친 그 객체이기 때문이다.
            if (!VerifyFromDisk(member.ProfileId, applied, stashRootId!, equipmentRootId!, original, out var mismatch))
            {
                // 되돌린다: 원래 아이템 목록으로 다시 쓰고 다시 저장한다. 그런 다음 **실패를 알린다.**
                // 조용히 성공하지 않는다. 묶음 전체가 되돌아간다 - 반쯤 입은 상태를 남기지 않는다.
                logger.Error($"[STM] 장비 편집 확인 실패(프로필 '{member.ProfileId}', 아이템 {applied.Count}건): {mismatch} 원래 상태로 되돌린다.");
                var restored = TryRestore(profileId, inventory, original);
                rejectionReason = restored
                    ? "저장한 내용을 디스크에서 다시 읽어 확인하지 못해 장비를 원래대로 되돌렸습니다. 서버 로그를 확인하세요."
                    : "저장한 내용을 확인하지 못했고 되돌리기도 실패했습니다. 이 프로필의 장비와 스태시를 직접 확인하세요.";
                return false;
            }

            logger.Info($"[STM] 장비 편집: 프로필 '{member.ProfileId}'(aid {member.Aid})의 아이템 {applied.Count}건을 한 번에 저장·확인했다({elapsedMs}ms) - {string.Join(", ", applied.Select(a => $"'{a.ItemId}' → '{a.ParentId}'/{a.SlotId}"))}");

            // 8. 이제 이 팀원은 임시 장비 대상이 아니다(§6-4, 한 방향). 장비는 이미 실물이 됐으므로 이
            //    사실을 못 적었다면 숨기지 않고 실패로 알린다 - 그대로 두면 다음 레이드에서 임시 장비가
            //    이 편집을 덮어쓴다.
            //    표시를 쓰다 **던지는** 경우도 같은 실패다(등록 문서를 못 읽거나 쓰지 못하는 상태).
            //    여기서는 되돌리지 않는다: 장비 변경은 저장되고 디스크에서 확인까지 끝났고, 되돌리면
            //    확인된 정상 상태를 다시 뒤집는 쓰기가 하나 더 생긴다. 플레이어에게는 다시 편집하라고 말한다.
            bool flagged;
            try
            {
                flagged = store.Update(sessionId, member.ProfileId, new StmProfileMemberChange(AutoJoin: null, HandEdited: true));
            }
            catch (Exception ex)
            {
                logger.Error($"[STM] 장비 편집: 프로필 '{member.ProfileId}' 의 장비는 바뀌고 확인됐지만 '손으로 고침' 표시를 쓰다 실패했다: {ex}");
                flagged = false;
            }

            if (!flagged)
            {
                logger.Error($"[STM] 장비 편집: 프로필 '{member.ProfileId}' 의 장비는 바뀌고 저장됐지만 '손으로 고침' 표시를 쓰지 못했다. 다음 레이드에서 임시 장비가 이 편집을 덮어쓸 수 있다.");
                rejectionReason = "장비는 바뀌었지만 '손으로 고침' 표시를 저장하지 못했습니다. 다시 한 번 편집해 주세요.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            var failedProfileId = requests != null && requests.Count > 0 ? requests[0]?.ProfileId : null;
            logger.Error($"[STM] 장비 편집이 실패했다(프로필 '{failedProfileId}', {requests?.Count ?? 0}건): {ex}");

            // 살아 있는 객체를 이미 바꿔 놓고 터졌으면 되돌린다. 되돌리지 않으면 SPT 의 주기적 저장이
            // "일어나지 않았다" 고 말한 편집을 나중에 디스크에 내려보낸다(HandEdited 도 꺼진 채라
            // 다음 레이드의 임시 장비가 그것을 가린다). 되돌리기도 실패하면 그렇다고 말한다.
            if (liveInventory != null && originalItems != null && liveProfileId != null)
            {
                var restored = TryRestore(liveProfileId.Value, liveInventory, originalItems);
                rejectionReason = restored
                    ? "장비를 고치는 중 오류가 나서 원래대로 되돌렸습니다. 서버 로그를 확인하세요."
                    : "장비를 고치는 중 오류가 났고 되돌리기도 실패했습니다. 이 프로필의 장비와 스태시를 직접 확인하세요.";
                return false;
            }

            rejectionReason = "장비를 고치는 중 오류가 났습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    // §6-8 의 "확인". 프로필 파일을 **디스크에서 다시 읽어** 세 가지를 본다:
    //   (a) 옮긴 아이템이 새 부모·새 슬롯에 있고 격자 좌표가 없다;
    //   (b) 그 아이템이 장비 트리 안이고 스태시 트리 밖이다(부모만 보는 것으로는 부족하다 - 부모 사슬이
    //       엉뚱하게 이어졌는지는 트리 판정만 안다. 판정은 CollectSubtreeIds 하나다);
    //   (c) 편집 전에 있던 아이템 id 가 하나도 사라지지 않았고 새 id 가 끼어들지도 않았다
    //       (보존 검사를 재사용한다 - 같은 불변식이다).
    // 못 읽거나 한 가지라도 어긋나면 false 다(닫힌 쪽). mismatch 는 로그에 남는 한 줄이다.
    private bool VerifyFromDisk(
        string profileId,
        IReadOnlyList<(string ItemId, string ParentId, string SlotId, bool ToStashTree, bool ExpectsCoordinates)> applied,
        string stashRootId,
        string equipmentRootId,
        List<Item> beforeEdit,
        out string mismatch)
    {
        mismatch = string.Empty;
        var filePath = Path.Combine(ProfileDirectory, $"{profileId}.json");
        if (!fileUtil.FileExists(filePath))
        {
            mismatch = $"프로필 파일 '{filePath}' 이(가) 없다.";
            return false;
        }

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

        // 트리 판정은 묶음 전체에 대해 한 번만 계산한다. 판정은 CollectSubtreeIds 하나다.
        var diskTriples = ToWornGearItems(onDisk);
        var diskEquipmentIds = WornGearLossPolicy.CollectSubtreeIds(diskTriples, new[] { equipmentRootId });
        var diskStashIds = WornGearLossPolicy.CollectSubtreeIds(diskTriples, new[] { stashRootId });

        foreach (var (itemId, expectedParentId, expectedSlotId, toStashTree, expectsCoordinates) in applied)
        {
            var moved = onDisk.FirstOrDefault(i => i?.Id != null && string.Equals(i.Id.ToString(), itemId, StringComparison.Ordinal));
            if (moved == null)
            {
                mismatch = $"옮긴 아이템 '{itemId}' 이(가) 디스크의 인벤토리에 없다.";
                return false;
            }

            // 좌표의 기대는 세 갈래다. 스태시 격자에 놓인 것은 좌표가 **있어야** 하고(없으면 그 아이템이
            // 어느 칸에 있는지 아무도 모른다), 탄을 담는 자리에 들어간 것도 **색인**을 가지며, 그 밖의
            // 슬롯에 들어간 것은 좌표가 **없어야** 한다.
            var expectsLocation = expectsCoordinates;
            var locationWrong = expectsLocation ? moved.Location == null : moved.Location != null;
            if (!string.Equals(moved.ParentId, expectedParentId, StringComparison.Ordinal)
                || !string.Equals(moved.SlotId, expectedSlotId, StringComparison.Ordinal)
                || locationWrong)
            {
                mismatch = $"디스크의 아이템 '{itemId}' 이(가) 기대와 다르다(부모 '{moved.ParentId}' 기대 '{expectedParentId}', 슬롯 '{moved.SlotId}' 기대 '{expectedSlotId}', 좌표 {(moved.Location == null ? "없음" : "있음")}, 기대 {(expectsLocation ? "있음" : "없음")}).";
                return false;
            }

            // 트리 기대도 뒤집힌다.
            var mustBeIn = toStashTree ? diskStashIds : diskEquipmentIds;
            var mustBeOutOf = toStashTree ? diskEquipmentIds : diskStashIds;
            if (!mustBeIn.Contains(itemId))
            {
                mismatch = $"디스크의 아이템 '{itemId}' 이(가) {(toStashTree ? "스태시" : "장비")} 트리 안에 없다.";
                return false;
            }

            if (mustBeOutOf.Contains(itemId))
            {
                mismatch = $"디스크의 아이템 '{itemId}' 이(가) 아직 {(toStashTree ? "장비" : "스태시")} 트리 안에 있다.";
                return false;
            }
        }

        var beforeIds = beforeEdit.Where(i => i?.Id != null).Select(i => i.Id.ToString()).ToList();
        var diskIds = onDisk.Where(i => i?.Id != null).Select(i => i.Id.ToString()).ToList();
        var unchanged = ProfileGearEditPolicy.CheckItemSetUnchanged(beforeIds, diskIds);
        if (!unchanged.Conserved)
        {
            mismatch = unchanged.Reason;
            return false;
        }

        return true;
    }

    // 확인이 어긋났을 때 원래 아이템 목록으로 되돌려 다시 저장한다. 성공하면 true. 실패해도 던지지
    // 않는다 - 호출자가 "되돌리기도 실패" 를 플레이어에게 말해야 한다.
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
            logger.Error($"[STM] 장비 편집 되돌리기가 실패했다(프로필 '{profileId}'): {ex}");
            return false;
        }
    }

    // 이 아이템이 그 뿌리의 하위 트리에 있는가. 부모를 따라 올라가며 본다 - 고리가 있어도 끝나게
    // 걸음 수를 끊는다(손상된 프로필에서 돌지 않는다).
    private static bool IsInSubtree(List<Item> items, string itemId, string rootId)
    {
        var current = items.FirstOrDefault(i => i?.Id != null && string.Equals(i.Id.ToString(), itemId, StringComparison.Ordinal));
        for (var steps = 0; current != null && steps < 64; steps++)
        {
            var currentId = current.Id.ToString();
            if (string.Equals(currentId, rootId, StringComparison.Ordinal)) return true;
            if (string.IsNullOrWhiteSpace(current.ParentId)) return false;
            current = items.FirstOrDefault(i => i?.Id != null && string.Equals(i.Id.ToString(), current.ParentId, StringComparison.Ordinal));
        }

        return false;
    }

    // 벗긴 아이템을 그 프로필 **자신의** 스태시 격자에 놓는다. 좌표를 지어내지 않는다 - 빈 자리를 찾고
    // ParentId·SlotId·Location 을 쓰는 것은 SPT 자신의 InventoryHelper.PlaceItemInContainer 다
    // (refs/4.1.0: itemWithChildren[0] 의 크기를 재서 FindSlotForItem 으로 자리를 찾고, 성공했을 때만
    // 세 필드를 쓴다). 격자 지도를 만드는 두 호출의 조합은 RestrictedMagazineRecovery.TryPlace 와 같다.
    // 던지지 않는다: 실패는 false + 사람이 읽을 사유이고, 호출자가 전체를 취소한다(아직 아무것도 안 썼다).
    // 꾸러미 구매(계획 4 Task 3)도 헌 장비를 그 팀원 스태시로 돌릴 때 이 자리 찾기를 쓴다.
    // 격자 자리를 찾는 판정이 두 곳에 생기면 한쪽만 고쳐져 아이템이 겹쳐 놓인다.
    internal bool TryPlaceInContainer(List<Item> items, Item target, string containerId, string gridName, out string failure)
    {
        failure = string.Empty;
        try
        {
            var container = items.FirstOrDefault(i => i?.Id != null && string.Equals(i.Id.ToString(), containerId, StringComparison.Ordinal));
            if (container == null)
            {
                failure = $"용기 '{containerId}' 를 아이템 목록에서 찾지 못했다.";
                return false;
            }

            var found = itemHelper.GetItem(container.Template);
            var grid = (found.Key ? found.Value?.Properties?.Grids : null)?
                .FirstOrDefault(g => g?.Name != null && string.Equals(g.Name, gridName, StringComparison.OrdinalIgnoreCase));
            if (grid?.Properties?.CellsH is not > 0 || grid.Properties.CellsV is not > 0)
            {
                failure = $"템플릿 '{container.Template}' 에서 '{gridName}' 격자의 크기를 읽지 못했다.";
                return false;
            }

            // 이 격자에 이미 있는 것만 지도에 넣는다(GetContainerMap 은 리그처럼 격자가 여러 개인 용기의
            // 모든 격자를 합쳐 버린다 - RestrictedMagazineRecovery 가 같은 이유로 같은 필터를 쓴다).
            // 벗길 아이템 자신은 아직 장비 트리에 있으므로 지도에 들어가지 않는다.
            var gridItems = items.Where(i => i?.ParentId == null
                || !string.Equals(i.ParentId, containerId, StringComparison.Ordinal)
                || string.Equals(i.SlotId, grid.Name, StringComparison.OrdinalIgnoreCase));
            var map = inventoryHelper.GetContainerMap(grid.Properties.CellsH!.Value, grid.Properties.CellsV!.Value, gridItems, container.Id);

            // 크기 계산은 하위 아이템(접히는 개조·부착물)까지 본다. 트리 판정은 CollectSubtreeIds 하나다.
            var subtreeIds = WornGearLossPolicy.CollectSubtreeIds(ToWornGearItems(items), new[] { target.Id.ToString() });
            var tree = items.Where(i => i?.Id != null && subtreeIds.Contains(i.Id.ToString())).ToList();
            // PlaceItemInContainer 는 itemWithChildren[0] 을 크기 계산의 주체로 쓴다 - 루트가 먼저여야 한다.
            tree.Remove(target);
            tree.Insert(0, target);

            var placement = inventoryHelper.PlaceItemInContainer(map, tree, containerId, grid.Name!);
            if (placement?.Success != true)
            {
                failure = $"'{grid.Name}' 격자에 빈 자리가 없다.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            failure = $"스태시 자리 찾기가 실패했다: {ex.Message}";
            return false;
        }
    }

    // 아이템 데이터베이스의 사실을 정책이 쓰는 모양으로 옮긴다. 데이터베이스는 서버에서 닿는다 -
    // ItemHelper.GetItem(tpl) 이 (찾았나, 템플릿) 을 주고, 템플릿의 Properties.Slots 가 그 아이템이
    // **정의한** 슬롯과 각 슬롯의 필터(Props.Filters[].Filter = 허용 템플릿 id 집합)를 들고 있다.
    // 하나라도 읽히지 않으면 그 항목을 넣지 않는다 - 정책이 "모름" 을 거부로 읽으므로 빈 칸이 곧 거부다.
    // 던지지 않는다: 데이터베이스 읽기가 터지면 빈 사실을 돌려 정책이 전부 거부하게 한다(닫힌 쪽).
    private GearSlotFacts BuildSlotFacts(IEnumerable<Item> items)
    {
        var templateIdByItemId = new Dictionary<string, string>(StringComparer.Ordinal);
        var slotsByTemplateId = new Dictionary<string, IReadOnlyList<GearSlotDefinition>>(StringComparer.Ordinal);
        var parentByTemplateId = new Dictionary<string, string>(StringComparer.Ordinal);
        // 더미 수량. 탄을 담는 자리의 용량 검사에만 쓰인다(PIT 의 GetItemStackCount 와 같은 식이다).
        var stackCountByItemId = new Dictionary<string, int>(StringComparer.Ordinal);
        // 용기의 칸(가방·리그·주머니·보안 컨테이너). 슬롯과 같은 길로 싣는다 - 둘 다 그 템플릿이 정의한다.
        var gridsByTemplateId = new Dictionary<string, IReadOnlyList<GearGridDefinition>>(StringComparer.Ordinal);
        try
        {
            foreach (var item in items)
            {
                if (item?.Id == null || item.Template.IsEmpty) continue;
                var itemId = item.Id.ToString();
                var templateId = item.Template.ToString();
                templateIdByItemId[itemId] = templateId;
                stackCountByItemId[itemId] = Math.Max(1, (int)Math.Ceiling(item.Upd?.StackObjectsCount ?? 1d));
                // 조상 사슬. 슬롯 필터가 아이템 tpl 대신 **범주 노드**를 담기 때문에 필요하다
                // (ProfileGearEditPolicy.FilterAccepts 의 실측 주석). 노드도 items.json 의 항목이라
                // 같은 GetItem 으로 읽힌다. 32칸으로 끊는다 - 손상된 사슬에서 돌지 않게.
                CollectTemplateAncestry(item.Template, parentByTemplateId);
                if (slotsByTemplateId.ContainsKey(templateId)) continue;

                var found = itemHelper.GetItem(item.Template);
                if (!found.Key || found.Value == null) continue;

                var definitions = new List<GearSlotDefinition>();
                foreach (var slot in found.Value.Properties?.Slots ?? [])
                {
                    if (slot?.Name == null) continue;
                    definitions.Add(new GearSlotDefinition(slot.Name, AllowedTemplatesOf(slot)));
                }

                // 탄을 담는 자리는 **Slots 에 없다**(2026-11-03 실측: 바닐라 탄창
                // mag_stanag_colt_ar15_std_556x45_30 은 Slots 가 비어 있고 Cartridges 에 'cartridges'
                // 하나를 둔다). 약실(patron_in_weapon)도 같은 이유로 Chambers 에 있다. 이 둘을 읽지
                // 않으면 탄은 **어떤 탄창에도** 넣을 수 없다 - 인계 문서 열린 항목 4 가 그 결함이었다.
                // 격자 좌표가 아니라 색인으로 담으므로 용량(_max_count)을 함께 싣는다.
                // 탄창: 용량이 Cartridges 항목의 _max_count 에 있다(STANAG 30발 = 30, GP-5 = 33).
                // 읽지 못하면 0 으로 싣는다 - 정책이 0 을 "몇 발인지 모른다" 로 읽어 거부한다. 자리를
                // 빼 버리면 "그런 슬롯이 없다" 로 거부돼 사유가 사실과 달라진다.
                foreach (var magazineSlot in found.Value.Properties?.Cartridges ?? [])
                {
                    if (magazineSlot?.Name == null) continue;
                    definitions.Add(new GearSlotDefinition(
                        magazineSlot.Name,
                        AllowedTemplatesOf(magazineSlot),
                        (int?)magazineSlot.MaxCount ?? 0));
                }

                // 약실: Chambers 항목에는 **개수 필드가 아예 없다**(2026-11-03 실측: M4A1 의
                // patron_in_weapon 은 filters 하나만 들고 있다). 그래서 1 로 싣는다 - 이름에서 뜻을
                // 지어낸 것이 아니라 (ㄱ) 데이터베이스가 개수를 말하지 않고 (ㄴ) 이 PC 의 프로필 여섯 개에
                // 있는 약실 아이템 15개가 전부 수량 1 이고 (ㄷ) 1 이 닫힌 쪽이기 때문이다. 더 큰 더미를
                // 약실에 넣으려는 편집은 이 값 때문에 거부된다.
                foreach (var chamberSlot in found.Value.Properties?.Chambers ?? [])
                {
                    if (chamberSlot?.Name == null) continue;
                    definitions.Add(new GearSlotDefinition(chamberSlot.Name, AllowedTemplatesOf(chamberSlot), 1));
                }

                // 격자. 허용 쪽은 보통 뿌리 노드 하나이고(조상 사슬로 맞춘다), 제외 쪽이 종류를 걷어낸다
                // (가방 안의 가방 등). 둘 다 그대로 옮긴다 - 뜻을 지어내지 않는다.
                var gridDefinitions = new List<GearGridDefinition>();
                foreach (var grid in found.Value.Properties?.Grids ?? [])
                {
                    if (grid?.Name == null) continue;
                    var allowedInGrid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var excludedFromGrid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var filter in grid.Properties?.Filters ?? [])
                    {
                        foreach (var tpl in filter?.Filter ?? [])
                        {
                            if (!tpl.IsEmpty) allowedInGrid.Add(tpl.ToString());
                        }

                        foreach (var tpl in filter?.ExcludedFilter ?? [])
                        {
                            if (!tpl.IsEmpty) excludedFromGrid.Add(tpl.ToString());
                        }
                    }

                    gridDefinitions.Add(new GearGridDefinition(grid.Name, allowedInGrid, excludedFromGrid));
                }

                gridsByTemplateId[templateId] = gridDefinitions;
                slotsByTemplateId[templateId] = definitions;
            }
        }
        catch (Exception ex)
        {
            // 반쯤 채운 사실로 판정하면 어떤 것은 통과하고 어떤 것은 거부되는 설명할 수 없는 결과가 된다.
            logger.Error($"[STM] 장비 편집: 아이템 데이터베이스를 읽지 못해 넣을 자리 검사를 할 수 없다(전부 거부된다): {ex}");
            return new GearSlotFacts(
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, IReadOnlyList<GearSlotDefinition>>(StringComparer.Ordinal),
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, int>(StringComparer.Ordinal),
                new Dictionary<string, IReadOnlyList<GearGridDefinition>>(StringComparer.Ordinal));
        }

        return new GearSlotFacts(templateIdByItemId, slotsByTemplateId, parentByTemplateId, stackCountByItemId, gridsByTemplateId);
    }

    // 자리 하나가 받는 템플릿 집합. Slots·Cartridges·Chambers 가 같은 모양(Props.Filters[].Filter)이다.
    // 비어 있으면 "제한 없음" 이 아니라 "모름" 이고, 정책이 그것을 거부로 읽는다.
    private static HashSet<string> AllowedTemplatesOf(Slot slot)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filter in slot.Properties?.Filters ?? [])
        {
            foreach (var tpl in filter?.Filter ?? [])
            {
                if (!tpl.IsEmpty) allowed.Add(tpl.ToString());
            }
        }

        return allowed;
    }

    // tpl → `_parent` 사슬을 위로 모은다. 이미 들어 있는 고리를 만나면 멈춘다(두 번 걷지 않는다).
    private void CollectTemplateAncestry(MongoId templateId, Dictionary<string, string> parentByTemplateId)
    {
        var current = templateId;
        for (var step = 0; step < 32; step++)
        {
            var key = current.ToString();
            if (parentByTemplateId.ContainsKey(key)) return;

            var found = itemHelper.GetItem(current);
            if (!found.Key || found.Value == null || found.Value.Parent.IsEmpty) return;

            parentByTemplateId[key] = found.Value.Parent.ToString();
            current = found.Value.Parent;
        }
    }

    // SPT 의 Item 을 판정이 쓰는 세 필드로 줄인다. 정책은 SPT 타입을 모른다.
    private static List<WornGearItem> ToWornGearItems(IEnumerable<Item> items) =>
        items.Where(i => i != null).Select(i => new WornGearItem(i.Id.ToString(), i.ParentId, i.SlotId)).ToList();
}
