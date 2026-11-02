using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pitTeam.Modules;
using pitTeam.Shared;
using SPT.Common.Http;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ResultProfile = EFT.OtherPlayerProfile;

namespace pitTeam.Patches
{
    // 서버가 프로필 팀원의 옵션 응답에 싣는 STM 쪽 정보(StmProfileDetail). 값이 있으면 이 화면의 주인은 프로필 팀원이다.
    // 안내문과 경고문은 서버가 TransferEntryClosePolicy 로 만든 플레이어의 말이고, 이 화면은 그대로 보여 주기만 한다.
    internal class StmProfileDetailWire
    {
        public string ProfileId { get; set; }
        public bool Blocked { get; set; }
        public string BlockedNotice { get; set; }
        public string JournalId { get; set; }
        public bool CanClose { get; set; }
        public string CloseWarning { get; set; }
        public bool CanDiscardJournal { get; set; }
        public string DiscardWarning { get; set; }
        // 장비 편집 입구(서버의 GearEditEntryPolicy 판정). 값이 없는 응답은 false - 열 수 없음으로 읽는다.
        public bool CanEditGear { get; set; }
        public string EditNotice { get; set; }
    }

    // 프로필 팀원의 상세 화면(design 6-3 오른쪽). 새 화면을 만들지 않는다: PIT 의 OtherPlayerProfileScreen 이 이미 착용 장비를
    // 보여 주고(서버가 줄여 보낸 응답), 여기서는 그 위에 STM 이 알려야 할 두 가지 - 이 프로필을 쓸 수 없는 이유, 편집이 막힌
    // 이유와 그 출구 - 만 얹는다. PIT 의 용병 편집 UI(이름·외형·숙련·공격성)는 프로필 팀원에게 뜻이 없어 만들지 않는다.
    //
    // 이 파일의 UI 는 게임 없이 검증되지 않는다. 판정(닫아도 되는가, 경고문, 상태 안내)은 전부 shared 의 정책이고 여기는 그리기와
    // 요청 보내기뿐이다. 위치·크기는 인게임 관문에서 확인한다.
    internal partial class OtherPlayerProfileScreenPatch
    {
        private const string StmCloseTransferRoute = "/stm/members/close-transfer";
        private const string StmDiscardJournalRoute = "/stm/members/discard-journal";

        // ---- 화면 문구 (StmProfileRosterClient 와 같은 관례: 한 곳에 모은다) ----
        private const string StmCloseButtonText = "이 기록 닫기";
        private const string StmDiscardButtonText = "기록 파일 버리기";
        private const string StmDiscardConfirmTitle = "읽을 수 없는 전송 기록 파일 버리기";
        private const string StmDiscardConfirmButtonText = "위 내용을 이해했고, 버립니다";
        private const string StmDiscardedToast = "전송 기록 파일을 버렸습니다. 이제 이 팀원의 장비를 편집할 수 있습니다.";
        private const string StmDiscardFailedToast = "전송 기록 파일을 버리지 못했습니다";
        private const string StmConfirmTitle = "멈춘 전송 기록 닫기";
        private const string StmConfirmButtonText = "위 내용을 이해했고, 닫습니다";
        private const string StmCancelButtonText = "취소";
        private const string StmClosedToast = "전송 기록을 닫았습니다. 이제 이 팀원의 장비를 편집할 수 있습니다.";
        private const string StmCloseFailedToast = "전송 기록을 닫지 못했습니다";

        private static GameObject StmDetailRoot;
        private static GameObject StmConfirmRoot;
        private static bool StmCloseInFlight;

        // 옵션 조회가 오류로 돌아왔을 때 그 사유. TryLoadProfileOptions 가 호출마다 비우고 채우며, PatchPostfix 가 바로 뒤에 읽는다.
        private static string StmPendingRefusalAccountId;
        private static string StmPendingRefusalMessage;

        // TryLoadProfileOptions 가 조회 시작에 부른다. 이전 화면의 사유가 다음 화면에 새어 나가지 않게 한다.
        private static void ResetStmOptionsRefusal()
        {
            StmPendingRefusalAccountId = null;
            StmPendingRefusalMessage = null;
        }

