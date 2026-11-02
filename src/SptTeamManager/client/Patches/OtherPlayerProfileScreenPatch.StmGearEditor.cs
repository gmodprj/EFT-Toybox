using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pitTeam.Modules;
using pitTeam.Shared;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ResultProfile = EFT.OtherPlayerProfile;

namespace pitTeam.Patches
{
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
    //      여기서는 편집기의 시작 상태와 끝 상태를 비교해(GearEditBatchPlanner) "이 아이템을 이 부모의 이 슬롯으로" 건 목록을
    //      만들고, /stm/members/edit-gear 에 **한 번에** 보낸다. 최종 집합은 서버가 자기 프로필에서 만든다.
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
        private const string StmEditGearRoute = "/stm/members/edit-gear";

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
        private const string StmEditorSavedToastFormat = "장비를 저장했습니다({0}건).";
        private const string StmEditorLayoutNoteFormat = " 칸 위치나 상태만 바꾼 {0}개는 저장되지 않습니다.";

        // 편집기를 여는 중(스태시 요청)과 저장 중. 둘 다 한 번에 하나만 - 중복 요청이 같은 편집을 두 번 보내지 않게 한다.
        private static bool StmGearEditOpening;
        private static bool StmGearEditSaving;
        // 저장 버튼 곁에 서버의 거절 사유를 남겨 두는 줄. 토스트는 금방 사라지므로 읽을 시간이 필요한 사유는 여기에도 둔다.
        private static CustomTextMeshProUGUI StmGearEditStatusLabel;

        // ---- 입구 ----

