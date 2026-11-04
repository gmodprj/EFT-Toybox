using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pitTeam.Modules;
using pitTeam.Shared;
using HarmonyLib;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ResultProfile = EFT.OtherPlayerProfile;

namespace pitTeam.Patches
{
    // 편집기 전용 인벤토리 컨트롤러. **종류를 바꾸는 것이 아니다**(불변식 11) - 기본
    // `InventoryController` 그대로이고, 실행 경로(`Execute`)도 그대로 지역 트리에서 끝난다.
    // 바꾸는 것은 `IsAllowedToSort` 라는 **읽기 전용 판정 하나**다.
    //
    // 왜 필요한가(2026-11-04 IL 실측): 정렬은 `ItemManipulator.Sort(item, controller, simulate)` 가
    // 하는데, 그 함수가 **안에서** `controller.IsAllowedToSort(item)` 를 보고 아니면
    // `CannotSortItemError` 를 돌려준다. 그 판정은 기본 `InventoryController` 에서 **무조건 false** 이고
    // `BackEndInventoryController` 만 재정의한다(`item.CurrentAddress?.GetOwner() == this`).
    // 그래서 편집기에서 정렬 버튼이 아무 일도 하지 않았다(대조표 4-2). 버튼을 눌러 일어나는 일은
    // **자리 바꾸기뿐**이라(묶음을 빼서 다시 놓는다) 우리 저장 경로가 그대로 받는다 - 아이템 id 가
    // 그대로이므로 보존 검사를 통과하고, 같은 격자 안이라 자리 검사도 통과한다.
    //
    // 규칙은 `BackEndInventoryController` 의 것을 따르되 **스태시 루트를 함께 허용**한다: 편집기의
    // 스태시는 `InventoryDescriptor` 로 세운 것이라 루트의 `CurrentAddress` 가 비어 있을 수 있고,
    // 그러면 주인 비교만으로는 정렬이 계속 거부된다.
    internal sealed class StmGearEditInventoryController : InventoryController
    {
        public StmGearEditInventoryController(EFT.InventoryLogic.IInventoryProfileInfo profile, bool examined)
            : base(profile, examined)
        {
        }

        public override bool IsAllowedToSort(CompoundItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (ReferenceEquals(item, Inventory?.Stash))
            {
                return true;
            }

            return item.CurrentAddress?.GetOwner() == this;
        }
    }

    // 서버가 /stm/members/gear-stash 로 주는 그 팀원 자신의 스태시. 아이템은 PIT 가 플레이어 스태시를 받는 것과 같은 평면 목록이고
    // 루트가 맨 앞이다. 장비 트리는 없다(상세 화면의 프로필이 이미 갖고 있다).
    internal class StmGearStashWire
    {
        public string StashRootId { get; set; }
        public string EquipmentRootId { get; set; }
        public JsonType.FlatItem[] Items { get; set; }
    }

    // 프로필 팀원의 장비 편집기. 새 화면을 만들지 않는다: PIT 용병 편집기의 **그리기 부품**(스태시 패널, 장비 인형, 아이템 UI
    // 구성, 닫기 정리)을 그대로 쓰고, 출처와 제출만 STM 의 것으로 바꾼다.
    //
    // PIT 편집기와 다른 점 (이 파일이 하는 일의 전부):
    //   1) 출처 칸의 스태시: PIT 는 ActiveProfileSession.Profile 의 **플레이어** 스태시를 복제해 쓴다. 여기서는 서버가 준 그 팀원
    //      자신의 스태시를 EFT 아이템 트리로 세워(FlatItemsToTree) 같은 InventoryDescriptor 경로로 편집기 프로필에 넣는다.
    //      플레이어 스태시는 한 번도 읽지 않는다 - session.Profile 은 프로필 복제의 껍데기(외형·스킬)로만 쓰인다.
    //   2) 제출: PIT 는 플레이어 스태시와 팀원 장비를 통째로 default-equipment 에 보내 서버가 플레이어 스태시를 바꿔 쓴다.
    //      여기서는 **그 팀원 두 트리의 최종 상태**를 /stm/members/apply-inventory 에 한 번에 보낸다(계획 5 Task B).
    //      조작의 종류(이동·나누기·합치기·탄박스 열기·버리기)를 적지 않고 결과만 보내므로, 화면에서 되는 것은
    //      그대로 저장된다. 정당한 변화인지는 서버가 본다(ProfileInventoryApplyPolicy: 늘어나는 것·바깥에서
    //      들어온 종류·구조 손상·트리 손상 거절, 편집기가 들고 있지 않던 아이템의 손실 거절).
    //      2026-11-03 이전의 "건 목록"(GearEditBatchPlanner → /stm/members/edit-gear)은 아이템이 생기고 사라지는
    //      조작을 적을 수 없어 버렸다 - 그 라우트는 서버에서 지웠다.
    //   3) 용병 전용 부가 기능을 열지 않는다: 보험 조회(OpenLoadoutEditorInsurance)와 수리(CanRepairLoadoutEditorEquipmentItem 은
    //      ViewedProfile 이 있어야 열린다 - 여기서는 ViewedProfile 을 건드리지 않는다)는 PIT 의 용병 라우트에 묶여 있다.
    //
    // PIT 의 파일은 이 파일을 위해 바뀌지 않았다: 같은 partial 클래스라 private 부품을 그대로 부른다. PIT 용병 경로
    // (ShowLoadoutEditorOverlay, SaveLoadoutEditorDefaultEquipmentAsync)는 이 파일이 호출하지도, 가로채지도 않는다.
    //
    // 이 파일의 UI 는 게임 없이 검증되지 않는다. 판정(입구, 건 목록과 순서)은 shared 의 정책이고 여기는 그리기와 요청 보내기뿐이다.
    internal partial class OtherPlayerProfileScreenPatch
    {
        private const string StmGearStashRoute = "/stm/members/gear-stash";
        // 계획 5 Task B. 편집기의 **최종 트리 전체**를 보내는 길. 옛 길(edit-gear)은 "이 아이템을 이
        // 부모의 이 슬롯으로" 의 목록이라 아이템이 생기고 사라지는 조작(나누기·합치기·탄박스 열기·
        // 버리기)을 적을 수 없었다. 서버는 조작의 종류를 묻지 않고 보존만 본다.
        private const string StmApplyInventoryRoute = "/stm/members/apply-inventory";

        // ---- 화면 문구 (StmDetail 과 같은 관례: 한 곳에 모은다) ----
        private const string StmEditEntryText = "팀원 장비 편집";
        private const string StmEditorTitleFormat = "{0} 장비 편집";
        private const string StmEditorSubtitle = "왼쪽은 이 팀원 자신의 스태시입니다. 여기 있는 아이템만 입힐 수 있고, 벗긴 아이템은 이 팀원의 스태시로 돌아갑니다. 저장하면 바로 이 팀원의 프로필에 반영됩니다.";
        private const string StmEditorStashTitleFormat = "{0} 의 스태시";
        private const string StmEditorGearTitleFormat = "{0} 의 장비";
        private const string StmEditorCancelText = "취소";
        private const string StmEditorSaveText = "저장";
        private const string StmEditorOpenFailedToast = "장비 편집을 열지 못했습니다";
        private const string StmEditorStashFailedToast = "이 팀원의 스태시를 읽지 못했습니다";
        private const string StmEditorSaveFailedToast = "장비를 저장하지 못했습니다";
        private const string StmEditorNoChangeToast = "바뀐 것이 없어 저장하지 않았습니다.";
        private const string StmEditorSkippedFormat = "이 팀원의 아이템 {0}개를 편집기가 읽지 못해 보이지 않습니다. 저장해도 그 아이템은 건드리지 않습니다.";
        private const string StmEditorAmmoBoxOpenedToast = "탄을 이 팀원의 스태시로 꺼냈습니다.";
        private const string StmEditorAmmoBoxStuckToast = "상자를 비웠지만 치우지 못했습니다. 저장 전에 직접 버리세요.";
        private const string StmEditorAmmoBoxNoRoomToast = "탄을 놓을 스태시 자리가 없어 상자를 열지 않았습니다.";
        private const string StmEditorSavedToastFormat = "장비를 저장했습니다({0}건).";
        private const string StmEditorLayoutNoteFormat = " 칸 위치만 바꾼 {0}개는 저장되지 않습니다.";
        private const string StmEditorSavedButCleanupFailedToast = "장비는 저장되었습니다. 편집기를 정리하다 오류가 났으니 팀원 화면을 다시 열어 확인하세요.";
        private const string StmEditorSavingText = "저장하는 중입니다. 끝날 때까지 편집기를 닫을 수 없습니다...";
        private const string StmEditorPanelFailedToast = "이 팀원의 스태시나 장비 칸을 그리지 못해 편집기를 열지 않았습니다";
        private const string StmEditorStashMismatchToast = "편집기의 스태시가 이 팀원의 것임을 확인하지 못해 편집기를 열지 않았습니다";

        // 편집기를 여는 중(스태시 요청)과 저장 중. 둘 다 한 번에 하나만 - 중복 요청이 같은 편집을 두 번 보내지 않게 한다.
        // 편집기가 **읽지 못해 싣지 못한** 그 팀원 아이템의 수. EFT 가 역직렬화를 거부한 항목(과 그 자식)은
        // 건너뛰고 열리는데, 그러면 화면에 없는 아이템이 생긴다. 저장은 그것들을 건드리지 않지만
        // (서버가 loadedItemIds 밖의 손실을 거절한다) **플레이어는 그 사실을 알아야 한다**.
        private static int StmGearEditSkippedCount;

        // 방금 경고한 소실 묶음의 서명(사라질 id 들을 정렬해 이은 것). 같은 묶음으로 다시 누르면 진행한다.

        private static bool StmGearEditOpening;
        private static bool StmGearEditSaving;
        // 저장 버튼 곁에 서버의 거절 사유를 남겨 두는 줄. 토스트는 금방 사라지므로 읽을 시간이 필요한 사유는 여기에도 둔다.
        private static CustomTextMeshProUGUI StmGearEditStatusLabel;
        // 편집기를 열지 못한 이유(플레이어가 읽는 말). 여는 쪽이 토스트로 보인다.
        private static string StmGearEditOpenFailure;

        // 편집기가 사라진 뒤에 도착한 저장 거절 사유. 화면이 아니라 문자열과 그 주인(팀원)만 든다 - 죽은 편집기를 되살리지 않고,
        // 다른 팀원의 화면에는 나오지 않으며(주인 대조), 한 번 꺼내면 비워지고, 10분이 지나면 버린다.
        private static string StmPendingGearRefusalAccountId;
        private static string StmPendingGearRefusalMessage;
        private static DateTime StmPendingGearRefusalAtUtc;
        // 지금 상세 화면이 보여 주는 팀원. 화면이 재사용돼도 거절 사유가 다른 팀원의 화면에 뜨지 않게 대조하는 기준이다.
        private static string StmShownDetailAccountId;
        private static GameObject StmGearEditEntryRoot;
        private const string StmRefusalPrefix = "직전 장비 저장이 거절되었습니다: ";

        // ---- 입구 ----