        // TryLoadProfileOptions 가 오류 본문을 받았을 때 부른다. 사유가 STM 의 상태 이름인지는 여기서 가리지 않고 쓰는 쪽이 가린다.
        private static void NoteStmOptionsRefusal(string accountId, int err, string errmsg)
        {
            if (err == 0 || string.IsNullOrWhiteSpace(errmsg)) return;
            StmPendingRefusalAccountId = accountId;
            StmPendingRefusalMessage = errmsg;
        }

        // PatchPostfix 의 분기. true 면 이 화면은 프로필 팀원의 것이고 STM 이 처리했다 - PIT 의 용병 UI 를 만들지 않는다.
        // false 면 PIT 의 경로가 그대로 흐른다(용병이거나 친구 등 STM 과 무관한 프로필).
        private static bool TryApplyStmDetail(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            FriendlyTeammateProfileOptions options,
            InventoryController inventoryController,
            EFT.IEftSession session,
            InventoryPlayerModelWithStatsWindow playerModelWindow)
        {
            string refusal = null;
            if (StmPendingRefusalAccountId != null
                && string.Equals(StmPendingRefusalAccountId, profile?.AccountId, StringComparison.Ordinal))
            {
                refusal = StmPendingRefusalMessage;
            }

            ResetStmOptionsRefusal();
            StmShownDetailAccountId = profile?.AccountId;

            StmProfileDetailWire detail = options?.Stm;
            try
            {
                CloseStmDetail();

                // 등록된 프로필 팀원인데 지금 쓸 수 없다: 이유를 말한다. 정확히 상태 이름일 때만 - PIT 의 오류 문장은 해당 없음이다.
                if (refusal != null && ProfileStatePolicy.TryDescribeRefusal(refusal, out ProfileStateNotice notice))
                {
                    ShowStmBannerWithPending(screen, notice.Message, profile?.AccountId, null, null);
                    return true;
                }

                if (detail == null)
                {
                    return false;
                }

                // 프로필 팀원이다. 막혀 있으면 그 사실과 출구를, 아니면 장비 편집 입구를 얹는다.
                if (detail.Blocked)
                {
                    // 안내문이 비어 왔으면 서버가 이유를 못 실은 것이다. 저널 문제라고 지어내지 않고 중립 문구를 쓴다.
                    string text = string.IsNullOrWhiteSpace(detail.BlockedNotice)
                        ? TransferEntryClosePolicy.NeutralBlockedNotice
                        : detail.BlockedNotice;
                    bool closable = detail.CanClose && !string.IsNullOrWhiteSpace(detail.JournalId) && !string.IsNullOrWhiteSpace(detail.CloseWarning);
                    bool discardable = detail.CanDiscardJournal && !string.IsNullOrWhiteSpace(detail.DiscardWarning);
                    ShowStmBannerWithPending(screen, text, profile?.AccountId, closable ? detail : null, discardable ? detail : null);
                    return true;
                }

                // 막히지 않은 프로필 팀원: 장비 편집 입구. 서버가 열 수 없다고 했으면(레이드 중 등) 이유를 말하는 안내 상자가 선다.
                ShowStmGearEditEntry(screen, profile, detail, inventoryController, session, playerModelWindow);
                return true;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Failed to draw the profile teammate notice; the screen is left as the game made it.");
                pitFireTeam.Log.LogError(ex);
                // 프로필 팀원이라고 서버가 알렸는데 그리기만 실패했다: PIT 의 용병 UI 를 그 위에 만들지 않는다. 편집 차단은 서버가 한다.
                return detail != null;
            }
        }