        // 상세 화면의 입구. 서버가 열어도 된다고 한 팀원에게만 버튼을 얹는다(canEditGear). 열 수 없으면 이유를 말하는 안내 상자다.
        // 판정은 GearEditEntryPolicy 가 했고 여기는 그리기뿐이다. 값이 없는 응답(옛 서버)은 열 수 없음으로 읽힌다.
        private static void ShowStmGearEditEntry(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            if (!detail.CanEditGear)
            {
                ShowStmBanner(screen, GearEditEntryPolicy.NoticeWhenNotEditable(detail.EditNotice), null, null);
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
            rect.anchoredPosition = new Vector2(0f, -96f);
            rect.sizeDelta = new Vector2(300f, 48f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            Image background = root.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0f);
            background.raycastTarget = false;

            DefaultUIButton button = CreateOverlayButton(buttonTemplate, root.transform, new Vector2(0f, 4f), new Vector2(300f, 40f));
            button.name = "pitFireTeam_StmGearEditButton";
            button.SetRawText(StmEditEntryText, 20);
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(() => OpenStmGearEditor(screen, profile, detail, inventoryController, session, playerModelWindow));

            StmDetailRoot = root;
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

                if (!TryShowStmGearEditorOverlay(screen, profile, detail, stash, inventoryController, session, playerModelWindow))
                {
                    AddTeammateCreationFlow.ShowToast(StmEditorOpenFailedToast);
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Opening the profile teammate gear editor failed.");
                pitFireTeam.Log.LogError(ex);
                CloseStmGearEditor();
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

            CreateOverlayText(
                "pitFireTeam_LoadoutEditorSubtitle",
                panel.transform,
                new Vector2(28f, -62f),
                new Vector2(-28f, -98f),
                TextAlignmentOptions.MidlineLeft,
                StmEditorSubtitle,
                17f,
                new Color(0.67f, 0.67f, 0.64f, 1f));

            RectTransform leftSection = CreateLoadoutEditorSection(
                panel.transform,
                "pitFireTeam_StmTeammateStashSection",
                string.Format(StmEditorStashTitleFormat, nickname),
                new Vector2(0f, 0f),
                new Vector2(0.5f, 1f),
                new Vector2(16f, 64f),
                new Vector2(-2f, -104f));

            RectTransform rightSection = CreateLoadoutEditorSection(
                panel.transform,
                "pitFireTeam_BotInventorySection",
                string.Format(StmEditorGearTitleFormat, nickname),
                new Vector2(0.5f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 64f),
                new Vector2(-10f, -104f));

            if (!TryBuildStmGearEditorPanels(profile, stash, leftSection, rightSection, session))
            {
                // 패널을 세우지 못했다. 빈 겉틀로 저장 버튼을 두지 않고 닫는다.
                CloseStmGearEditor();
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
            saveButton.OnClick.AddListener(() => SaveStmGearEdit(profile, detail, inventoryController, session, playerModelWindow));
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

            TryBuildLoadoutEditorStashPanel(leftSection, editorProfile, editorInventoryController);
            TryBuildLoadoutEditorFollowerPanel(profile, rightSection, itemUiContext, editorProfile, editorInventoryController);

            // 패널을 세우는 동안 아이템 컴포넌트 상태(특히 무기 부착물)가 초기화될 수 있다. 플레이어가 만지기 전의 이 시점이
            // "바뀌었는가" 를 따질 첫 안정된 기준이다(PIT 편집기와 같은 이유).
            try
            {
                CaptureLoadoutEditorInitialState(editorProfile);
            }
            catch (Exception ex)
            {
                // 기준이 없으면 비교할 수 없다 - 이 편집기는 저장할 수 없다.
                pitFireTeam.Log.LogError($"[STM] Could not capture the gear editor baseline; the editor is not opened: {ex.Message}");
                return false;
            }

            return LoadoutEditorInitialEquipmentItems != null && LoadoutEditorInitialStashItems != null;
        }

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
                editorInventoryController = new InventoryController(editorProfile, false);
                return editorProfile.Inventory?.Equipment != null && editorProfile.Inventory.Stash != null;
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

        private static async void SaveStmGearEdit(
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
                CloseLoadoutEditorChildWindows();
                SetStmGearEditStatus(string.Empty);

                if (profile == null || detail == null || LoadoutEditorProfile?.Inventory?.Equipment == null || LoadoutEditorProfile.Inventory.Stash == null)
                {
                    ReportStmGearEditFailure(StmEditorSaveFailedToast);
                    return;
                }

                // 시작 상태와 끝 상태 -> 건 목록. 판정과 순서는 GearEditBatchPlanner 의 것이다.
                JsonType.FlatItem[] endEquipment = Singleton<EFT.ItemFactory>.Instance.TreeToFlatItems(new Item[] { LoadoutEditorProfile.Inventory.Equipment });
                JsonType.FlatItem[] endStash = Singleton<EFT.ItemFactory>.Instance.TreeToFlatItems(new Item[] { LoadoutEditorProfile.Inventory.Stash });
                GearEditPlan plan = GearEditBatchPlanner.Plan(
                    ToStmEditorStates(LoadoutEditorInitialEquipmentItems, LoadoutEditorInitialStashItems),
                    ToStmEditorStates(endEquipment, endStash));

                if (!plan.Ok)
                {
                    ReportStmGearEditFailure(plan.Reason);
                    return;
                }

                string layoutNote = plan.LayoutOnlyCount > 0 ? string.Format(StmEditorLayoutNoteFormat, plan.LayoutOnlyCount) : string.Empty;
                if (plan.Edits.Count == 0)
                {
                    CloseStmGearEditor();
                    AddTeammateCreationFlow.ShowToast(StmEditorNoChangeToast + layoutNote);
                    return;
                }

                // 한 편집 세션 = 한 요청 = 서버의 쓰기 한 번. 아이템마다 따로 보내지 않는다.
                string body = JsonConvert.SerializeObject(new
                {
                    aid = profile.AccountId,
                    profileId = detail.ProfileId,
                    edits = plan.Edits.Select(edit => new
                    {
                        itemId = edit.ItemId,
                        targetSlotId = edit.TargetSlotId,
                        targetParentId = edit.TargetParentId,
                    }).ToList(),
                });
                string json = await Task.Run(() => RequestHandler.PostJson(StmEditGearRoute, body));
                string failure = ReadStmCloseFailure(json, StmEditorSaveFailedToast);
                if (failure != null)
                {
                    // 서버가 쓴 사유 그대로. 거절된 묶음은 아무것도 쓰지 않았으므로 편집기는 열어 둔다(고쳐서 다시 저장할 수 있다).
                    ReportStmGearEditFailure(failure);
                    return;
                }

                int saved = plan.Edits.Count;
                RefreshStmDetailAfterSave(profile, inventoryController, session, playerModelWindow);
                CloseStmGearEditor();
                AddTeammateCreationFlow.ShowToast(string.Format(StmEditorSavedToastFormat, saved) + layoutNote);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Saving the profile teammate gear edit failed.");
                pitFireTeam.Log.LogError(ex);
                ReportStmGearEditFailure(StmEditorSaveFailedToast);
            }
            finally
            {
                StmGearEditSaving = false;
                SetLoadoutEditorBusy(false);
            }
        }

        // 편집기 아이템을 shared 판정이 읽는 모양으로 줄인다. 장비와 스태시는 별개 트리지만 id 는 한 프로필 안에서 겹치지 않는다.
        // LayoutKey: 이 경로가 저장하지 못하는 속성(칸 위치, upd 상태)을 문자열로 바꾼 것.
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
                    StmTokenKey(item.location) + "|" + StmTokenKey(item.upd)));
            }

            return states;
        }

        private static string StmTokenKey(UnparsedData data)
        {
            JToken token = data?.JToken;
            return IsNullOrEmptyJson(token) ? string.Empty : token.ToString(Formatting.None);
        }

        // 거절·실패: 토스트와 편집기 안의 줄 둘 다. 토스트는 금방 사라지고, 사유는 슬롯이나 아이템 이름을 담고 있어 읽을 시간이 필요하다.
        private static void ReportStmGearEditFailure(string message)
        {
            SetStmGearEditStatus(message);
            AddTeammateCreationFlow.ShowToast(message);
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
        private static void CloseStmGearEditor()
        {
            CloseLoadoutEditorOverlay();
            ActiveProfileSession = null;
            ActiveProfileInventoryController = null;
            StmGearEditStatusLabel = null;
        }
    }
}