        // 상세 화면의 입구. 서버가 열어도 된다고 한 팀원에게만 버튼을 얹는다(canEditGear). 열 수 없으면 이유를 말하는 안내 상자다.
        // 판정은 GearEditEntryPolicy 가 했고 여기는 그리기뿐이다. 값이 없는 응답(옛 서버)은 열 수 없음으로 읽힌다.
        private static void ShowStmGearEditEntry(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            FriendlyTeammateProfileOptions options,
            StmProfileDetailWire detail,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            // 화면이 없으면 아무것도 그리지 못한다 - 대기 중인 사유를 꺼내 버리기 전에 돌아간다.
            if (screen == null) return;

            if (!detail.CanEditGear)
            {
                ShowStmBannerWithPending(screen, GearEditEntryPolicy.NoticeWhenNotEditable(detail.EditNotice), profile?.AccountId, null, null);
                // 장비 편집이 막혀도(레이드 중 등) AI 타입은 바꿀 수 있다. 줄은 만들되 편집 버튼은 눌리지 않는다.
                TryShowStmProfileControlRow(screen, profile, options, detail, inventoryController, session, playerModelWindow);
                return;
            }

            // 대기 중이던 거절 사유가 있으면 입구 위에 안내 상자로 먼저 말한다. 입구는 그 아래로 내려 겹치지 않게 한다.
            // 사유는 안내 상자 그리기가 던지지 않고 돌아온 뒤에야 대기에서 비워진다(그리기가 던지면 대기에 남는다). 아래 버튼 템플릿이
            // 없어 입구를 못 그려도 사유는 이미 안내 상자로 말해진 뒤이고, 입구가 없다는 사실은 경고 로그에 남는다.
            float entryTop = -96f;
            string pendingRefusal = PeekStmPendingGearRefusal(profile?.AccountId);
            if (pendingRefusal != null)
            {
                ShowStmBanner(screen, StmRefusalPrefix + pendingRefusal, null, null);
                // 그려진 뒤에야 비운다(위 ShowStmBanner 가 던지면 대기에 남는다).
                ClearStmPendingGearRefusalIf(pendingRefusal);
                entryTop = -240f;
            }

            // 용병 화면과 같은 한 줄(장비 편집 | AI 타입)을 먼저 시도한다. 성공하면 입구는 그 줄의 왼쪽 칸이다.
            // 거절 사유가 떠 있을 때는 안내 상자 아래의 떠 있는 버튼이 제자리이므로 줄을 만들지 않는다.
            if (pendingRefusal == null
                && TryShowStmProfileControlRow(screen, profile, options, detail, inventoryController, session, playerModelWindow))
            {
                return;
            }

            DefaultUIButton buttonTemplate = BackButtonField?.GetValue(screen) as DefaultUIButton;
            if (buttonTemplate == null)
            {
                // 버튼을 못 그리면 입구가 없다. 열 수 없는 쪽으로 넘어진다.
                pitFireTeam.Log.LogWarning("[STM] Gear edit entry skipped: template button not found. Editing is not offered.");
                return;
            }

            GameObject root = new GameObject("pitFireTeam_StmGearEditEntry", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(screen.transform, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, entryTop);
            rect.sizeDelta = new Vector2(300f, 48f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            // 안내 상자가 없을 때만 ARENA 탭 옆으로 옮긴다. 안내 상자가 서면 그 아래(entryTop)가 제자리다.
            if (pendingRefusal == null)
            {
                TryPlaceEntryBesideArenaTab(screen, rect);
            }

            Image background = root.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0f);
            background.raycastTarget = false;

            // 버튼은 자리를 잡은 뒤의 상자 크기를 따른다(ARENA 탭 옆으로 가면 좁아진다).
            Vector2 buttonSize = new Vector2(rect.sizeDelta.x - 8f, Mathf.Max(rect.sizeDelta.y - 8f, 32f));
            DefaultUIButton button = CreateOverlayButton(buttonTemplate, root.transform, new Vector2(0f, 4f), buttonSize);
            button.name = "pitFireTeam_StmGearEditButton";
            button.SetRawText(StmEditEntryText, 20);
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(() => OpenStmGearEditor(screen, profile, detail, inventoryController, session, playerModelWindow));

            if (pendingRefusal == null) StmDetailRoot = root;
            else StmGearEditEntryRoot = root;
        }

        // 조작 줄의 왼쪽 칸(원래는 장비 프리셋 드롭다운)을 "팀원 장비 편집" 버튼으로 덮는다.
        // PIT 의 ReplaceLoadoutDropdownWithEditButton 과 같은 자리·같은 모양이고, 누를 때 여는 것만 STM 의 편집기다
        // (PIT 의 편집기는 플레이어 스태시를 출처로 쓴다 - 프로필 팀원에게는 틀린 경로다. 서버도 그 저장을 거부한다).
        private static void ReplaceStmLoadoutDropdownWithEditButton(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            InventoryClothingSelectionPanel panel,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            try
            {
                if (screen == null || profile == null || panel == null) return;

                DropDownBox upperDropdown = UpperDropdownField?.GetValue(panel) as DropDownBox;
                RectTransform sourceRect = upperDropdown?.transform as RectTransform;
                Transform buttonParent = sourceRect?.parent ?? panel.transform.Find("Upper") ?? panel.transform;

                Transform existing = buttonParent.Find("pitFireTeam_StmGearEditRowButton");
                if (existing != null) GameObject.Destroy(existing.gameObject);

                DefaultUIButton buttonTemplate = HideoutButtonField?.GetValue(screen) as DefaultUIButton;
                if (buttonTemplate == null)
                {
                    pitFireTeam.Log.LogWarning("[STM] 조작 줄의 장비 편집 버튼을 만들지 못했다: 버튼 템플릿이 없다.");
                    return;
                }

                DefaultUIButton button = GameObject.Instantiate(buttonTemplate, buttonParent, false);
                button.name = "pitFireTeam_StmGearEditRowButton";
                button.gameObject.SetActive(true);
                button.SetRawText(StmEditEntryText, 18);
                HideProfileButtonIconContainer(button);
                // 서버가 열 수 없다고 했으면(레이드 중 등) 버튼은 보이되 눌리지 않는다. 사유는 위의 안내 상자가 말한다.
                button.Interactable = detail != null && detail.CanEditGear;
                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener(() => OpenStmGearEditor(screen, profile, detail, inventoryController, session, playerModelWindow));

                if (button.transform is RectTransform buttonRect)
                {
                    if (sourceRect != null)
                    {
                        buttonRect.anchorMin = sourceRect.anchorMin;
                        buttonRect.anchorMax = sourceRect.anchorMax;
                        buttonRect.pivot = sourceRect.pivot;
                        buttonRect.anchoredPosition = sourceRect.anchoredPosition;
                        buttonRect.sizeDelta = sourceRect.sizeDelta;
                        buttonRect.offsetMin = sourceRect.offsetMin;
                        buttonRect.offsetMax = sourceRect.offsetMax;
                    }

                    buttonRect.localScale = Vector3.one;
                }

                if (upperDropdown != null) upperDropdown.gameObject.SetActive(false);

                Transform icon = panel.transform.Find("Upper/Icon");
                if (icon != null) icon.gameObject.SetActive(false);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 조작 줄의 장비 편집 버튼을 만들다 실패했다. 줄은 전술 드롭다운만 남는다.");
                pitFireTeam.Log.LogError(ex);
            }
        }

        // 통계 화면의 EFT / ARENA 탭. 입구 버튼을 그 줄 오른쪽에 붙이려고만 쓴다(2026-11-02 인게임에서 입구가 두 탭 **아래**에
        // 겹쳐 보였다). 화면 모양이 바뀌어 못 찾으면 그냥 쓰지 않는다 — 입구는 원래 자리인 가운데 위에 선다.
        private static readonly FieldInfo ArenaToggleField = AccessTools.Field(typeof(OtherPlayerProfileScreen), "_arenaToggle");

        private const float StmEntryTabGap = 12f;

        private static void TryPlaceEntryBesideArenaTab(OtherPlayerProfileScreen screen, RectTransform rect)
        {
            try
            {
                if (!(ArenaToggleField?.GetValue(screen) is Component toggle)) return;
                if (!(toggle.transform is RectTransform arenaRect)) return;
                if (!(arenaRect.parent is RectTransform row)) return;

                // 탭의 자기 RectTransform 은 눈에 보이는 탭보다 작을 수 있다(그림과 글자가 자식이다).
                // 2026-11-02 인게임에서 `anchoredPosition + rect.width` 로 붙였더니 ARENA 위에 겹쳐 그려졌다.
                // 자식까지 포함한 실제 경계를 재서 그 오른쪽 끝에 붙인다.
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(row, arenaRect);
                if (bounds.size.x <= 1f) return;

                // 앵커·피벗은 ARENA 와 같게 두고 거기서 옮긴다. 그래야 줄의 피벗이 어디에 있든 계산이 맞는다.
                // 옮기는 양은 좌표가 아니라 **경계로** 잰다: 우리 상자의 왼쪽 끝이 ARENA 의 오른쪽 끝 옆에 오게 한다.
                rect.SetParent(row, false);
                rect.anchorMin = arenaRect.anchorMin;
                rect.anchorMax = arenaRect.anchorMax;
                rect.pivot = arenaRect.pivot;
                rect.sizeDelta = new Vector2(240f, Mathf.Max(bounds.size.y, 36f));
                rect.localScale = Vector3.one;
                rect.anchoredPosition = arenaRect.anchoredPosition;

                Bounds mine = RectTransformUtility.CalculateRelativeRectTransformBounds(row, rect);
                rect.anchoredPosition += new Vector2(bounds.max.x - mine.min.x + StmEntryTabGap, bounds.center.y - mine.center.y);
                rect.SetAsLastSibling();
            }
            catch (Exception ex)
            {
                // 자리를 못 잡아도 입구는 이미 가운데 위에 있다. 쓸 수 없게 되는 것은 아니다.
                pitFireTeam.Log.LogWarning($"[STM] Could not place the gear edit entry next to the ARENA tab: {ex.Message}");
            }
        }

        // ---- 열기 ----

        private static async void OpenStmGearEditor(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            if (StmGearEditOpening || StmGearEditSaving || LoadoutEditorOverlayRoot != null) return;
            StmGearEditOpening = true;
            try
            {
                if (screen == null || profile == null || detail == null || !detail.CanEditGear
                    || string.IsNullOrWhiteSpace(detail.ProfileId) || session?.Profile == null || inventoryController == null)
                {
                    AddTeammateCreationFlow.ShowToast(StmEditorOpenFailedToast);
                    return;
                }

                // 그 팀원 자신의 스태시. 서버는 저장과 같은 관문을 서므로 레이드·미결 전송이면 여기서 사유와 함께 거절된다.
                string body = JsonConvert.SerializeObject(new { aid = profile.AccountId, profileId = detail.ProfileId });
                string json = await Task.Run(() => RequestHandler.PostJson(StmGearStashRoute, body));
                string failure = ReadStmCloseFailure(json, StmEditorStashFailedToast);
                if (failure != null)
                {
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                StmGearStashWire stash = DeserializeBodySuccess<StmGearStashWire>(json)?.data;
                if (stash?.Items == null || stash.Items.Length == 0 || string.IsNullOrWhiteSpace(stash.StashRootId))
                {
                    AddTeammateCreationFlow.ShowToast(StmEditorStashFailedToast);
                    return;
                }

                StmGearEditOpenFailure = null;
                if (!TryShowStmGearEditorOverlay(screen, profile, detail, stash, inventoryController, session, playerModelWindow))
                {
                    AddTeammateCreationFlow.ShowToast(StmGearEditOpenFailure ?? StmEditorOpenFailedToast);
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Opening the profile teammate gear editor failed.");
                pitFireTeam.Log.LogError(ex);
                TeardownStmGearEditor();
                AddTeammateCreationFlow.ShowToast(ex.Message ?? StmEditorOpenFailedToast);
            }
            finally
            {
                StmGearEditOpening = false;
            }
        }

        // PIT 의 ShowLoadoutEditorOverlay 와 같은 겉틀(배경·패널·머리줄·두 구역·두 버튼)에 STM 의 글과 STM 의 저장을 단다.
        // 겉틀을 따로 두는 것은 PIT 함수에 분기를 넣지 않으려는 것이다 - 용병 경로의 코드가 한 줄도 바뀌지 않는다.
        private static bool TryShowStmGearEditorOverlay(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            StmGearStashWire stash,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            CloseLoadoutEditorOverlay();

            DefaultUIButton buttonTemplate = BackButtonField?.GetValue(screen) as DefaultUIButton;
            if (buttonTemplate == null)
            {
                pitFireTeam.Log.LogWarning("[STM] Gear editor overlay aborted: template button not found.");
                return false;
            }

            // PIT 의 편집기 부품이 읽는 전역을 이 편집기 동안만 채운다(ViewedProfile 은 일부러 비워 둔다 - 수리 등 용병 전용
            // 경로가 열리지 않는다). 화면이 닫히거나 다시 열리면 PIT 의 정리 코드(ResetTeammateProfileUi·Hide 후처리)가 비운다.
            ActiveProfileSession = session;
            ActiveProfileInventoryController = inventoryController;
            RememberActiveBackendInventoryController(session, inventoryController);

            string nickname = profile.Info?.Nickname ?? "teammate";

            GameObject overlayRoot = new GameObject("pitFireTeam_LoadoutEditorOverlay", typeof(RectTransform), typeof(Image));
            overlayRoot.transform.SetParent(screen.transform, false);
            RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.localScale = Vector3.one;
            overlayRect.SetAsLastSibling();

            Image backdrop = overlayRoot.GetComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.55f);
            backdrop.raycastTarget = true;

            // LoadoutEditorOverlayRoot 는 PIT 의 여러 패치(장비 루트 컨텍스트, 탄 빼기, 인덱스)가 "편집기가 열려 있다" 의
            // 표지로 읽는다. 겉틀이 서자마자 채워 두면 아래 부품이 중간에 실패해도 정리(CloseLoadoutEditorOverlay)가 이 겉틀을 치운다.
            LoadoutEditorOverlayRoot = overlayRoot;

            GameObject panel = new GameObject("pitFireTeam_LoadoutEditorPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlayRoot.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.offsetMin = new Vector2(235f, 100f);
            panelRect.offsetMax = new Vector2(-235f, -100f);
            panelRect.localScale = Vector3.one;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.86f);
            panelImage.raycastTarget = true;

            GameObject header = new GameObject("pitFireTeam_LoadoutEditorHeader", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(panel.transform, false);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(0f, -36f);
            headerRect.offsetMax = Vector2.zero;

            Image headerImage = header.GetComponent<Image>();
            headerImage.color = new Color(0.055f, 0.055f, 0.055f, 0.95f);
            headerImage.raycastTarget = true;
            AddLoadoutEditorHeaderDivider(header.transform, "TopDivider", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -1f), Vector2.zero);
            AddLoadoutEditorHeaderDivider(header.transform, "BottomDivider", Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 1f));

            ProfileOverlayHeaderDragHandle dragHandle = header.AddComponent<ProfileOverlayHeaderDragHandle>();
            dragHandle.Target = panelRect;

            CreateOverlayText(
                "pitFireTeam_LoadoutEditorTitle",
                header.transform,
                new Vector2(18f, 0f),
                new Vector2(-54f, 0f),
                TextAlignmentOptions.MidlineLeft,
                string.Format(StmEditorTitleFormat, nickname),
                20f,
                new Color(0.87f, 0.87f, 0.84f, 1f));

            Button closeButton = CreateWindowCloseButton(header.transform);
            if (closeButton.transform is RectTransform closeRect)
            {
                closeRect.anchorMin = new Vector2(1f, 0.5f);
                closeRect.anchorMax = new Vector2(1f, 0.5f);
                closeRect.pivot = new Vector2(1f, 0.5f);
                closeRect.anchoredPosition = new Vector2(-6f, 0f);
            }

            closeButton.onClick.AddListener(new UnityAction(CloseStmGearEditor));

            // 앵커를 **직접** 준다. `CreateOverlayText` 는 앵커를 (0,0)-(1,1) 전체 늘이기로 고정하고
            // (LoadoutUi.cs:773-774, PIT 업스트림 파일이라 건드리지 않는다) 그 상태에서 offsetMin.y 는
            // "패널 **바닥**에서" 를 뜻한다. 여기 쓰던 -62/-98 은 "패널 위에서 아래로" 의 수였으므로
            // 상자가 패널 거의 전체 높이로 늘어났고, MidlineLeft 가 그 안에서 세로 가운데에 글씨를 놓아
            // **안내 문구가 격자 한가운데를 가로질러** 그려졌다(2026-11-04 인게임 실측).
            // 같은 함정이 꾸러미 구매 창에서도 한 번 나왔다(WORKLOG "좌표계에서 한 번 틀렸다").
            // 바로 아래 상태줄이 이미 같은 방식으로 앵커를 덮어쓴다 - 그 짝을 맞춘다.
            CustomTextMeshProUGUI subtitle = CreateOverlayText(
                "pitFireTeam_LoadoutEditorSubtitle",
                panel.transform,
                Vector2.zero,
                Vector2.zero,
                TextAlignmentOptions.MidlineLeft,
                StmEditorSubtitle,
                17f,
                new Color(0.67f, 0.67f, 0.64f, 1f));
            if (subtitle.transform is RectTransform subtitleRect)
            {
                // 머리글(36px) 아래, 두 칸이 시작하는 -104 위. 40px 띠다.
                subtitleRect.anchorMin = new Vector2(0f, 1f);
                subtitleRect.anchorMax = new Vector2(1f, 1f);
                subtitleRect.offsetMin = new Vector2(28f, -100f);
                subtitleRect.offsetMax = new Vector2(-28f, -60f);
            }

            RectTransform leftSection = CreateLoadoutEditorSection(
                panel.transform,
                "pitFireTeam_StmTeammateStashSection",
                string.Format(StmEditorStashTitleFormat, nickname),
                new Vector2(0f, 0f),
                new Vector2(0.5f, 1f),
                new Vector2(16f, 64f),
                // -104 → -136. 안내 문구 띠는 위에서 60~100 인데 칸이 104 에서 시작해 **여유가 4px
                // 뿐**이었고, 그래서 스태시 패널이 자기 머리글(창고·통화 줄)을 그리면 문구와 겹쳐
                // 보였다(2026-11-04 인게임). 36px 를 비워 둔다 - 검색칸까지 켜면 그 줄이 더
                // 빠듯해지므로(대조표 2-19) 넉넉한 쪽으로 잡는다. 격자에서 32px 를 내주는 값이다.
                new Vector2(-2f, -136f));

            RectTransform rightSection = CreateLoadoutEditorSection(
                panel.transform,
                "pitFireTeam_BotInventorySection",
                string.Format(StmEditorGearTitleFormat, nickname),
                new Vector2(0.5f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 64f),
                // 왼쪽 칸과 같은 높이로 맞춘다 - 둘이 어긋나면 머리글 줄이 삐뚤어 보인다.
                new Vector2(-10f, -136f));

            if (!TryBuildStmGearEditorPanels(profile, stash, leftSection, rightSection, session))
            {
                // 패널을 세우지 못했다. 빈 겉틀로 저장 버튼을 두지 않고 닫는다.
                TeardownStmGearEditor();
                return false;
            }

            // 거절 사유를 남겨 두는 줄. 저장·취소 버튼의 왼쪽 아래.
            CustomTextMeshProUGUI status = CreateOverlayText(
                "pitFireTeam_StmGearEditStatus",
                panel.transform,
                Vector2.zero,
                Vector2.zero,
                TextAlignmentOptions.MidlineLeft,
                string.Empty,
                17f,
                new Color(0.93f, 0.62f, 0.55f, 1f));
            if (status.transform is RectTransform statusRect)
            {
                statusRect.anchorMin = new Vector2(0f, 0f);
                statusRect.anchorMax = new Vector2(1f, 0f);
                statusRect.offsetMin = new Vector2(28f, 6f);
                statusRect.offsetMax = new Vector2(-410f, 60f);
            }

            StmGearEditStatusLabel = status;

            DefaultUIButton cancelButton = CreateOverlayButton(buttonTemplate, panel.transform, Vector2.zero, new Vector2(180f, 36f));
            cancelButton.name = "pitFireTeam_LoadoutEditorCancelButton";
            cancelButton.SetRawText(StmEditorCancelText, 20);
            cancelButton.OnClick.RemoveAllListeners();
            cancelButton.OnClick.AddListener(CloseStmGearEditor);
            if (cancelButton.transform is RectTransform cancelRect)
            {
                cancelRect.anchorMin = new Vector2(1f, 0f);
                cancelRect.anchorMax = new Vector2(1f, 0f);
                cancelRect.pivot = new Vector2(1f, 0f);
                cancelRect.anchoredPosition = new Vector2(-212f, 18f);
                cancelRect.localScale = Vector3.one * 0.9f;
            }

            DefaultUIButton saveButton = CreateOverlayButton(buttonTemplate, panel.transform, Vector2.zero, new Vector2(180f, 36f));
            saveButton.name = "pitFireTeam_LoadoutEditorDoneButton";
            saveButton.SetRawText(StmEditorSaveText, 20);
            saveButton.OnClick.RemoveAllListeners();
            saveButton.OnClick.AddListener(() => SaveStmGearEdit(screen, profile, detail, inventoryController, session, playerModelWindow));
            if (saveButton.transform is RectTransform saveRect)
            {
                saveRect.anchorMin = new Vector2(1f, 0f);
                saveRect.anchorMax = new Vector2(1f, 0f);
                saveRect.pivot = new Vector2(1f, 0f);
                saveRect.anchoredPosition = new Vector2(-24f, 18f);
                saveRect.localScale = Vector3.one * 0.9f;
            }

            return true;
        }

        // PIT 의 TryBuildLoadoutEditorPanels 와 같은 순서로 부품을 세운다. 다른 점은 편집기 프로필의 스태시가 서버가 준 그 팀원의
        // 스태시라는 것, 그리고 보험 조회를 열지 않는다는 것이다. 어느 하나라도 못 세우면 false(편집기를 열지 않는다).
        private static bool TryBuildStmGearEditorPanels(
            ResultProfile profile,
            StmGearStashWire stash,
            RectTransform leftSection,
            RectTransform rightSection,
            EFT.IEftSession session)
        {
            ItemUiContext itemUiContext = ItemUiContext.Instance;
            if (profile?.Equipment == null || itemUiContext == null)
            {
                pitFireTeam.Log.LogWarning("[STM] Gear editor build aborted: teammate equipment or ItemUiContext was unavailable.");
                return false;
            }

            if (!TryCreateStmGearEditorProfile(profile, stash, session, out Profile editorProfile, out InventoryController editorInventoryController))
            {
                return false;
            }

            LoadoutEditorProfile = editorProfile;
            LoadoutEditorInventoryController = editorInventoryController;
            RebuildLoadoutEditorItemIndexes();

            itemUiContext.Configure(
                editorInventoryController,
                editorProfile,
                session,
                session.InsuranceCompany,
                null,
                null,
                new CompoundItem[] { editorProfile.Inventory.Stash },
                EItemUiContextType.TransferItemsScreen,
                ECursorResult.ShowCursor,
                null,
                editorProfile.Inventory.Equipment,
                null);

            // 플레이어 인벤토리 화면과 같은 문맥을 준다(InventoryScreenController 가 쓰는 바로 그것).
            // PIT 가 쓰던 AreaStashItemContext 로는 스태시 칸의 조작 메뉴가 짧았다 - 탄박스를 리그로 옮기면
            // 메뉴가 길어지는데 스태시에서는 짧은 이유가 이 **문맥의 종류**였다(2026-11-03 인게임).
            // 검색창이 **이 문맥 그대로** 를 받아야 한다(아래 EnableStmStashSearch) - 패널도 같은 객체를 쓴다
            // (`SimpleStashPanel.Show` 가 `itemContext as InventorySelectableItemContext` 로 받는다).
            EFT.InventoryLogic.InventorySelectableItemContext stashContext =
                new EFT.InventoryLogic.InventorySelectableItemContext(EItemViewType.Inventory);

            TryBuildLoadoutEditorStashPanel(
                leftSection,
                editorProfile,
                editorInventoryController,
                stashContext);
            // **`searchAvailability` 는 `Unavailable` 그대로 둔다(기본값) - 검색칸은 우리가 직접 단다.**
            // `SimpleStashPanel.InitSearch` 의 관문이 그 값만 보는 것이 아니다(2026-11-04 IL 실측):
            //   flag = 값 != Unavailable && _searchTab != null && tab == Gear
            //          && bool_2 && ViewType ∈ {Inventory, TradingPlayer, TransferPlayer, ScavInventory}
            // 이고 `bool_2` 가 **`_item.Parent.GetOwner() is BackEndInventoryController`** 다.
            // 우리 편집기의 스태시 주인은 기본 `InventoryController` 이므로(불변식 11 - 그것이 안전장치다)
            // 값을 `StashOnly` 로 바꿔도 flag 는 거짓이고 `_searchTab` 은 꺼진 채였다. 2026-11-04 에
            // "값은 들어갔는데 패널이 안 세워진다" 고 적은 것의 정체가 이 관문이고, 그때 함께 본
            // "오른쪽 빈 폭" 은 검색칸과 **무관한 별개**였다(격자가 고정 폭이라 남는 폭 - 아래 손질).
            // 우회로는 `InitSearch` 를 건너뛰고 창을 **직접 여는 것**이다: `ItemUiContext.ShowStashSearchWindow`
            // 와 `StashSearchWindow.Show` 에는 컨트롤러 종류 검사가 **없고**(바로 옆 `ShowSortingTable` 에는
            // 있다) 우리 컨트롤러·문맥을 그대로 받는다. 불변식 11 의 길 ⒜ 다 - 안전장치를 깎지 않는다.
            // 보안 컨테이너 칸을 그린다 - 플레이어 자신의 화면에는 늘 있고, 프로필 팀원도 같아야 한다.
            // 컨테이너 **자체**를 떼는 것은 서버가 계속 막는다(IsStructuralSlot) - 여는 것만 열린다.
            // 장비 트리도 스태시와 **같은 계열의 문맥**을 준다. 그래야 리그·가방 안의 탄창에
            // `삽탄`(LoadAmmo)·`탄창 프리셋`이 뜬다 - 그 둘은 문맥이 InventorySelectableItemContext
            // 일 때만 열리는데, PIT 의 장비 루트 문맥은 자식에게 DefaultItemContext 를 준다
            // (2026-11-04 인게임: "리그의 탄창에 삽탄이 없다"). 근거는 StmGearEditorEquipmentRootContext.
            TryBuildLoadoutEditorFollowerPanel(
                profile, rightSection, itemUiContext, editorProfile, editorInventoryController,
                keepSecuredContainer: true,
                equipmentItemContextOverride: new StmGearEditorEquipmentRootContext(EItemViewType.Inventory));

            // PIT 의 두 패널 함수는 실패를 삼켜 대체 글자로 바꾸고 void 로 돌아온다 - 겉으로는 쓸 수 있는 편집기처럼 보인다.
            // 패널 필드는 성공했을 때만 채워진다(편집기를 열기 직전 CloseLoadoutEditorOverlay 가 비웠다). 비어 있으면 열지 않는다.
            if (LoadoutEditorStashPanel == null || LoadoutEditorEquipmentPanel == null)
            {
                pitFireTeam.Log.LogError($"[STM] Gear editor panels were not built (stash={(LoadoutEditorStashPanel != null)}, equipment={(LoadoutEditorEquipmentPanel != null)}); the editor is not opened.");
                StmGearEditOpenFailure = StmEditorPanelFailedToast;
                return false;
            }

            // 왼쪽 칸의 **네 가장자리를 한 번에 맞춘다**(빈 띠·창 밖으로 나간 자식·잘림). 상수를 박지
            // 않고 그려진 월드 모서리를 재며, 고친 자리와 남은 여백을 전부 로그에 남긴다 - 셈과 근거는
            // 그 함수의 주석에 있다. 검색칸과는 무관한 자리다(그쪽은 아래 EnableStmStashSearch).
            FitStmStashSection(leftSection);

            // 플레이어 창고 화면과 같은 검색칸을 단다(대조표 2-19).
            EnableStmStashSearch(itemUiContext, editorProfile, stashContext);

            // 패널을 세우는 동안 아이템 컴포넌트 상태(특히 무기 부착물)가 초기화될 수 있다. 플레이어가 만지기 전의 이 시점이
            // "바뀌었는가" 를 따질 첫 안정된 기준이다(PIT 편집기와 같은 이유).
            try
            {
                CaptureLoadoutEditorInitialState(editorProfile);
                if (StmGearEditSkippedCount > 0)
                {
                    // 저장은 이것들을 건드리지 않는다(서버가 거절한다). 보이지 않는다는 사실만 말한다.
                    string skippedNotice = string.Format(StmEditorSkippedFormat, StmGearEditSkippedCount);
                    SetStmGearEditStatus(skippedNotice);
                    // 상태 줄은 스태시 패널에 가릴 수 있다(실측). 토스트로도 한 번 말한다.
                    AddTeammateCreationFlow.ShowToast(skippedNotice);
                }
            }
            catch (Exception ex)
            {
                // 기준이 없으면 비교할 수 없다 - 이 편집기는 저장할 수 없다.
                pitFireTeam.Log.LogError($"[STM] Could not capture the gear editor baseline; the editor is not opened: {ex.Message}");
                return false;
            }

            return LoadoutEditorInitialEquipmentItems != null && LoadoutEditorInitialStashItems != null;
        }

        // 검색칸(`_searchTab`)을 직접 켜고 우리 손으로 창을 연다. `InitSearch` 가 켜 주지 않는 이유는
        // 호출부의 주석(불변식 11 의 `bool_2` 관문)에 있다.
        // 플레이어의 창고 화면(`LobbyInventoryScreenController`)을 그대로 따른다: 그쪽은
        // `EStashSearchAvailability.All` + `InventorySelectableItemContext(Inventory)` 이고, 우리 편집기의
        // 스태시 문맥이 **같은 종류**다. `All` 의 뜻은 창에 장비도 함께 검색하는 토글이 선다는 것이라
        // 장비 트리를 같이 넘긴다(그 팀원의 장비다 - 플레이어의 것이 아니다).
        private static void EnableStmStashSearch(
            ItemUiContext itemUiContext,
            Profile editorProfile,
            EFT.InventoryLogic.ItemContext stashContext)
        {
            try
            {
                SimpleStashPanel stashPanel = LoadoutEditorStashPanel;
                ToggleEFT searchTab = stashPanel?._searchTab;
                EFT.InventoryLogic.Stash stashRoot = editorProfile?.Inventory?.Stash;
                if (searchTab == null || stashRoot == null || stashContext == null || itemUiContext == null)
                {
                    // 템플릿에 검색칸이 없을 수 있다(패널 프리팹이 바뀌면). 없으면 편집기는 그대로 연다.
                    pitFireTeam.Log.LogWarning(
                        $"[STM] Stash search tab was not wired (tab={(searchTab != null)}, stash={(stashRoot != null)}, context={(stashContext != null)}, uiContext={(itemUiContext != null)}).");
                    return;
                }

                // `InitSearch` 가 flag=false 로 꺼 두고 머리글을 켜 두었다. 그 둘을 플레이어 화면 쪽으로 뒤집는다
                // - 플레이어의 창고에서도 검색칸이 "창고" 이름표 자리를 대신한다(`_header.SetActive(!flag)`).
                searchTab.gameObject.SetActive(true);
                if (stashPanel._header != null)
                {
                    stashPanel._header.SetActive(false);
                }

                StmStashSearchUnsubscribe = searchTab.OnStateChange.Subscribe(active =>
                {
                    if (!active)
                    {
                        return;
                    }

                    try
                    {
                        // 창이 받는 문맥은 **스태시 문맥의 자식**이다(바닐라 `SearchHandler` 와 같다).
                        // 그래서 창에서 꺼낸 아이템도 우리 지역 트리로만 간다.
                        WindowContext window = itemUiContext.ShowStashSearchWindow(
                            stashContext.CreateChild(stashRoot),
                            editorProfile.Inventory.Equipment);
                        StmStashSearchWindow = window;
                        if (window != null)
                        {
                            window.OnClose += () =>
                            {
                                StmStashSearchWindow = null;
                                searchTab.Show(ToggleEFT.EToggleState.Off);
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        pitFireTeam.Log.LogError($"[STM] Could not open the stash search window: {ex.GetType().Name}: {ex.Message}");
                        searchTab.Show(ToggleEFT.EToggleState.Off);
                    }
                });

                searchTab.Show(ToggleEFT.EToggleState.Off);
                pitFireTeam.Log.LogInfo("[STM] Stash search tab wired for the teammate gear editor.");
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Could not wire the stash search tab: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // 검색칸 구독을 끊는 손잡이와 열려 있는 창. 편집기를 닫을 때 둘 다 정리한다 - 창이 편집기보다
        // 오래 남으면 **사라진 지역 트리**를 가리키게 된다.
        private static Action StmStashSearchUnsubscribe;
        private static WindowContext StmStashSearchWindow;

        private static void CloseStmStashSearch()
        {
            try
            {
                StmStashSearchWindow?.Close();
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Could not close the stash search window: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                StmStashSearchWindow = null;
            }

            try
            {
                StmStashSearchUnsubscribe?.Invoke();
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Could not unsubscribe the stash search tab: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                StmStashSearchUnsubscribe = null;
            }
        }

        // 왼쪽 칸(스태시)의 **네 가장자리를 한 번에 맞춘다.** 2026-11-04 에 오른쪽 띠만 보고 고쳤다가
        // 기준을 잘못 잡아 격자를 잘랐고, 그 다음엔 왼쪽으로 튀어나온 필터 아이콘 띠를 못 봐서 또 돌았다.
        // 그래서 이 함수는 **한 자리만 고치지 않는다**: 재고 → 왼쪽을 들이고 → 오른쪽 띠를 걷고 →
        // 안 들어가면 돌려주고 → 오른쪽으로 나간 것을 들인 다음, 최종 수치를 전부 로그에 남긴다.
        //
        // 왜 상수를 박지 않는가: 격자 폭은 칸 수에 따라 고정이고(`ItemViewFactory.GetCellPixelSize` =
        // 칸수 × 63 + 1), 복제한 패널에는 자기 rect **바깥**에 붙는 자식이 있으며(범주 필터 띠는 왼쪽
        // 바깥이다 - 플레이어 화면에는 그 자리가 있지만 우리 창에는 없다), 안쪽 여백은 프리팹의 것이다.
        // 셋 다 해상도·칸 수·프리팹에 따라 달라지므로 **그려진 월드 모서리를 재는 것**만이 답을 준다.
        //
        // 어느 쪽으로 넘어뜨리는가(불변식 2): **격자가 잘리는 것이 빈 띠보다 나쁘다.** 그래서 의심스러우면
        // 덜 줄이고, 한계(`StmStashSectionMaxAdjust`)를 넘는 값이 나오면 엉뚱한 자식을 잰 것으로 보고
        // 아무것도 하지 않는다.
        private static void FitStmStashSection(RectTransform sectionContent)
        {
            try
            {
                SimpleStashPanel stashPanel = LoadoutEditorStashPanel;
                if (stashPanel == null || sectionContent == null)
                {
                    return;
                }

                // 줄일 것은 배경 상자(부모)다. 내용물 칸은 앵커가 늘이기라 부모를 줄이면 따라온다.
                // 그 배경 상자의 부모가 편집기 창이고, "창 밖" 의 기준이 그것이다.
                if (!(sectionContent.parent is RectTransform sectionRect) || !(sectionRect.parent is RectTransform windowRect))
                {
                    return;
                }

                // 격자·자식이 최종 크기를 갖기 전에 재면 0 이 나온다.
                Canvas.ForceUpdateCanvases();

                ScrollRect scroll = stashPanel._stashScroll;
                RectTransform viewport = scroll?.viewport ?? scroll?.transform as RectTransform;

                // 스크롤바는 격자 오른쪽에 그대로 두어야 한다 - 그 폭만큼은 남긴다.
                float scrollbarWidth = 0f;
                if (scroll?.verticalScrollbar?.transform is RectTransform scrollbarRect)
                {
                    scrollbarWidth = Mathf.Max(0f, scrollbarRect.rect.width);
                }

                // ── 1. 잰다 ───────────────────────────────────────────────────────────────
                MeasureStmStashPanelEdges(stashPanel, windowRect, out float childLeft, out float childRight, out string leftName, out string rightName);
                float gridRight = MeasureStmStashGridRight(stashPanel, sectionRect);

                pitFireTeam.Log.LogInfo(
                    $"[STM] Stash fit (before): window=[{windowRect.rect.xMin:F1},{windowRect.rect.xMax:F1}], section={sectionRect.rect.width:F1}, "
                    + $"viewport={(viewport != null ? viewport.rect.width : -1f):F1}, gridRight={gridRight:F1}, scrollbar={scrollbarWidth:F1}, "
                    + $"childLeft={childLeft:F1} ({leftName ?? "-"}), childRight={childRight:F1} ({rightName ?? "-"}).");

                // ── 2. 왼쪽으로 창 밖에 나간 것을 들인다 ──────────────────────────────────
                if (leftName != null)
                {
                    float shift = (windowRect.rect.xMin + StmStashWindowEdgeMargin) - childLeft;
                    if (shift > StmStashSectionSlackEpsilon)
                    {
                        if (shift > StmStashSectionMaxAdjust)
                        {
                            pitFireTeam.Log.LogWarning($"[STM] Stash left shift {shift:F1} exceeds the {StmStashSectionMaxAdjust:F0} limit; left edge left as is.");
                        }
                        else
                        {
                            sectionRect.offsetMin = new Vector2(sectionRect.offsetMin.x + shift, sectionRect.offsetMin.y);
                            Canvas.ForceUpdateCanvases();
                            pitFireTeam.Log.LogInfo($"[STM] Stash left edge shifted in by {shift:F1} ({leftName}).");
                        }
                    }
                }

                // ── 3. 격자 오른쪽의 빈 띠를 걷는다 ───────────────────────────────────────
                // **기준은 칸이 아니라 뷰포트의 오른쪽 끝이다.** 칸과 뷰포트 사이의 여백은 남는 폭이
                // 아니라 프리팹이 둔 여백인데, 2026-11-04 1차 고침이 그것까지 걷어 격자를 8px 잘랐다.
                gridRight = MeasureStmStashGridRight(stashPanel, sectionRect);
                if (gridRight > float.MinValue + 1f)
                {
                    float viewportRight = sectionRect.rect.xMax;
                    if (viewport != null)
                    {
                        Vector3[] viewportCorners = new Vector3[4];
                        viewport.GetWorldCorners(viewportCorners);
                        viewportRight = sectionRect.InverseTransformPoint(viewportCorners[2]).x;
                    }

                    float slack = viewportRight - gridRight - scrollbarWidth;
                    if (slack > StmStashSectionSlackEpsilon && slack <= StmStashSectionMaxAdjust)
                    {
                        sectionRect.offsetMax = new Vector2(sectionRect.offsetMax.x - slack, sectionRect.offsetMax.y);
                        Canvas.ForceUpdateCanvases();
                        pitFireTeam.Log.LogInfo($"[STM] Stash right slack trimmed by {slack:F1} (viewportRight={viewportRight:F1}, gridRight={gridRight:F1}).");
                    }
                    else if (slack > StmStashSectionMaxAdjust)
                    {
                        pitFireTeam.Log.LogWarning($"[STM] Stash right slack {slack:F1} exceeds the {StmStashSectionMaxAdjust:F0} limit; right edge left as is.");
                    }
                }
                else
                {
                    pitFireTeam.Log.LogWarning("[STM] Stash grid view was not found; the right slack is left as is.");
                }

                // ── 4. 오른쪽으로 창 밖에 나간 것을 들인다 ────────────────────────────────
                MeasureStmStashPanelEdges(stashPanel, windowRect, out _, out childRight, out _, out rightName);
                if (rightName != null)
                {
                    float pullIn = childRight - (windowRect.rect.xMax - StmStashWindowEdgeMargin);
                    if (pullIn > StmStashSectionSlackEpsilon && pullIn <= StmStashSectionMaxAdjust)
                    {
                        sectionRect.offsetMax = new Vector2(sectionRect.offsetMax.x - pullIn, sectionRect.offsetMax.y);
                        Canvas.ForceUpdateCanvases();
                        pitFireTeam.Log.LogInfo($"[STM] Stash right edge pulled in by {pullIn:F1} ({rightName}).");
                    }
                    else if (pullIn > StmStashSectionMaxAdjust)
                    {
                        pitFireTeam.Log.LogWarning($"[STM] Stash right pull-in {pullIn:F1} exceeds the {StmStashSectionMaxAdjust:F0} limit; right edge left as is.");
                    }
                }

                // ── 5. 그래도 안 들어가면 돌려준다 ────────────────────────────────────────
                // 위 셈이 보지 못하는 안쪽 여백이 또 있을 수 있다. 모델이 아니라 **증상**(내용물이
                // 뷰포트보다 넓다 = 가로 스크롤이 생긴다)을 재서 그 차이를 돌려준다.
                for (int pass = 0; pass < 3; pass++)
                {
                    RectTransform content = scroll?.content;
                    if (content == null || viewport == null)
                    {
                        break;
                    }

                    float overflow = content.rect.width - viewport.rect.width;
                    if (overflow <= 0f)
                    {
                        break;
                    }

                    float giveBack = overflow + StmStashSectionSlackEpsilon;
                    pitFireTeam.Log.LogWarning(
                        $"[STM] Stash content overflows (pass {pass + 1}): content={content.rect.width:F1}, viewport={viewport.rect.width:F1}; giving back {giveBack:F1}.");
                    sectionRect.offsetMax = new Vector2(sectionRect.offsetMax.x + giveBack, sectionRect.offsetMax.y);
                    Canvas.ForceUpdateCanvases();
                }

                // 폭이 바뀌었으니 스크롤을 맨 위로 되돌린다(패널을 세울 때 한 것이 흐트러질 수 있다).
                if (scroll != null)
                {
                    scroll.StopMovement();
                    scroll.normalizedPosition = Vector2.up;
                }

                // ── 6. 최종 상태를 남긴다 ─────────────────────────────────────────────────
                // 다음 사람이 **이 한 줄만 보고** 어디가 몇 px 남았는지 알 수 있어야 한다.
                MeasureStmStashPanelEdges(stashPanel, windowRect, out childLeft, out childRight, out leftName, out rightName);
                gridRight = MeasureStmStashGridRight(stashPanel, sectionRect);
                pitFireTeam.Log.LogInfo(
                    $"[STM] Stash fit (after): section={sectionRect.rect.width:F1}, viewport={(viewport != null ? viewport.rect.width : -1f):F1}, "
                    + $"content={(scroll?.content != null ? scroll.content.rect.width : -1f):F1}, gridRight={gridRight:F1}, "
                    + $"childLeft={childLeft:F1} ({leftName ?? "-"}), childRight={childRight:F1} ({rightName ?? "-"}), "
                    + $"windowInset=[{childLeft - windowRect.rect.xMin:F1},{windowRect.rect.xMax - childRight:F1}].");
            }
            catch (Exception ex)
            {
                // 겉모양 손질이다. 실패해도 편집기는 열린다.
                pitFireTeam.Log.LogWarning($"[STM] Could not fit the stash section: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // 스태시 패널의 **직계 자식들**이 창 좌표계에서 어디까지 뻗는가. 직계만 보는 이유는 손자까지
        // 훑으면 격자 안의 아이템 뷰 수백 개와 드래그용 임시 객체까지 잡혀 엉뚱한 값이 나오기 때문이다.
        private static void MeasureStmStashPanelEdges(
            SimpleStashPanel stashPanel,
            RectTransform windowRect,
            out float childLeft,
            out float childRight,
            out string leftName,
            out string rightName)
        {
            childLeft = float.MaxValue;
            childRight = float.MinValue;
            leftName = null;
            rightName = null;

            Vector3[] corners = new Vector3[4];
            foreach (Transform child in stashPanel.transform)
            {
                if (!(child is RectTransform childRect) || !child.gameObject.activeInHierarchy || childRect.rect.width <= 0f)
                {
                    continue;
                }

                childRect.GetWorldCorners(corners);
                // corners: 0=왼아래 1=왼위 2=오른위 3=오른아래.
                float left = windowRect.InverseTransformPoint(corners[0]).x;
                float right = windowRect.InverseTransformPoint(corners[2]).x;
                if (left < childLeft)
                {
                    childLeft = left;
                    leftName = child.name;
                }

                if (right > childRight)
                {
                    childRight = right;
                    rightName = child.name;
                }
            }
        }

        // 그려진 격자의 오른쪽 끝(칸 좌표계). 아무것도 못 재면 float.MinValue 다.
        private static float MeasureStmStashGridRight(SimpleStashPanel stashPanel, RectTransform sectionRect)
        {
            float gridRight = float.MinValue;
            EFT.UI.DragAndDrop.GridView[] grids = stashPanel.GetComponentsInChildren<EFT.UI.DragAndDrop.GridView>(false);
            if (grids == null)
            {
                return gridRight;
            }

            Vector3[] corners = new Vector3[4];
            foreach (EFT.UI.DragAndDrop.GridView grid in grids)
            {
                if (!(grid?.transform is RectTransform gridRect) || gridRect.rect.width <= 0f)
                {
                    continue;
                }

                gridRect.GetWorldCorners(corners);
                gridRight = Mathf.Max(gridRight, sectionRect.InverseTransformPoint(corners[2]).x);
            }

            return gridRight;
        }

        // 1px 미만의 남는 폭은 반올림 오차다 - 건드리면 격자가 잘릴 수 있다.
        private const float StmStashSectionSlackEpsilon = 2f;

        // 창 테두리와 패널 내용 사이에 둘 최소 여백. 칸의 원래 들여쓰기(16px)의 절반이다.
        private const float StmStashWindowEdgeMargin = 8f;

        // 한 번에 옮길 수 있는 한계. 이보다 큰 값이 나오면 재는 것이 틀린 것이므로(엉뚱한 자식을 잡았다)
        // 옮기지 않고 로그만 남긴다 - 편집기를 망가뜨리는 것보다 어긋난 채 두는 것이 낫다(불변식 2).
        private const float StmStashSectionMaxAdjust = 160f;

        // 편집기 프로필: 플레이어 프로필의 복제(외형·스킬의 껍데기)에, 인벤토리만 그 팀원의 장비와 **그 팀원의 스태시**로 바꾼다.
        // PIT 와 같은 InventoryDescriptor 경로를 쓰므로 아이템 id 가 보존된다 - 저장이 "어느 아이템이 움직였나" 를 id 로 말한다.
        private static bool TryCreateStmGearEditorProfile(
            ResultProfile profile,
            StmGearStashWire stash,
            EFT.IEftSession session,
            out Profile editorProfile,
            out InventoryController editorInventoryController)
        {
            editorProfile = null;
            editorInventoryController = null;

            try
            {
                StmGearEditSkippedCount = 0;
                if (!TryBuildStmEditorStash(stash, out EFT.InventoryLogic.Stash teammateStash, out string stashFailure))
                {
                    pitFireTeam.Log.LogWarning($"[STM] Teammate stash could not be built for the gear editor: {stashFailure}");
                    return false;
                }

                // 장비 쪽은 용병 편집기의 비밀 컨테이너 가리기(SanitizeLoadoutEditorEquipment)를 거치지 않는다: 그 규칙은 PIT 용병의
                // "현실 모드" 설정에 묶여 있고, 여기서 지우면 실제 프로필의 아이템이 편집기에서 사라진 것처럼 보인다.
                InventoryEquipment equipment = profile.Equipment.CloneItemWithSameId() as InventoryEquipment;
                Profile baseProfile = session?.Profile?.Clone();
                if (baseProfile?.Inventory == null || equipment == null)
                {
                    return false;
                }

                CaptureLoadoutEditorOriginalPinLocks(teammateStash);

                EFT.InventoryDescriptor inventoryDescriptor = new EFT.InventoryDescriptor(baseProfile.Inventory, EFT.FullySearchedSearchController.Instance)
                {
                    Equipment = EFT.ItemBinarySerializer.SerializeItem(equipment, null),
                    Stash = EFT.ItemBinarySerializer.SerializeItem(teammateStash, null)
                };

                baseProfile.Inventory = inventoryDescriptor.ToInventory();
                ApplyLoadoutEditorOriginalPinLocks(baseProfile.Inventory?.Stash);

                editorProfile = baseProfile;
                // 두 번째 인자는 **examined**(이 컨트롤러가 모든 아이템을 "살펴본 것" 으로 보는가)다.
                // PIT 가 false 를 주고 있었고, 그래서 EFT 의 조작 메뉴가 Inspect·Examine·Discard 셋만 남았다
                // (ItemContextInteractionsSwitcher.IsActive: 살펴보지 않은 아이템은 그 셋만 보여 준다).
                // 열기·장전·비우기·나누기가 메뉴에 없던 이유가 이것이다 - 플레이어 화면에서는 자기 백과사전에
                // 있는 것이라 다 보인다. 팀원의 물건도 플레이어가 보고 있는 것이므로 true 로 연다.
                // 이 값은 **읽는 쪽 판정**일 뿐 프로필에 아무것도 쓰지 않는다.
                editorInventoryController = new StmGearEditInventoryController(editorProfile, true);

                // 편집기의 스태시가 서버가 말한 그 팀원 스태시 루트인가 - 아니면 출처 칸을 믿을 수 없다.
                if (editorProfile.Inventory?.Equipment == null || editorProfile.Inventory.Stash == null
                    || !string.Equals(editorProfile.Inventory.Stash.Id, stash.StashRootId, StringComparison.OrdinalIgnoreCase))
                {
                    StmGearEditOpenFailure = StmEditorStashMismatchToast;
                    editorProfile = null;
                    editorInventoryController = null;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Failed to create the local profile teammate gear editor profile.");
                pitFireTeam.Log.LogError(ex);
                editorProfile = null;
                editorInventoryController = null;
                return false;
            }
        }

        // 서버의 평면 목록을 EFT 스태시 트리로 세운다. 루트가 목록에 없거나 루트 밖 아이템이 섞여 있으면 세우지 않는다(닫힌 쪽).
        // EFT 가 거부한 항목은 PIT 가 플레이어 스태시에서 하는 것과 같은 정책으로 건너뛴다 - 보이지 않을 뿐 서버의 것은 그대로다.
        private static bool TryBuildStmEditorStash(StmGearStashWire stash, out EFT.InventoryLogic.Stash teammateStash, out string failure)
        {
            teammateStash = null;
            failure = null;

            string rootId = stash.StashRootId;
            Dictionary<string, JsonType.FlatItem> byId = ToFlatItemDictionary(stash.Items);
            if (!byId.ContainsKey(rootId))
            {
                failure = "stash root was not in the item list";
                return false;
            }

            List<JsonType.FlatItem> ordered = ExpandFlatItemRoots(new[] { rootId }, byId);
            if (ordered.Count != byId.Count || byId.Count != stash.Items.Length)
            {
                failure = "item list had items outside the stash tree, duplicates, or items without an id";
                return false;
            }

            JsonType.FlatItem[] items = ordered.ToArray();
            EFT.ItemFactory.FlatItemsToResultTree tree = Singleton<EFT.ItemFactory>.Instance.FlatItemsToTree(items, false, null);
            if (tree.DeserializationErrors != null && tree.DeserializationErrors.Count > 0)
            {
                List<string> errorTexts = new List<string>();
                foreach (object error in tree.DeserializationErrors)
                {
                    errorTexts.Add(error?.ToString() ?? "(error text unavailable)");
                }

                items = SkipEftRejectedStashItems(items, errorTexts, rootId, out List<string> skippedIds);
                StmGearEditSkippedCount += skippedIds?.Count ?? 0;
                if (skippedIds != null && skippedIds.Count > 0)
                {
                    pitFireTeam.Log.LogWarning($"[STM] The gear editor could not load {skippedIds.Count} teammate stash item(s): {string.Join(", ", skippedIds)}");
                }
                tree = Singleton<EFT.ItemFactory>.Instance.FlatItemsToTree(items, false, null);
                if (tree.DeserializationErrors != null && tree.DeserializationErrors.Count > 0)
                {
                    failure = $"{tree.DeserializationErrors.Count} deserialization error(s) remained after skipping {skippedIds.Count} rejected item(s)";
                    return false;
                }
            }

            if (!tree.Items.TryGetValue(rootId, out Item root) || !(root is EFT.InventoryLogic.Stash builtStash))
            {
                failure = "stash root did not build into a stash";
                return false;
            }

            teammateStash = builtStash;
            return true;
        }

        // ---- 저장 ----

        // 저장의 결과는 **한 번** 정해진다 - 서버가 실제로 한 말에서(SaveOutcomePolicy) - 그리고 그 뒤의 어떤 단계도 다시
        // 분류하지 못한다. 알리기는 전달일 뿐이다: 전달이 실패하면 실패했다고 말할 뿐, 거절을 모름으로, 모름을 거절로 바꾸지 않는다.
        private enum StmSaveKind { NotSent, NoChange, Confirmed, Refused, Unknown }

        private sealed class StmSaveVerdict
        {
            public StmSaveKind Kind;
            public string Message;
            public int Saved;
            public string LayoutNote = string.Empty;
        }

        private static async void SaveStmGearEdit(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            if (StmGearEditSaving || StmGearEditOpening) return;
            StmGearEditSaving = true;
            SetLoadoutEditorBusy(true);
            try
            {
                StmSaveVerdict verdict = await DecideStmSaveAsync(profile, detail);
                DeliverStmSaveVerdict(verdict, screen, profile, inventoryController, session, playerModelWindow);
            }
            finally
            {
                StmGearEditSaving = false;
                SetLoadoutEditorBusy(false);
            }
        }

        // 결과를 정한다. 던지지 않는다: 요청 전의 예외는 NotSent(아무것도 보내지 않았다), 요청이 나간 뒤의 예외는 Unknown(서버가
        // 썼는지 모른다)이다. 알리는 일은 여기서 하지 않는다.
        private static async Task<StmSaveVerdict> DecideStmSaveAsync(ResultProfile profile, StmProfileDetailWire detail)
        {
            bool requestSent = false;
            try
            {
                CloseLoadoutEditorChildWindows();
                // 저장 중에는 닫을 수 없고(CloseStmGearEditor), 멈춘 것처럼 보이지 않게 말해 둔다. 거절되면 이 줄이 사유로 바뀐다.
                SetStmGearEditStatus(StmEditorSavingText);

                if (profile == null || detail == null || LoadoutEditorProfile?.Inventory?.Equipment == null || LoadoutEditorProfile.Inventory.Stash == null)
                {
                    return new StmSaveVerdict { Kind = StmSaveKind.NotSent, Message = StmEditorSaveFailedToast };
                }

                // 편집기의 **최종 트리 전체**를 보낸다. 조작의 종류를 적지 않으므로 나누기·합치기·
                // 탄박스 열기·버리기가 그대로 저장되고, 칸 위치도 함께 가므로 "저장되지 않는 변화" 가
                // 없다. 정당한 변화인지는 서버가 본다(ProfileInventoryApplyPolicy): 늘어나는 것·바깥에서
                // 들어온 종류·구조 손상·트리 손상을 거절하고, 줄어드는 것(버리기·탄박스 소비)은 허용한다.
                JsonType.FlatItem[] endEquipment = Singleton<EFT.ItemFactory>.Instance.TreeToFlatItems(new Item[] { LoadoutEditorProfile.Inventory.Equipment });
                JsonType.FlatItem[] endStash = Singleton<EFT.ItemFactory>.Instance.TreeToFlatItems(new Item[] { LoadoutEditorProfile.Inventory.Stash });
                if (endEquipment == null || endStash == null)
                {
                    return new StmSaveVerdict { Kind = StmSaveKind.NotSent, Message = StmEditorSaveFailedToast };
                }

                int changed = CountStmChangedItems(LoadoutEditorInitialEquipmentItems, LoadoutEditorInitialStashItems, endEquipment, endStash);
                if (changed == 0)
                {
                    return new StmSaveVerdict { Kind = StmSaveKind.NoChange };
                }

                string layoutNote = string.Empty;

                // 한 편집 세션 = 한 요청 = 서버의 쓰기 한 번.
                // 편집기가 **열릴 때 들고 있던** id 들도 함께 보낸다. 서버는 이 목록 밖의 손실을
                // 거절한다 - EFT 가 읽지 못해 편집기에 실리지 않은 아이템이 제출본에 없다는 이유로
                // 지워지는 것을 막는다(2026-11-03 실측: 그렇게 68개가 지워졌다).
                List<string> loadedItemIds = StmItemIds(LoadoutEditorInitialEquipmentItems, LoadoutEditorInitialStashItems);

                // 사라지는 아이템은 로그에만 남긴다. 한때 "한 번 더 누르세요" 로 막았는데, 탄 더미가
                // **합쳐지면** id 가 없어지는 것이 정상이라 평범한 편집마다 경고가 떴다(2026-11-03 사용자
                // 지적). 진짜 보호는 서버에 있다: 편집기가 들고 있지 않던 아이템은 애초에 지울 수 없다.
                HashSet<string> finalIds = new HashSet<string>(StmItemIds(endEquipment, endStash), StringComparer.Ordinal);
                List<string> losing = loadedItemIds.Where(id => !finalIds.Contains(id)).ToList();
                if (losing.Count > 0)
                {
                    pitFireTeam.Log.LogWarning($"[STM] The save removes {losing.Count} item id(s) (merges and opened ammo packs look the same here): {string.Join(", ", losing)}");
                }
                string body = JsonConvert.SerializeObject(new
                {
                    aid = profile.AccountId,
                    profileId = detail.ProfileId,
                    items = endEquipment.Concat(endStash).Where(item => item != null).ToList(),
                    loadedItemIds,
                });
                // 요청이 나간 뒤부터는 서버가 썼는지 우리가 모른다.
                requestSent = true;
                string json = await Task.Run(() => RequestHandler.PostJson(StmApplyInventoryRoute, body));

                SaveOutcome outcome = ClassifyStmSaveResponse(json);
                switch (outcome.Kind)
                {
                    case SaveOutcomeKind.Confirmed:
                        return new StmSaveVerdict { Kind = StmSaveKind.Confirmed, Saved = changed, LayoutNote = layoutNote };
                    case SaveOutcomeKind.Refused:
                        // 서버가 쓴 사유 그대로. 거절된 묶음은 아무것도 쓰지 않았으므로 편집기는 열어 둔다(고쳐서 다시 저장할 수 있다).
                        return new StmSaveVerdict { Kind = StmSaveKind.Refused, Message = outcome.Message };
                    default:
                        return new StmSaveVerdict { Kind = StmSaveKind.Unknown, Message = outcome.Message };
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Deciding the outcome of the profile teammate gear save threw.");
                pitFireTeam.Log.LogError(ex);
                return requestSent
                    ? new StmSaveVerdict { Kind = StmSaveKind.Unknown, Message = SaveOutcomePolicy.UnknownText }
                    : new StmSaveVerdict { Kind = StmSaveKind.NotSent, Message = StmEditorSaveFailedToast };
            }
        }

        // 응답 본문을 읽어 shared 정책에 넘긴다. 비었거나 JSON 이 아니거나 err 가 없으면 "읽지 못함" 이고 정책이 모름으로 정한다.
        private static SaveOutcome ClassifyStmSaveResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return SaveOutcomePolicy.Classify(false, null, null);
            }

            try
            {
                JToken root = JToken.Parse(json);
                JToken errToken = root["err"];
                int? err = errToken == null || errToken.Type == JTokenType.Null ? (int?)null : errToken.Value<int>();
                return SaveOutcomePolicy.Classify(true, err, root["errmsg"]?.ToString());
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] The gear save response could not be read: {ex.Message}");
                return SaveOutcomePolicy.Classify(false, null, null);
            }
        }

        // 정해진 결과를 플레이어에게 전한다. 전달이 던져도 결과는 바뀌지 않는다 - 정해진 말을 가장 단순한 길(토스트)로 한 번 더 낸다.
        private static void DeliverStmSaveVerdict(
            StmSaveVerdict verdict,
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            try
            {
                switch (verdict.Kind)
                {
                    case StmSaveKind.NotSent:
                    case StmSaveKind.Refused:
                        ReportStmGearEditFailure(verdict.Message, screen, profile);
                        break;
                    case StmSaveKind.Unknown:
                        // 불확실한 결과는 나중 화면에 거절되었다고 되살리지 않는다.
                        ReportStmGearEditFailure(verdict.Message, screen, profile, false);
                        break;
                    case StmSaveKind.NoChange:
                        TeardownStmGearEditor();
                        AddTeammateCreationFlow.ShowToast(StmEditorNoChangeToast + verdict.LayoutNote);
                        break;
                    case StmSaveKind.Confirmed:
                        // 서버가 쓰기를 확인했다. 아래 후처리의 실패는 "저장하지 못했습니다" 가 될 수 없다 - 쓰기는 일어났다.
                        try
                        {
                            RefreshStmDetailAfterSave(profile, inventoryController, session, playerModelWindow);
                            TeardownStmGearEditor();
                        }
                        catch (Exception postEx)
                        {
                            pitFireTeam.Log.LogError("[STM] The gear edit was saved, but cleaning up the editor afterwards failed.");
                            pitFireTeam.Log.LogError(postEx);
                            AddTeammateCreationFlow.ShowToast(StmEditorSavedButCleanupFailedToast);
                            break;
                        }

                        AddTeammateCreationFlow.ShowToast(string.Format(StmEditorSavedToastFormat, verdict.Saved) + verdict.LayoutNote);
                        break;
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError($"[STM] Delivering the gear save outcome ({verdict.Kind}) to the player failed; the outcome is unchanged.");
                pitFireTeam.Log.LogError(ex);
                try
                {
                    AddTeammateCreationFlow.ShowToast(StmFinalOutcomeText(verdict));
                }
                catch (Exception toastEx)
                {
                    pitFireTeam.Log.LogError(toastEx);
                }
            }
        }

        private static string StmFinalOutcomeText(StmSaveVerdict verdict)
        {
            switch (verdict.Kind)
            {
                case StmSaveKind.Confirmed: return string.Format(StmEditorSavedToastFormat, verdict.Saved);
                case StmSaveKind.NoChange: return StmEditorNoChangeToast;
                default: return verdict.Message;
            }
        }

        // 편집기 아이템을 shared 판정이 읽는 모양으로 줄인다. 장비와 스태시는 별개 트리지만 id 는 한 프로필 안에서 겹치지 않는다.
        // LayoutKey: 칸 위치만. 부모·슬롯이 같은데 위치만 다르면 "저장되지 않는 변화" 로 센다. upd 는 넣지 않는다 - EFT 가
        // 무기 등을 초기화하며 플레이어가 만지지 않은 아이템의 upd 를 바꿀 수 있어, 넣으면 안내가 헛되이 뜬다.
        // 저장할 것이 있는가, 몇 개나 달라졌는가. 아이템 **한 줄 전체**(자리·좌표·수량·내구도까지)를
        // 직렬화해 비교한다 - 자리만 보면 수량만 바뀐 편집(탄창 채우기)을 "바뀐 것이 없다" 로 읽는다.
        private static int CountStmChangedItems(
            JsonType.FlatItem[] beforeEquipment,
            JsonType.FlatItem[] beforeStash,
            JsonType.FlatItem[] afterEquipment,
            JsonType.FlatItem[] afterStash)
        {
            HashSet<string> before = StmItemFingerprints(beforeEquipment, beforeStash);
            HashSet<string> after = StmItemFingerprints(afterEquipment, afterStash);
            int changed = 0;
            foreach (string fingerprint in after)
            {
                if (!before.Contains(fingerprint)) changed++;
            }

            foreach (string fingerprint in before)
            {
                if (!after.Contains(fingerprint)) changed++;
            }

            return changed;
        }

        // 탄박스 열기를 편집기가 직접 처리한다(StmEditorAmmoBoxUnloadPatch 가 부른다).
        // true 를 돌려주면 EFT 의 원래 처리를 건너뛴다 - 그 처리는 빈 상자를 버리면서 이 편집기에서
        // 탄까지 잃는다(2026-11-03 실측). 편집기가 아니거나 탄박스가 아니면 false 로 EFT 에 맡긴다.
        internal static bool TryStmEditorUnloadAmmoBox(ItemContext itemContext, out Task handled)
        {
            handled = null;
            InventoryController controller = LoadoutEditorInventoryController;
            EFT.InventoryLogic.Stash stash = LoadoutEditorProfile?.Inventory?.Stash;
            if (controller == null || stash == null) return false;
            if (!(itemContext?.Item is AmmoBox box)) return false;

            handled = StmUnloadAmmoBoxAsync(box, controller, stash);
            return true;
        }

        // 탄을 한 더미씩 그 팀원 스태시로 옮긴다. 자리를 못 찾거나 적용이 실패하면 **거기서 멈춘다** -
        // 이미 옮긴 것은 그대로 두고(그것도 그 팀원의 것이다), 상자는 버리지 않는다. 아이템이 조용히
        // 사라지는 길을 만들지 않는 것이 이 함수의 전부다.
        private static async Task StmUnloadAmmoBoxAsync(AmmoBox box, InventoryController controller, EFT.InventoryLogic.Stash stash)
        {
            int moved = 0;
            try
            {
                CompoundItem[] targets = new CompoundItem[] { stash };
                while (box.Cartridges?.Last is Ammo ammo)
                {
                    var placement = ItemManipulator.QuickFindAppropriatePlace(
                        ammo, controller, targets, ItemManipulator.EMoveItemOrder.UnloadAmmo, simulate: true);
                    if (placement.Failed)
                    {
                        AddTeammateCreationFlow.ShowToast(StmEditorAmmoBoxNoRoomToast);
                        return;
                    }

                    var result = await controller.TryRunNetworkTransaction(placement);
                    if (result.Failed)
                    {
                        pitFireTeam.Log.LogWarning($"[STM] Unloading an ammo box stopped after {moved} stack(s): {result.Error}");
                        AddTeammateCreationFlow.ShowToast(StmEditorAmmoBoxNoRoomToast);
                        return;
                    }

                    moved++;
                }

                if (moved == 0) return;

                // 탄이 **정말로** 상자 밖으로 나갔는지 확인한 뒤에만 상자를 치운다. EFT 의 ThrowItem 은
                // 남은 내용물까지 함께 지우는 Discard 라(역컴파일), 확인 없이 부르면 바로 그 사고가 된다.
                if (box.Cartridges?.Last != null)
                {
                    AddTeammateCreationFlow.ShowToast(StmEditorAmmoBoxStuckToast);
                    return;
                }

                controller.ThrowItem(box, false, null);
                AddTeammateCreationFlow.ShowToast(StmEditorAmmoBoxOpenedToast);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Unloading an ammo box in the gear editor threw.");
                pitFireTeam.Log.LogError(ex);
                AddTeammateCreationFlow.ShowToast(StmEditorAmmoBoxNoRoomToast);
            }
        }

        // 편집기가 들고 있던 아이템 id 들(열릴 때의 상태 그대로).
        private static List<string> StmItemIds(params JsonType.FlatItem[][] groups)
        {
            List<string> ids = new List<string>();
            foreach (JsonType.FlatItem[] group in groups)
            {
                if (group == null) continue;
                foreach (JsonType.FlatItem item in group)
                {
                    if (item == null) continue;
                    string id = item._id.ToString();
                    if (!string.IsNullOrWhiteSpace(id)) ids.Add(id);
                }
            }

            return ids;
        }

        private static HashSet<string> StmItemFingerprints(params JsonType.FlatItem[][] groups)
        {
            HashSet<string> fingerprints = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonType.FlatItem[] group in groups)
            {
                if (group == null) continue;
                foreach (JsonType.FlatItem item in group)
                {
                    if (item == null) continue;
                    try
                    {
                        fingerprints.Add(JsonConvert.SerializeObject(item));
                    }
                    catch (Exception ex)
                    {
                        // 읽지 못한 아이템은 "달라진 것" 쪽으로 센다 - 조용히 같다고 하지 않는다.
                        pitFireTeam.Log.LogWarning($"[STM] An editor item could not be fingerprinted: {ex.Message}");
                        fingerprints.Add(Guid.NewGuid().ToString());
                    }
                }
            }

            return fingerprints;
        }

        private static List<EditorItemState> ToStmEditorStates(JsonType.FlatItem[] equipment, JsonType.FlatItem[] stash)
        {
            if (equipment == null || stash == null) return null;
            List<EditorItemState> states = new List<EditorItemState>(equipment.Length + stash.Length);
            foreach (JsonType.FlatItem item in equipment.Concat(stash))
            {
                if (item == null) continue;
                states.Add(new EditorItemState(
                    item._id.ToString(),
                    GetParentId(item),
                    item.slotId,
                    StmTokenKey(item.location)));
            }

            return states;
        }

        private static string StmTokenKey(UnparsedData data)
        {
            JToken token = data?.JToken;
            return IsNullOrEmptyJson(token) ? string.Empty : token.ToString(Formatting.None);
        }

        // 거절·실패: 토스트와 편집기 안의 줄 둘 다. 토스트는 금방 사라지고, 사유는 슬롯이나 아이템 이름을 담고 있어 읽을 시간이 필요하다.
        //
        // 저장 결과는 편집기가 아직 있는지에 기대지 않는다. PIT 화면의 숨김 처리 등 이 파일 밖의 길이 저장 요청이 나가 있는 동안
        // 편집기를 치울 수 있고, 그 뒤에 도착한 거절 사유는 상태 줄(StmGearEditStatusLabel)이 이미 없다. 그때는 사유를
        // **메시지로만** 남긴다(화면 참조가 아니라 문자열): 화면이 살아 있고 보이면 상세 화면의 안내 상자에 바로 올리고,
        // 아니면 이 팀원 앞으로 대기시켜 다음에 이 팀원의 상세 화면이 열릴 때 그 안내 상자가 말한다.
        private static void ReportStmGearEditFailure(string message, OtherPlayerProfileScreen screen, ResultProfile profile, bool remember = true)
        {
            bool editorAlive = StmGearEditStatusLabel != null;
            SetStmGearEditStatus(message);

            if (!editorAlive)
            {
                // 사유를 지키는 일은 UI 호출이 성공하는지에 기대지 않는다: 먼저 기억하고, 그다음에 보여 주고, 마지막에 토스트를 낸다.
                // 불확실한 결과(저장되었을 수 있음)는 나중 화면에 거절되었다고 되살리지 않는다(remember == false).
                if (remember) RememberStmPendingGearRefusal(message, profile?.AccountId);

                // 화면이 재사용돼 다른 팀원의 상세일 수 있다 - 지금 화면의 팀원이 이 저장의 주인일 때만 올린다.
                if (screen != null && screen.gameObject.activeInHierarchy
                    && PendingRefusalPolicy.SameOwner(profile?.AccountId, StmShownDetailAccountId))
                {
                    try
                    {
                        CloseStmDetail();
                        ShowStmBanner(screen, (remember ? StmRefusalPrefix : string.Empty) + message, null, null);
                        // 안내 상자가 그려졌다 - 이제야 대기를 비운다(방금 기억한 그 사유일 때만).
                        if (remember) ClearStmPendingGearRefusalIf(message);
                    }
                    catch (Exception ex)
                    {
                        // 그리기가 실패했다. 대기에 남겨 두었으므로 이 팀원의 다음 화면이 말한다.
                        pitFireTeam.Log.LogWarning($"[STM] The gear-save refusal could not be drawn on the live detail screen; it stays pending for the next one: {ex.Message}");
                    }
                }
            }

            AddTeammateCreationFlow.ShowToast(message);
        }

        // 가장 새 것만 남긴다. 아직 읽히지 않은 이전 사유를 버릴 때는 조용히 버리지 않고 로그에 주인과 함께 남긴다.
        private static void RememberStmPendingGearRefusal(string message, string accountId)
        {
            if (StmPendingGearRefusalMessage != null)
            {
                pitFireTeam.Log.LogWarning($"[STM] An unread gear-save refusal for teammate '{StmPendingGearRefusalAccountId}' was replaced by a newer one for '{accountId}'. Dropped reason: {StmPendingGearRefusalMessage}");
            }

            StmPendingGearRefusalAccountId = accountId;
            StmPendingGearRefusalMessage = message;
            StmPendingGearRefusalAtUtc = DateTime.UtcNow;
        }

        private static void ClearStmPendingGearRefusalIf(string message)
        {
            if (!string.Equals(StmPendingGearRefusalMessage, message, StringComparison.Ordinal)) return;
            StmPendingGearRefusalMessage = null;
            StmPendingGearRefusalAccountId = null;
        }

        // 대기 중인 거절 사유를 **들여다본다** - 비우지 않는다. 비우는 것은 사유가 실제로 화면에 그려진 뒤의 일이다
        // (ShowStmBannerWithPending). 이 팀원 것이 아니면 null 이고 대기는 그대로다(그 팀원의 화면이 기다린다). 만료됐으면 로그를
        // 남기고 버린다(다른 세션의 잔재일 수 있다).
        private static string PeekStmPendingGearRefusal(string accountId)
        {
            string message = StmPendingGearRefusalMessage;
            if (message == null) return null;

            // 주인·만료 판정은 shared 의 PendingRefusalPolicy 다.
            PendingRefusalVerdict verdict = PendingRefusalPolicy.Judge(StmPendingGearRefusalAccountId, StmPendingGearRefusalAtUtc, accountId, DateTime.UtcNow);
            if (verdict == PendingRefusalVerdict.NotOwner) return null;

            if (verdict == PendingRefusalVerdict.Expired)
            {
                pitFireTeam.Log.LogWarning($"[STM] An expired gear-save refusal for teammate '{StmPendingGearRefusalAccountId}' was dropped unread: {message}");
                StmPendingGearRefusalMessage = null;
                StmPendingGearRefusalAccountId = null;
                return null;
            }

            return message;
        }

        // 안내 상자를 그리고, 대기 중이던 사유가 있으면 앞머리에 붙인다. 대기는 ShowStmBanner 가 **던지지 않고 돌아온 뒤에만** 비운다 -
        // 그리기가 던지면 사유는 대기에 남아 이 팀원의 다음 화면이 말한다.
        private static void ShowStmBannerWithPending(
            OtherPlayerProfileScreen screen,
            string baseText,
            string accountId,
            StmProfileDetailWire closable,
            StmProfileDetailWire discardable)
        {
            string pending = PeekStmPendingGearRefusal(accountId);
            string text = pending == null ? baseText : StmRefusalPrefix + pending + Environment.NewLine + Environment.NewLine + baseText;
            ShowStmBanner(screen, text, closable, discardable);
            if (pending != null) ClearStmPendingGearRefusalIf(pending);
        }

        private static void SetStmGearEditStatus(string message)
        {
            if (StmGearEditStatusLabel != null)
            {
                StmGearEditStatusLabel.text = message ?? string.Empty;
            }
        }

        // 저장이 서버에 박혔다. 화면 뒤의 장비 표시가 낡은 채 남지 않게 프로필을 다시 읽어 모델을 다시 그린다(용병 편집 뒤에
        // PIT 가 하는 것과 같은 함수 - 이 조회는 STM 이 프로필 팀원에게 먼저 답한다). 실패해도 저장은 이미 끝났다.
        private static void RefreshStmDetailAfterSave(
            ResultProfile profile,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            try
            {
                if (profile == null || inventoryController == null || session == null || playerModelWindow == null) return;
                RefreshPlayerVisualization(profile, inventoryController, session, playerModelWindow);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] The detail screen could not be refreshed after the gear edit was saved: {ex.Message}");
            }
        }

        // ---- 닫기 ----

        // 취소·X·저장 뒤의 정리. PIT 의 CloseLoadoutEditorOverlay 가 패널·컨텍스트·전역을 치우고(아이템 UI 컨텍스트를 플레이어
        // 프로필로 되돌리는 것 포함), 그 뒤에 이 편집기가 채운 전역을 비운다.
        // X·취소 버튼이 부르는 길. 저장 요청이 나가 있는 동안은 닫지 않는다 - 닫으면 뒤늦게 오는 거절 사유가 상태 줄 없이 토스트로만
        // 간다. 저장 코드와 열기 실패 정리는 가드 없는 TeardownStmGearEditor 를 쓴다.
        private static void CloseStmGearEditor()
        {
            if (StmGearEditSaving) return;
            TeardownStmGearEditor();
        }

        private static void TeardownStmGearEditor()
        {
            // 검색창이 편집기보다 오래 살아 사라진 지역 트리를 가리키지 않게 먼저 닫는다.
            CloseStmStashSearch();
            CloseLoadoutEditorOverlay();
            ActiveProfileSession = null;
            ActiveProfileInventoryController = null;
            StmGearEditStatusLabel = null;
        }
    }
}
