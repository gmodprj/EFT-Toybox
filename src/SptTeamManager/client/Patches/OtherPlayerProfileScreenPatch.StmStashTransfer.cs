using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Reflection;
using EFT.UI;
using Newtonsoft.Json;
using pitTeam.Modules;
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
    // 계획 6 B3 — 플레이어 스태시 ↔ 프로필 팀원 스태시 교환 화면(요청 2).
    //
    // **별개 화면이다**(설계 §6-5). 장비 편집기에 섞지 않는다 — 편집기는 한 프로필 **안의** 이동이고
    // 이것은 프로필 **둘 사이의** 전송이라, 지켜야 하는 것이 다르다(저널·총량 보존·id 충돌).
    //
    // 이 1차 화면은 **목록과 버튼**이다. 격자에 끼워 넣고 드래그로 건네는 모양은 다음 단계로 둔다:
    //   · 두 프로필의 격자를 한 창에 띄우면 EFT 의 드래그가 **자기 InventoryController 안에서** 옮기려
    //     들고, 그것을 가로채는 자리가 편집기 하나(1,800줄)만큼 더 필요하다.
    //   · 목록형은 그 자리를 하나도 열지 않고 **서버의 전송 경로를 그대로** 쓴다. 되는지 먼저 보고
    //     모양은 그 뒤에 입힌다. 되지도 않는 것을 예쁘게 만들지 않는다.
    //
    // 화면이 하는 일은 셋뿐이다: 양쪽 스태시를 받아 그리고(`/stm/members/stash-pair`), 고른 것을
    // 보내고(`/stm/members/stash-transfer`), 결과를 다시 받아 그린다. **판정은 전부 서버**다.
    internal partial class OtherPlayerProfileScreenPatch
    {
        private const string StmStashPairRoute = "/stm/members/stash-pair";
        private const string StmStashTransferRoute = "/stm/members/stash-transfer";

        private const string StmTransferButtonText = "스태시 교환";
        private const string StmTransferTitleFormat = "스태시 교환 — {0}";
        private const string StmTransferSubtitle =
            "반대쪽 스태시로 끌어다 놓으면 옮겨집니다. 여럿을 고른 채 끌면 한 번에 갑니다. 스태시 바닥에 놓인 것만 건넬 수 있고, 가방을 건네면 안에 든 것은 따라갑니다.";
        private const string StmTransferOpenFailedToast = "스태시 교환 화면을 열지 못했습니다";
        private const string StmTransferEmptySide = "(비어 있음)";
        private const string StmTransferNothingPicked = "건넬 것을 먼저 고르세요";

        // 한 쪽에 한 번에 그리는 줄 수. 넘치면 쪽을 넘긴다 — 스크롤 부품을 새로 세우지 않는다.
        private const int StmTransferRowsPerPage = 12;
        private const float StmTransferRowHeight = 26f;

        private static GameObject StmTransferOverlayRoot;
        private static RectTransform StmTransferButtonRoot;
        private static bool StmTransferBusy;

        private static StmStashPairWire StmTransferPair;
        private static string StmTransferAid;
        private static string StmTransferProfileId;

        private static readonly HashSet<string> StmTransferPickedLeft = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> StmTransferPickedRight = new HashSet<string>(StringComparer.Ordinal);
        private static int StmTransferPageLeft;
        private static int StmTransferPageRight;

        private static Transform StmTransferLeftList;
        private static Transform StmTransferRightList;

        // ---- 격자 패널(B3-b) ----
        // 한 쪽당 (스태시 트리 + 프로필 + 컨트롤러 + 패널) 한 벌. 장비 편집기가 팀원 스태시에 쓰는
        // 그 길을 **두 번** 쓴다 — PIT 의 패널 빌더는 바깥에서 받은 프로필/컨트롤러로 그리므로
        // 두 벌을 따로 세울 수 있다. 다만 그 함수가 끝에 LoadoutEditorStashPanel(전역)에 써 두므로
        // 부른 **직후에 받아 두고 전역은 비운다** — 편집기의 정리 코드가 우리 패널을 건드리지 않게.

        // 끌어다 놓은 그 한 개와 그 자리. 서버가 **그 자리에 그대로** 놓는다(나머지는 빈 자리를 찾는다).
        private static string StmTransferAnchorId;
        private static string StmTransferAnchorGrid;
        private static int StmTransferAnchorX = -1;
        private static int StmTransferAnchorY = -1;
        private static int StmTransferAnchorRotation;
        private static bool StmTransferGridsReady;
        private static SimpleStashPanel StmTransferLeftPanel;
        private static SimpleStashPanel StmTransferRightPanel;
        private static Profile StmTransferLeftProfile;
        private static Profile StmTransferRightProfile;
        private static InventoryController StmTransferLeftController;
        private static InventoryController StmTransferRightController;
        private static CustomTextMeshProUGUI StmTransferLeftHeader;
        private static CustomTextMeshProUGUI StmTransferRightHeader;
        private static CustomTextMeshProUGUI StmTransferStatusLabel;

        internal class StmStashEntryWire
        {
            public string Id { get; set; }
            public string Tpl { get; set; }
            public string Name { get; set; }
            public int Stack { get; set; }
            public int Contained { get; set; }
        }

        internal class StmStashSideWire
        {
            public string ProfileId { get; set; }
            public string Nickname { get; set; }
            public string StashRootId { get; set; }
            public StmStashEntryWire[] Items { get; set; }

            // 격자를 세울 재료. 장비 편집기가 팀원 스태시를 세울 때 받는 것과 같은 평면 목록이다.
            public JsonType.FlatItem[] Tree { get; set; }
        }

        internal class StmStashPairWire
        {
            public StmStashSideWire Player { get; set; }
            public StmStashSideWire Teammate { get; set; }
        }

        internal class StmStashTransferResultWire
        {
            public int Picked { get; set; }
            public int Moved { get; set; }
            public bool ToTeammate { get; set; }
        }

        // 상세 화면의 조작 줄에 한 칸을 더 단다. 꾸러미 버튼 둘(rowOffset 2·3) 다음이다.
        private static void CreateStmStashTransferButton(
            OtherPlayerProfileScreen screen,
            RectTransform loadoutSelector,
            Transform parent,
            ResultProfile profile,
            StmProfileDetailWire detail)
        {
            try
            {
                if (screen == null || loadoutSelector == null || parent == null || profile == null || detail == null) return;

                StmTransferButtonRoot = CreateStmKitRowButton(
                    screen, loadoutSelector, parent, detail,
                    StmTransferButtonRoot, rowOffset: 4,
                    name: "pitFireTeam_StmStashTransferButton",
                    text: StmTransferButtonText,
                    onClick: () => OpenStmStashTransfer(screen, profile, detail));
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 스태시 교환 버튼을 만들다 실패했다. 나머지 화면은 그대로다.");
                pitFireTeam.Log.LogError(ex);
            }
        }

        private static async void OpenStmStashTransfer(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail)
        {
            if (StmTransferBusy || StmTransferOverlayRoot != null) return;
            StmTransferBusy = true;
            try
            {
                if (screen == null || profile == null || detail == null || string.IsNullOrWhiteSpace(detail.ProfileId))
                {
                    AddTeammateCreationFlow.ShowToast(StmTransferOpenFailedToast);
                    return;
                }

                StmTransferAid = profile.AccountId;
                StmTransferProfileId = detail.ProfileId;
                StmTransferPickedLeft.Clear();
                StmTransferPickedRight.Clear();
                StmTransferPageLeft = 0;
                StmTransferPageRight = 0;

                if (!await LoadStmTransferPairAsync()) return;

                if (!TryShowStmTransferOverlay(screen))
                {
                    AddTeammateCreationFlow.ShowToast(StmTransferOpenFailedToast);
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 스태시 교환 화면을 열다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                CloseStmTransferOverlay();
                AddTeammateCreationFlow.ShowToast(ex.Message ?? StmTransferOpenFailedToast);
            }
            finally
            {
                StmTransferBusy = false;
            }
        }

        // 양쪽 스태시를 받아 온다. 실패는 **토스트로 말하고 아무것도 그리지 않는다** — 빈 목록을
        // 그리면 "스태시가 비었다" 로 읽혀 사람이 틀린 판단을 한다.
        private static async Task<bool> LoadStmTransferPairAsync()
        {
            string body = JsonConvert.SerializeObject(new { aid = StmTransferAid, profileId = StmTransferProfileId });
            string json = await Task.Run(() => RequestHandler.PostJson(StmStashPairRoute, body));

            string failure = ReadStmCloseFailure(json, StmTransferOpenFailedToast);
            if (failure != null)
            {
                AddTeammateCreationFlow.ShowToast(failure);
                return false;
            }

            StmStashPairWire pair = DeserializeBodySuccess<StmStashPairWire>(json)?.data;
            if (pair?.Player == null || pair.Teammate == null)
            {
                AddTeammateCreationFlow.ShowToast(StmTransferOpenFailedToast);
                return false;
            }

            StmTransferPair = pair;
            return true;
        }

        private static bool TryShowStmTransferOverlay(OtherPlayerProfileScreen screen)
        {
            // **자료를 지우지 않고** 남은 겉틀만 치운다. 여기서 CloseStmTransferOverlay 를 부르면
            // 그 정리가 StmTransferPair 까지 비워, 바로 아래에서 읽을 때 NullReference 가 난다
            // (2026-11-07 인게임 실측: 버튼을 누를 때마다 "Object reference not set" 이 떴다.
            //  IL 위치 0x30e 를 역어셈블해 이 줄로 좁혔다).
            DestroyStmTransferOverlayRoot();

            GameObject overlayRoot = new GameObject("pitFireTeam_StmStashTransferOverlay", typeof(RectTransform), typeof(Image));
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

            StmTransferOverlayRoot = overlayRoot;

            GameObject panel = new GameObject("pitFireTeam_StmStashTransferPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlayRoot.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            // 좌우에 **인게임 스태시 격자 두 벌**이 들어간다. 격자는 칸 수가 고정이라 폭이 줄지
            // 않는다 — 창이 좁으면 칸을 넘어 옆 격자 위로 겹쳐 그려진다(2026-11-07 인게임 실측).
            // 그래서 창을 화면 거의 전체로 벌린다. 목록형(1차)의 280 은 격자에는 턱없이 좁았다.
            panelRect.offsetMin = new Vector2(24f, 80f);
            panelRect.offsetMax = new Vector2(-24f, -80f);
            panelRect.localScale = Vector3.one;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.88f);
            panelImage.raycastTarget = true;

            // 머리줄 — 편집기와 같은 모양(제목 + 닫기 + 끌어 옮기기).
            GameObject header = new GameObject("pitFireTeam_StmStashTransferHeader", typeof(RectTransform), typeof(Image));
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
                "pitFireTeam_StmStashTransferTitle", header.transform,
                new Vector2(18f, 0f), new Vector2(-54f, 0f),
                TextAlignmentOptions.MidlineLeft,
                string.Format(StmTransferTitleFormat, StmTransferPair.Teammate.Nickname ?? string.Empty),
                20f, new Color(0.87f, 0.87f, 0.84f, 1f));

            Button closeButton = CreateWindowCloseButton(header.transform);
            if (closeButton.transform is RectTransform closeRect)
            {
                closeRect.anchorMin = new Vector2(1f, 0.5f);
                closeRect.anchorMax = new Vector2(1f, 0.5f);
                closeRect.pivot = new Vector2(1f, 0.5f);
                closeRect.anchoredPosition = new Vector2(-6f, 0f);
            }

            closeButton.onClick.AddListener(new UnityAction(CloseStmTransferOverlay));

            // 안내 줄. **앵커를 직접 준다** — CreateOverlayText 는 전체 늘이기로 고정하므로 그대로 두면
            // 글씨가 창 한가운데를 가로지른다(2026-11-04 실측, LoadoutUi 주석의 그 함정이다).
            CustomTextMeshProUGUI subtitle = CreateOverlayText(
                "pitFireTeam_StmStashTransferSubtitle", panel.transform,
                Vector2.zero, Vector2.zero,
                TextAlignmentOptions.MidlineLeft, StmTransferSubtitle,
                16f, new Color(0.67f, 0.67f, 0.64f, 1f));
            AnchorStmTransferBand(subtitle.transform as RectTransform, top: -40f, height: 26f, leftPad: 18f, rightPad: 18f);

            StmTransferStatusLabel = CreateOverlayText(
                "pitFireTeam_StmStashTransferStatus", panel.transform,
                Vector2.zero, Vector2.zero,
                TextAlignmentOptions.MidlineLeft, string.Empty,
                16f, new Color(0.85f, 0.78f, 0.55f, 1f));
            AnchorStmTransferBand(StmTransferStatusLabel.transform as RectTransform, top: -66f, height: 24f, leftPad: 18f, rightPad: 18f);

            // 두 칸. 왼쪽이 나, 오른쪽이 팀원이다.
            StmTransferLeftHeader = CreateStmTransferColumnHeader(panel.transform, "Left", left: true);
            StmTransferRightHeader = CreateStmTransferColumnHeader(panel.transform, "Right", left: false);
            StmTransferLeftList = CreateStmTransferColumnBody(panel.transform, "LeftList", left: true);
            StmTransferRightList = CreateStmTransferColumnBody(panel.transform, "RightList", left: false);

            // 인게임 스태시와 같은 격자를 양쪽에 세운다. 하나라도 못 세우면 **둘 다 목록형으로** 간다 —
            // 한쪽만 격자면 고르는 방법이 좌우로 달라져 더 헷갈린다.
            // 상세 화면이 열릴 때 채워 둔 그 세션이다(StmDetail). 비어 있으면 격자를 세울 재료
            // (프로필 껍데기)가 없다 — 조용히 목록으로 떨어지지 않게 그 사실을 적는다.
            EFT.IEftSession session = ActiveProfileSession;
            if (session?.Profile == null)
            {
                pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 세션이 비어 있어 격자를 세우지 못한다(목록으로 그린다).");
            }
            bool leftOk = TryBuildStmTransferGrid(
                StmTransferPair.Player, StmTransferLeftList as RectTransform, session,
                out StmTransferLeftPanel, out StmTransferLeftProfile, out StmTransferLeftController);
            bool rightOk = leftOk && TryBuildStmTransferGrid(
                StmTransferPair.Teammate, StmTransferRightList as RectTransform, session,
                out StmTransferRightPanel, out StmTransferRightProfile, out StmTransferRightController);

            StmTransferGridsReady = leftOk && rightOk;
            if (!StmTransferGridsReady)
            {
                DestroyStmTransferGrids();
                pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 격자를 세우지 못해 목록으로 그린다.");
            }
            else
            {
                EnableStmTransferMultiSelect();
            }

            // 격자가 서면 **버튼을 두지 않는다.** 끌어다 놓는 것이 곧 전송이다(B3-c).
            // 격자를 못 세워 목록으로 떨어졌을 때만 버튼이 필요하다 — 목록은 끌 수 없다.
            if (!StmTransferGridsReady)
            {
                CreateStmTransferActionButton(panel.transform, "GiveButton", "건네기 →", new Vector2(-86f, 16f), () => SubmitStmTransfer(toTeammate: true));
                CreateStmTransferActionButton(panel.transform, "TakeButton", "← 가져오기", new Vector2(86f, 16f), () => SubmitStmTransfer(toTeammate: false));
            }

            RedrawStmTransferLists();
            return true;
        }

        // 패널 위에서 아래로 내려가는 띠. offsetMin.y 가 "패널 바닥에서" 를 뜻하는 함정을 피하려고
        // 앵커를 위쪽 가로줄로 고정하고 높이를 직접 준다.
        private static void AnchorStmTransferBand(RectTransform rect, float top, float height, float leftPad, float rightPad)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(leftPad, top - height);
            rect.offsetMax = new Vector2(-rightPad, top);
        }

        private static CustomTextMeshProUGUI CreateStmTransferColumnHeader(Transform panel, string name, bool left)
        {
            CustomTextMeshProUGUI label = CreateOverlayText(
                "pitFireTeam_StmStashTransfer" + name + "Header", panel,
                Vector2.zero, Vector2.zero,
                TextAlignmentOptions.MidlineLeft, string.Empty,
                17f, new Color(0.87f, 0.87f, 0.84f, 1f));

            // 줄바꿈을 끈다 — 켜 두면 한국어 닉네임이 글자마다 줄이 바뀌어 **첫 글자만** 보인다
            // (버튼에서 당한 것과 같은 자리다, 2026-11-07).
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;

            RectTransform rect = label.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(left ? 0f : 0.5f, 1f);
                rect.anchorMax = new Vector2(left ? 0.5f : 1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(left ? 18f : 9f, -122f);
                rect.offsetMax = new Vector2(left ? -9f : -18f, -96f);
            }

            return label;
        }

        private static Transform CreateStmTransferColumnBody(Transform panel, string name, bool left)
        {
            // **마스크를 두지 않는다.** 2026-11-07 에 RectMask2D 로 겹침을 막았더니 스태시 패널이
            // 자기 칸 **왼쪽 바깥에** 그리는 분류(필터) 띠까지 잘라 먹었다 — 플레이어 창고 화면에
            // 늘 붙어 있는 그 세로 아이콘 줄이다. 겹침은 창을 넓혀 막고, 자르지는 않는다.
            GameObject body = new GameObject("pitFireTeam_StmStashTransfer" + name, typeof(RectTransform), typeof(Image));
            body.transform.SetParent(panel, false);
            RectTransform rect = body.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(left ? 0f : 0.5f, 0f);
            rect.anchorMax = new Vector2(left ? 0.5f : 1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            // 왼쪽 여백을 넉넉히 둔다 — 스태시 패널은 분류(필터) 띠를 **자기 칸 왼쪽 바깥에** 그린다.
            // 여백이 없으면 그 띠가 옆 칸 위로 가거나 창 밖으로 나간다.
            rect.offsetMin = new Vector2(left ? 56f : 48f, 56f);
            rect.offsetMax = new Vector2(left ? -9f : -18f, -124f);

            Image background = body.GetComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
            background.raycastTarget = false;

            return body.transform;
        }

        // 한 줄 = 버튼 하나. 고르면 색이 바뀐다. 아이콘을 그리지 않는 것은 1차 범위의 선택이다.
        private static void CreateStmTransferRow(Transform parent, StmStashEntryWire entry, bool picked, int index, Action onClick)
        {
            GameObject row = new GameObject("Row" + index, typeof(RectTransform), typeof(Image), typeof(Button));
            row.transform.SetParent(parent, false);
            RectTransform rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(4f, -(StmTransferRowHeight * (index + 1)) - 4f);
            rect.offsetMax = new Vector2(-4f, -(StmTransferRowHeight * index) - 4f);

            Image background = row.GetComponent<Image>();
            background.color = picked
                ? new Color(0.38f, 0.33f, 0.14f, 0.95f)
                : new Color(0.14f, 0.14f, 0.14f, 0.85f);
            background.raycastTarget = true;

            string stack = entry.Stack > 1 ? " ×" + entry.Stack : string.Empty;
            string contained = entry.Contained > 0 ? "  (안에 " + entry.Contained + "개)" : string.Empty;

            CustomTextMeshProUGUI label = CreateOverlayText(
                "Label", row.transform,
                new Vector2(8f, 0f), new Vector2(-8f, 0f),
                TextAlignmentOptions.MidlineLeft,
                (entry.Name ?? entry.Tpl ?? entry.Id) + stack + contained,
                15f,
                picked ? new Color(1f, 0.95f, 0.75f, 1f) : new Color(0.82f, 0.82f, 0.8f, 1f));
            label.raycastTarget = false;

            row.GetComponent<Button>().onClick.AddListener(new UnityAction(() => onClick()));
        }

        private static void CreateStmTransferActionButton(Transform panel, string name, string text, Vector2 offset, Action onClick)
        {
            GameObject button = new GameObject("pitFireTeam_StmStashTransfer" + name, typeof(RectTransform), typeof(Image), typeof(Button));
            button.transform.SetParent(panel, false);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(150f, 30f);
            rect.anchoredPosition = offset;

            Image background = button.GetComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            background.raycastTarget = true;

            CustomTextMeshProUGUI label = CreateOverlayText(
                "Label", button.transform,
                Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, text,
                16f, new Color(0.87f, 0.87f, 0.84f, 1f));
            label.raycastTarget = false;

            button.GetComponent<Button>().onClick.AddListener(new UnityAction(() => onClick()));
        }

        private static void CreateStmTransferPager(Transform parent, bool left, int page, int pages)
        {
            if (pages <= 1) return;

            CustomTextMeshProUGUI label = CreateOverlayText(
                "Pager", parent,
                Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center,
                "◀  " + (page + 1) + " / " + pages + "  ▶",
                15f, new Color(0.7f, 0.7f, 0.68f, 1f));

            RectTransform rect = label.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.offsetMin = new Vector2(4f, 2f);
                rect.offsetMax = new Vector2(-4f, 26f);
            }

            label.raycastTarget = false;

            // 쪽 넘김은 같은 띠 위의 투명 버튼 둘이다(왼쪽 절반 = 이전, 오른쪽 절반 = 다음).
            CreateStmTransferPagerHalf(parent, left, forward: false);
            CreateStmTransferPagerHalf(parent, left, forward: true);
        }

        private static void CreateStmTransferPagerHalf(Transform parent, bool leftColumn, bool forward)
        {
            GameObject half = new GameObject(forward ? "PagerNext" : "PagerPrev", typeof(RectTransform), typeof(Image), typeof(Button));
            half.transform.SetParent(parent, false);
            RectTransform rect = half.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(forward ? 0.5f : 0f, 0f);
            rect.anchorMax = new Vector2(forward ? 1f : 0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(4f, 2f);
            rect.offsetMax = new Vector2(-4f, 26f);

            Image background = half.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.01f);
            background.raycastTarget = true;

            half.GetComponent<Button>().onClick.AddListener(new UnityAction(() => StepStmTransferPage(leftColumn, forward)));
        }

        private static void StepStmTransferPage(bool leftColumn, bool forward)
        {
            StmStashSideWire side = leftColumn ? StmTransferPair?.Player : StmTransferPair?.Teammate;
            int count = side?.Items?.Length ?? 0;
            int pages = Math.Max(1, (count + StmTransferRowsPerPage - 1) / StmTransferRowsPerPage);
            int page = leftColumn ? StmTransferPageLeft : StmTransferPageRight;
            page = Math.Max(0, Math.Min(pages - 1, page + (forward ? 1 : -1)));

            if (leftColumn) StmTransferPageLeft = page; else StmTransferPageRight = page;
            RedrawStmTransferLists();
        }

        private static void RedrawStmTransferLists()
        {
            DrawStmTransferColumn(StmTransferLeftList, StmTransferLeftHeader, StmTransferPair?.Player, StmTransferPickedLeft, StmTransferPageLeft, left: true);
            DrawStmTransferColumn(StmTransferRightList, StmTransferRightHeader, StmTransferPair?.Teammate, StmTransferPickedRight, StmTransferPageRight, left: false);
        }

        private static void DrawStmTransferColumn(
            Transform list, CustomTextMeshProUGUI header, StmStashSideWire side,
            HashSet<string> picked, int page, bool left)
        {
            if (list == null) return;

            StmStashEntryWire[] items = side?.Items ?? new StmStashEntryWire[0];

            // 격자가 서 있으면 머리줄만 손본다 — 줄 목록을 그 위에 겹쳐 그리지 않는다.
            if (StmTransferGridsReady)
            {
                if (header != null)
                {
                    header.text = (left ? "나" : (side?.Nickname ?? "팀원")) + "  —  " + items.Length + "개";
                }

                return;
            }

            for (int i = list.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(list.GetChild(i).gameObject);
            }

            if (header != null)
            {
                header.text = (left ? "나" : (side?.Nickname ?? "팀원"))
                    + "  —  " + items.Length + "개"
                    + (picked.Count > 0 ? "  (고름 " + picked.Count + ")" : string.Empty);
            }

            if (items.Length == 0)
            {
                CustomTextMeshProUGUI empty = CreateOverlayText(
                    "Empty", list,
                    new Vector2(8f, 0f), new Vector2(-8f, 0f),
                    TextAlignmentOptions.Top, StmTransferEmptySide,
                    15f, new Color(0.6f, 0.6f, 0.58f, 1f));
                empty.raycastTarget = false;
                return;
            }

            int pages = Math.Max(1, (items.Length + StmTransferRowsPerPage - 1) / StmTransferRowsPerPage);
            page = Math.Max(0, Math.Min(pages - 1, page));

            int start = page * StmTransferRowsPerPage;
            int end = Math.Min(items.Length, start + StmTransferRowsPerPage);
            for (int i = start; i < end; i++)
            {
                StmStashEntryWire entry = items[i];
                string id = entry?.Id;
                if (string.IsNullOrEmpty(id)) continue;

                bool isPicked = picked.Contains(id);
                CreateStmTransferRow(list, entry, isPicked, i - start, () =>
                {
                    if (!picked.Remove(id)) picked.Add(id);
                    RedrawStmTransferLists();
                });
            }

            CreateStmTransferPager(list, left, page, pages);
        }

        // 목록형(격자를 못 세웠을 때)의 길. 고른 줄을 보낸다.
        private static void SubmitStmTransfer(bool toTeammate)
        {
            HashSet<string> picked = toTeammate ? StmTransferPickedLeft : StmTransferPickedRight;
            if (picked.Count == 0)
            {
                SetStmTransferStatus(StmTransferNothingPicked);
                return;
            }

            SubmitStmTransferIds(picked, toTeammate);
        }

        // 실제로 보내는 자리. 끌어다 놓기와 목록형이 **같은 길**을 쓴다 — 보내는 규칙이 둘로
        // 갈리면 한쪽만 고쳐지는 자리가 생긴다.
        private static async void SubmitStmTransferIds(HashSet<string> picked, bool toTeammate)
        {
            if (StmTransferBusy) return;

            if (picked == null || picked.Count == 0)
            {
                SetStmTransferStatus(StmTransferNothingPicked);
                return;
            }

            StmTransferBusy = true;
            try
            {
                string[] ids = picked.ToArray();
                string body = JsonConvert.SerializeObject(new
                {
                    aid = StmTransferAid,
                    profileId = StmTransferProfileId,
                    toTeammate,
                    itemIds = ids,
                    anchorItemId = StmTransferAnchorId,
                    anchorGrid = StmTransferAnchorGrid,
                    anchorX = StmTransferAnchorX,
                    anchorY = StmTransferAnchorY,
                    anchorRotation = StmTransferAnchorRotation,
                });

                SetStmTransferStatus("옮기는 중…");
                string json = await Task.Run(() => RequestHandler.PostJson(StmStashTransferRoute, body));

                // 서버의 거절은 **그대로 보여 준다**. 화면이 제 말로 바꿔 쓰면 사람이 사유를 못 읽는다.
                string failure = ReadStmCloseFailure(json, "옮기지 못했습니다");
                if (failure != null)
                {
                    SetStmTransferStatus(failure);
                    return;
                }

                StmStashTransferResultWire moved = DeserializeBodySuccess<StmStashTransferResultWire>(json)?.data;
                picked.Clear();

                // 결과는 **다시 받아** 그린다. 화면이 제 손으로 목록을 고치면 서버와 어긋난다.
                if (await LoadStmTransferPairAsync())
                {
                    StmTransferPageLeft = 0;
                    StmTransferPageRight = 0;

                    // **격자는 다시 세워야 바뀐다.** 머리줄만 고치면 숫자는 맞는데 칸은 그대로라,
                    // "일부만 옮겨졌다" 로 보이고 창을 껐다 켜면 멀쩡하다(2026-11-07 지적).
                    // 격자는 세울 때의 스태시 트리를 들고 있어서, 새 트리로 다시 세우는 수밖에 없다.
                    if (StmTransferGridsReady)
                    {
                        RebuildStmTransferGrids();
                    }

                    RedrawStmTransferLists();
                }

                SetStmTransferStatus(moved == null
                    ? "옮겼습니다."
                    : (toTeammate ? "건넸습니다 — " : "가져왔습니다 — ")
                        + "고른 " + moved.Picked + "개, 안에 든 것까지 " + moved.Moved + "개.");
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 스태시 교환 요청이 실패했다.");
                pitFireTeam.Log.LogError(ex);
                SetStmTransferStatus(ex.Message ?? "옮기지 못했습니다");
            }
            finally
            {
                StmTransferBusy = false;
            }
        }

        private static void SetStmTransferStatus(string text)
        {
            if (StmTransferStatusLabel != null) StmTransferStatusLabel.text = text ?? string.Empty;
        }

        // 한 쪽의 격자를 세운다. 장비 편집기의 길을 그대로 쓴다:
        //   평면 목록 → EFT 스태시 트리(FlatItemsToTree) → 그 스태시를 꽂은 프로필 → InventoryController
        //   → PIT 의 SimpleStashPanel 빌더.
        // 실패하면 false 를 돌리고 화면은 목록형으로 떨어진다 — 빈 격자를 세워 "스태시가 비었다" 로
        // 읽히게 두지 않는다.
        private static bool TryBuildStmTransferGrid(
            StmStashSideWire side,
            RectTransform section,
            EFT.IEftSession session,
            out SimpleStashPanel panel,
            out Profile profile,
            out InventoryController controller)
        {
            panel = null;
            profile = null;
            controller = null;

            try
            {
                if (side?.Tree == null || side.Tree.Length == 0 || string.IsNullOrWhiteSpace(side.StashRootId))
                {
                    return false;
                }

                StmGearStashWire asStash = new StmGearStashWire
                {
                    StashRootId = side.StashRootId,
                    Items = side.Tree,
                };

                if (!TryBuildStmEditorStash(asStash, out EFT.InventoryLogic.Stash stash, out string stashFailure))
                {
                    pitFireTeam.Log.LogWarning($"[STM] 스태시 교환: '{side.Nickname}' 의 격자를 세우지 못했다 ({stashFailure}).");
                    return false;
                }

                Profile baseProfile = session?.Profile?.Clone();
                InventoryEquipment equipment = baseProfile?.Inventory?.Equipment?.CloneItemWithSameId() as InventoryEquipment;
                if (baseProfile?.Inventory == null || equipment == null)
                {
                    return false;
                }

                EFT.InventoryDescriptor descriptor = new EFT.InventoryDescriptor(baseProfile.Inventory, EFT.FullySearchedSearchController.Instance)
                {
                    Equipment = EFT.ItemBinarySerializer.SerializeItem(equipment, null),
                    Stash = EFT.ItemBinarySerializer.SerializeItem(stash, null),
                };

                baseProfile.Inventory = descriptor.ToInventory();
                if (baseProfile.Inventory?.Stash == null
                    || !string.Equals(baseProfile.Inventory.Stash.Id, side.StashRootId, StringComparison.OrdinalIgnoreCase))
                {
                    // 세운 스태시가 서버가 말한 그 스태시가 아니면 믿을 수 없다(편집기와 같은 관문).
                    return false;
                }

                profile = baseProfile;
                // examined: true — 편집기와 같은 이유다(살펴보지 않은 것으로 보면 조작 메뉴가 셋만 남는다).
                controller = new StmGearEditInventoryController(profile, true);

                SimpleStashPanel previous = LoadoutEditorStashPanel;
                LoadoutEditorStashPanel = null;
                TryBuildLoadoutEditorStashPanel(
                    section, profile, controller,
                    new EFT.InventoryLogic.InventorySelectableItemContext(EItemViewType.Inventory));
                panel = LoadoutEditorStashPanel;
                LoadoutEditorStashPanel = previous;

                return panel != null;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 스태시 교환: 격자를 세우다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                panel = null;
                profile = null;
                controller = null;
                return false;
            }
        }

        // ---- 끌어다 놓기(B3-c) ----
        //
        // 드롭 패치가 묻는 두 가지. 이 자리에 두는 이유는 패널·자료·창 상태가 전부 여기 있어서다.

        /// <summary>
        /// 이 드롭이 **교차 드롭**인가(한쪽 스태시의 것을 반대쪽 격자에 놓았는가).
        /// 그럴 때만 참이고, `toTeammate` 가 방향을 말한다.
        /// </summary>
        internal static bool StmTransferHandlesDrop(Component targetGrid, Item dragged, out bool toTeammate)
        {
            toTeammate = false;

            if (!StmTransferGridsReady || StmTransferOverlayRoot == null || dragged?.Id == null) return false;
            if (targetGrid == null) return false;

            bool targetIsTeammate = StmTransferRightPanel != null && targetGrid.transform.IsChildOf(StmTransferRightPanel.transform);
            bool targetIsPlayer = StmTransferLeftPanel != null && targetGrid.transform.IsChildOf(StmTransferLeftPanel.transform);
            if (!targetIsTeammate && !targetIsPlayer) return false;

            bool fromPlayer = StmTransferSideOwns(StmTransferPair?.Player, dragged.Id);
            bool fromTeammate = StmTransferSideOwns(StmTransferPair?.Teammate, dragged.Id);

            // 같은 칸 안에서 옮기는 것은 **우리 일이 아니다** — EFT 가 그대로 처리한다.
            if (fromPlayer && targetIsTeammate) { toTeammate = true; return true; }
            if (fromTeammate && targetIsPlayer) { toTeammate = false; return true; }
            return false;
        }

        // 그 스태시의 **바닥에 놓인 것**인가. 가방 안의 것을 따로 건네는 길은 열지 않는다(B1 정책).
        private static bool StmTransferSideOwns(StmStashSideWire side, string itemId)
        {
            if (side?.Items == null || string.IsNullOrEmpty(itemId)) return false;
            foreach (StmStashEntryWire entry in side.Items)
            {
                if (string.Equals(entry?.Id, itemId, StringComparison.Ordinal)) return true;
            }

            return false;
        }

        /// <summary>
        /// 끌어다 놓은 것을 실제로 보낸다. 고른 것이 여럿이면 **한 번에** 보낸다.
        /// 놓은 자리(격자 이름·칸·회전)를 함께 보내 **그 자리에 그대로** 놓이게 한다.
        /// </summary>
        internal static void StmTransferDropped(Item dragged, bool toTeammate, string gridName, int x, int y, int rotation)
        {
            if (dragged?.Id == null) return;

            StmTransferAnchorId = dragged.Id;
            StmTransferAnchorGrid = gridName;
            StmTransferAnchorX = x;
            StmTransferAnchorY = y;
            StmTransferAnchorRotation = rotation;

            StmStashSideWire side = toTeammate ? StmTransferPair?.Player : StmTransferPair?.Teammate;
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal) { dragged.Id };

            // 끌어온 것이 지금 고른 것들 중 하나면 그 선택 전체를 보낸다 — 복수로 끌면 그렇게 보인다.
            HashSet<string> selection = ReadStmTransferSelection();
            if (selection.Contains(dragged.Id))
            {
                foreach (string id in selection)
                {
                    if (StmTransferSideOwns(side, id)) ids.Add(id);
                }
            }

            SubmitStmTransferIds(ids, toTeammate);
        }

        // 선택 상자(UI Fixes)를 우리 창에도 연다. 편집기의 같은 함수와 같은 관문을 선다 —
        // 설 수 없는 자리면 붙이지 않는다(붙이면 매 프레임 null 을 만진다).
        private static void EnableStmTransferMultiSelect()
        {
            try
            {
                if (StmTransferOverlayRoot == null) return;

                Type drawMultiSelect = AccessTools.TypeByName("UIFixes.DrawMultiSelect");
                if (drawMultiSelect == null) return;

                if (StmTransferOverlayRoot.GetComponentInParent<GraphicRaycaster>() == null) return;

                PreloaderUI preloader = Singleton<PreloaderUI>.Instance;
                if (preloader == null
                    || preloader.transform.childCount == 0
                    || preloader.transform.GetChild(0).GetComponent<GraphicRaycaster>() == null)
                {
                    return;
                }

                if (StmTransferOverlayRoot.GetComponent(drawMultiSelect) != null) return;
                StmTransferOverlayRoot.AddComponent(drawMultiSelect);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 선택 상자를 켜지 못했다: " + ex.Message);
            }
        }

        // 전송 뒤 양쪽 격자를 **새 자료로 다시 세운다.** 세우지 못하면 목록으로 떨어진다.
        private static void RebuildStmTransferGrids()
        {
            try
            {
                DestroyStmTransferGrids();

                EFT.IEftSession session = ActiveProfileSession;
                bool leftOk = TryBuildStmTransferGrid(
                    StmTransferPair?.Player, StmTransferLeftList as RectTransform, session,
                    out StmTransferLeftPanel, out StmTransferLeftProfile, out StmTransferLeftController);
                bool rightOk = leftOk && TryBuildStmTransferGrid(
                    StmTransferPair?.Teammate, StmTransferRightList as RectTransform, session,
                    out StmTransferRightPanel, out StmTransferRightProfile, out StmTransferRightController);

                StmTransferGridsReady = leftOk && rightOk;
                if (!StmTransferGridsReady)
                {
                    DestroyStmTransferGrids();
                    pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 전송 뒤 격자를 다시 세우지 못했다. 창을 닫았다 열면 다시 선다.");
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 스태시 교환: 격자를 다시 세우다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                StmTransferGridsReady = false;
            }
        }

        private static void DestroyStmTransferGrids()
        {
            foreach (SimpleStashPanel panel in new[] { StmTransferLeftPanel, StmTransferRightPanel })
            {
                try
                {
                    if (panel != null)
                    {
                        panel.Close();
                        GameObject.Destroy(panel.gameObject);
                    }
                }
                catch (Exception ex)
                {
                    pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 격자를 치우다 실패했다: " + ex.Message);
                }
            }

            StmTransferLeftPanel = null;
            StmTransferRightPanel = null;
            StmTransferLeftProfile = null;
            StmTransferRightProfile = null;
            StmTransferLeftController = null;
            StmTransferRightController = null;
            StmTransferGridsReady = false;
        }

        // 복수 선택(UI Fixes)에서 지금 고른 아이템의 id 를 읽는다. UI Fixes 가 없으면 빈 목록이다 —
        // 그때는 화면이 "고른 것이 없다" 고 말하고 아무것도 보내지 않는다(조용히 전부 보내지 않는다).
        private static HashSet<string> ReadStmTransferSelection()
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                Type multiSelect = AccessTools.TypeByName("UIFixes.MultiSelect");
                PropertyInfo contexts = multiSelect == null ? null : AccessTools.Property(multiSelect, "ItemContexts");
                if (contexts?.GetValue(null) is System.Collections.IEnumerable list)
                {
                    foreach (object entry in list)
                    {
                        if (entry is EFT.InventoryLogic.ItemContext itemContext && itemContext.Item?.Id != null)
                        {
                            ids.Add(itemContext.Item.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 고른 것을 읽지 못했다: " + ex.Message);
            }

            return ids;
        }

        // 겉틀만 치운다. **받아 온 자료(StmTransferPair)와 고른 것은 건드리지 않는다** — 다시 그릴 때도
        // 이 함수를 쓰기 때문이다. 자료까지 비우는 것은 화면을 정말 닫을 때뿐이다.
        private static void DestroyStmTransferOverlayRoot()
        {
            // 격자를 **먼저** 닫는다. 겉틀을 먼저 없애면 패널이 자기 자식과 함께 사라져
            // Close() 가 이미 없는 것을 만진다.
            DestroyStmTransferGrids();

            try
            {
                if (StmTransferOverlayRoot != null)
                {
                    GameObject.Destroy(StmTransferOverlayRoot);
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning("[STM] 스태시 교환 화면을 치우다 실패했다: " + ex.Message);
            }
            finally
            {
                StmTransferOverlayRoot = null;
                StmTransferLeftList = null;
                StmTransferRightList = null;
                StmTransferLeftHeader = null;
                StmTransferRightHeader = null;
                StmTransferStatusLabel = null;
            }
        }

        private static void CloseStmTransferOverlay()
        {
            DestroyStmTransferOverlayRoot();
            StmTransferPair = null;
            StmTransferPickedLeft.Clear();
            StmTransferPickedRight.Clear();
        }
    }
}