        // 화면 위쪽에 얹는 안내 상자. closable 이 있을 때만 닫기 버튼이 붙는다.
        private static void ShowStmBanner(OtherPlayerProfileScreen screen, string text, StmProfileDetailWire closable, StmProfileDetailWire discardable)
        {
            if (screen == null || string.IsNullOrWhiteSpace(text)) return;

            DefaultUIButton buttonTemplate = BackButtonField?.GetValue(screen) as DefaultUIButton;
            bool withButton = (closable != null || discardable != null) && buttonTemplate != null;

            GameObject root = new GameObject("pitFireTeam_StmDetailNotice", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(screen.transform, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -96f);
            rect.sizeDelta = new Vector2(860f, withButton ? 188f : 132f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.16f, 0.07f, 0.05f, 0.97f);
            background.raycastTarget = true;

            CreateOverlayText(
                "pitFireTeam_StmDetailNoticeText",
                root.transform,
                new Vector2(18f, withButton ? 62f : 12f),
                new Vector2(-18f, -12f),
                TextAlignmentOptions.TopLeft,
                text,
                19f,
                new Color(0.93f, 0.9f, 0.84f, 1f));

            if (withButton)
            {
                DefaultUIButton closeButton = CreateOverlayButton(buttonTemplate, root.transform, new Vector2(18f, 14f), new Vector2(240f, 38f));
                closeButton.OnClick.RemoveAllListeners();
                // 이 버튼은 닫지도 버리지도 않는다. 경고창을 열 뿐이고, 실제 요청은 경고창의 확인 버튼에서만 나간다.
                if (closable != null)
                {
                    closeButton.name = "pitFireTeam_StmCloseTransferButton";
                    closeButton.SetRawText(StmCloseButtonText, 20);
                    closeButton.OnClick.AddListener(() => ShowStmConfirm(screen, StmConfirmTitle, closable.CloseWarning, StmConfirmButtonText, () => SubmitStmCloseTransfer(closable)));
                }
                else
                {
                    closeButton.name = "pitFireTeam_StmDiscardJournalButton";
                    closeButton.SetRawText(StmDiscardButtonText, 20);
                    closeButton.OnClick.AddListener(() => ShowStmConfirm(screen, StmDiscardConfirmTitle, discardable.DiscardWarning, StmDiscardConfirmButtonText, () => SubmitStmDiscardJournal()));
                }
            }

            StmDetailRoot = root;
        }

        private static void ShowStmConfirm(OtherPlayerProfileScreen screen, string title, string warning, string confirmLabel, Action onConfirm)
        {
            if (screen == null || string.IsNullOrWhiteSpace(warning) || onConfirm == null) return;
            CloseStmConfirm();

            DefaultUIButton buttonTemplate = BackButtonField?.GetValue(screen) as DefaultUIButton;
            if (buttonTemplate == null)
            {
                // 확인창을 못 그리면 닫지 않는다. 경고를 보여 주지 못한 채 닫기를 허용하는 길은 없다.
                pitFireTeam.Log.LogWarning("[STM] Close-transfer confirmation skipped: template button not found. Nothing was closed.");
                return;
            }

            GameObject overlay = new GameObject("pitFireTeam_StmCloseConfirm", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(screen.transform, false);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.localScale = Vector3.one;
            overlayRect.SetAsLastSibling();

            Image backdrop = overlay.GetComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.66f);
            backdrop.raycastTarget = true;

            GameObject panel = new GameObject("pitFireTeam_StmCloseConfirmPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlay.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800f, 460f);
            panelRect.localScale = Vector3.one;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.02f, 0.02f, 0.02f, 0.98f);
            panelImage.raycastTarget = true;

            GameObject header = new GameObject("pitFireTeam_StmCloseConfirmHeader", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(panel.transform, false);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(0f, -38f);
            headerRect.offsetMax = Vector2.zero;
            Image headerImage = header.GetComponent<Image>();
            headerImage.color = new Color(0.3f, 0.09f, 0.07f, 0.97f);
            headerImage.raycastTarget = true;

            CreateOverlayText(
                "pitFireTeam_StmCloseConfirmTitle",
                header.transform,
                new Vector2(18f, 0f),
                new Vector2(-54f, 0f),
                TextAlignmentOptions.MidlineLeft,
                title,
                20f,
                new Color(0.95f, 0.9f, 0.85f, 1f));

            Button headerClose = CreateWindowCloseButton(header.transform);
            if (headerClose.transform is RectTransform closeRect)
            {
                closeRect.anchorMin = new Vector2(1f, 0.5f);
                closeRect.anchorMax = new Vector2(1f, 0.5f);
                closeRect.pivot = new Vector2(1f, 0.5f);
                closeRect.anchoredPosition = new Vector2(-6f, 0f);
            }

            headerClose.onClick.AddListener(new UnityAction(CloseStmConfirm));

            CreateOverlayText(
                "pitFireTeam_StmCloseConfirmBody",
                panel.transform,
                new Vector2(30f, 84f),
                new Vector2(-30f, -54f),
                TextAlignmentOptions.TopLeft,
                warning,
                20f,
                new Color(0.9f, 0.88f, 0.84f, 1f));

            // 취소가 왼쪽, 확인이 오른쪽. 확인 버튼 문구는 "닫습니다" 가 무엇을 받아들이는지 다시 말한다.
            DefaultUIButton cancelButton = CreateOverlayButton(buttonTemplate, panel.transform, new Vector2(30f, 24f), new Vector2(180f, 40f));
            cancelButton.name = "pitFireTeam_StmCloseCancelButton";
            cancelButton.SetRawText(StmCancelButtonText, 20);
            cancelButton.OnClick.RemoveAllListeners();
            cancelButton.OnClick.AddListener(CloseStmConfirm);

            DefaultUIButton confirmButton = CreateOverlayButton(buttonTemplate, panel.transform, new Vector2(470f, 24f), new Vector2(300f, 40f));
            confirmButton.name = "pitFireTeam_StmCloseConfirmButton";
            confirmButton.SetRawText(confirmLabel, 19);
            confirmButton.OnClick.RemoveAllListeners();
            confirmButton.OnClick.AddListener(() => onConfirm());

            StmConfirmRoot = overlay;
        }

        // 읽을 수 없는 저널 문서를 버리는 요청. 확인창의 확인 버튼에서만 불린다.
        private static async void SubmitStmDiscardJournal()
        {
            if (StmCloseInFlight) return;
            StmCloseInFlight = true;
            try
            {
                string body = JsonConvert.SerializeObject(new { acknowledged = true });
                string json = await Task.Run(() => RequestHandler.PostJson(StmDiscardJournalRoute, body));
                string failure = ReadStmCloseFailure(json, StmDiscardFailedToast);

                CloseStmConfirm();
                if (failure != null)
                {
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                CloseStmDetail();
                AddTeammateCreationFlow.ShowToast(StmDiscardedToast);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Discarding the unreadable transfer journal failed.");
                pitFireTeam.Log.LogError(ex);
                CloseStmConfirm();
                AddTeammateCreationFlow.ShowToast(StmDiscardFailedToast);
            }
            finally
            {
                StmCloseInFlight = false;
            }
        }

        // 확인 버튼에서만 불린다. acknowledged 는 이 호출에서만 true 로 보낸다 - 다른 어떤 경로도 이 요청을 만들지 않는다.
        private static async void SubmitStmCloseTransfer(StmProfileDetailWire detail)
        {
            if (detail == null || StmCloseInFlight) return;
            StmCloseInFlight = true;
            try
            {
                string body = JsonConvert.SerializeObject(new
                {
                    profileId = detail.ProfileId,
                    journalId = detail.JournalId,
                    acknowledged = true,
                });
                string json = await Task.Run(() => RequestHandler.PostJson(StmCloseTransferRoute, body));
                string failure = ReadStmCloseFailure(json, StmCloseFailedToast);

                CloseStmConfirm();
                if (failure != null)
                {
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                // 닫혔다. 안내 상자는 더 이상 사실이 아니므로 거둔다(다른 항목이 남았다면 화면을 다시 열 때 나온다).
                CloseStmDetail();
                AddTeammateCreationFlow.ShowToast(StmClosedToast);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Closing the transfer entry failed.");
                pitFireTeam.Log.LogError(ex);
                CloseStmConfirm();
                AddTeammateCreationFlow.ShowToast(StmCloseFailedToast);
            }
            finally
            {
                StmCloseInFlight = false;
            }
        }

        // 성공이면 null. 빈 응답이나 읽을 수 없는 응답은 성공이 아니다. 서버의 errmsg 는 플레이어가 읽는 말이다.
        private static string ReadStmCloseFailure(string json, string fallback)
        {
            if (string.IsNullOrWhiteSpace(json)) return fallback;

            JToken root = JToken.Parse(json);
            int err = root["err"]?.Value<int>() ?? -1;
            if (err == 0) return null;

            string errmsg = root["errmsg"]?.ToString();
            return string.IsNullOrWhiteSpace(errmsg) ? fallback : errmsg;
        }

        private static void CloseStmConfirm()
        {
            if (StmConfirmRoot == null) return;
            GameObject.Destroy(StmConfirmRoot);
            StmConfirmRoot = null;
        }

        private static void CloseStmDetail()
        {
            CloseStmConfirm();
            if (StmGearEditEntryRoot != null)
            {
                GameObject.Destroy(StmGearEditEntryRoot);
                StmGearEditEntryRoot = null;
            }

            if (StmDetailRoot == null) return;
            GameObject.Destroy(StmDetailRoot);
            StmDetailRoot = null;
        }
    }
}
