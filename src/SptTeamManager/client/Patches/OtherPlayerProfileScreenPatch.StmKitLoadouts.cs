using EFT.InventoryLogic;
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
using UnityEngine.UI;
using ResultProfile = EFT.OtherPlayerProfile;

namespace pitTeam.Patches
{
    // 서버가 /stm/members/builds 로 주는 저장된 장비 빌드 목록. **두 묶음이 각자의 자리에 온다** -
    // 한 목록에 꼬리표로 섞여 오지 않으므로 화면이 둘을 가를 때 실수할 여지가 없다.
    internal class StmGearBuildRowWire
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    internal class StmGearBuildsWire
    {
        public List<StmGearBuildRowWire> TeammateBuilds { get; set; }
        public List<StmGearBuildRowWire> PlayerBuilds { get; set; }
        public string TeammateNickname { get; set; }
        public string ProfileId { get; set; }
    }

    internal class StmKitSlotWire
    {
        public string SlotId { get; set; }
        public string Outcome { get; set; }
        public int BuyCount { get; set; }
        public int StashCount { get; set; }
    }

    internal class StmKitQuoteWire
    {
        public string BuildId { get; set; }
        public string BuildName { get; set; }
        public string Owner { get; set; }
        public int Total { get; set; }
        public bool Affordable { get; set; }
        public int PlayerRoubles { get; set; }
        public int ItemsToBuy { get; set; }
        public int ItemsToStash { get; set; }
        public List<StmKitSlotWire> Slots { get; set; }
    }

    internal class StmKitBuyWire
    {
        public string BuildName { get; set; }
        public int Paid { get; set; }
        public int ItemsBought { get; set; }
        public int ItemsStashed { get; set; }
    }

    // 계획 4 Task 4 — 프로필 팀원의 "장비 꾸러미" 화면.
    //
    // 용병 화면의 같은 이름 버튼은 PIT 의 키트 구매로 가는데, 그 경로는 프로필 팀원에게 쓸 수 없다
    // (서버가 BuyKit 을 거부한다 - 플레이어 스태시를 출처로 쓰는 길이다). 여기서는 STM 의 경로로 간다:
    // 저장된 장비 빌드를 두 묶음으로 보여 주고, 고르면 값을 묻고, 확인하면 산다.
    //
    // 화면은 판정을 하지 않는다. 살 수 있는지·얼마인지·무엇이 바뀌는지는 전부 서버가 정하고 여기서는
    // 그것을 보여 주기만 한다. 살 때도 **화면이 보여 준 값**을 함께 보내 서버가 다시 견준다 - 그 사이에
    // 값이 바뀌었으면 서버가 거절한다(GearKitPurchasePolicy).
    internal partial class OtherPlayerProfileScreenPatch
    {
        private const string StmBuildsRoute = "/stm/members/builds";
        private const string StmKitQuoteRoute = "/stm/members/kit-quote";
        private const string StmKitBuyRoute = "/stm/members/kit-buy";

        // ---- 화면 문구 ----
        private const string StmKitButtonText = "장비 꾸러미";
        private const string StmKitPickerTitle = "장비 꾸러미";
        private const string StmKitTeammateGroupFormat = "{0} 의 빌드";
        private const string StmKitPlayerGroupText = "내 빌드";
        private const string StmKitNoBuildsText = "저장된 장비 빌드가 없습니다. 게임에서 장비 프리셋을 먼저 저장하세요.";
        private const string StmKitLoadFailedToast = "빌드 목록을 불러오지 못했습니다";
        private const string StmKitQuoteFailedToast = "값을 매기지 못했습니다";
        private const string StmKitBuyFailedToast = "장비를 사지 못했습니다";
        private const string StmKitBusyText = "불러오는 중입니다...";
        private const string StmKitBuyingText = "사는 중입니다. 끝날 때까지 창을 닫지 마세요...";
        private const string StmKitBuyButtonText = "구매";
        private const string StmKitBackButtonText = "목록으로";
        private const string StmKitConfirmTitle = "팀원 장비 구매";
        private const string StmKitConfirmButtonText = "위 내용을 이해했고, 삽니다";

        // 창 크기. CreateOverlayButton 은 **왼쪽 아래 기준**(anchor·pivot 0,0)이라 버튼 y 는 바닥에서 잰다.
        // 목록은 위에서부터 쌓는 편이 읽기 쉬우므로 "위에서 몇 px" 로 세고 마지막에 바닥 기준으로 바꾼다.
        private const float StmKitPanelWidth = 620f;
        private const float StmKitPanelHeight = 520f;
        private const float StmKitRowWidth = 520f;
        private const float StmKitRowHeight = 36f;
        private const int StmKitMaxRows = 10;

        private static GameObject StmKitOverlayRoot;
        private static RectTransform StmKitButtonRoot;
        private static bool StmKitBusy;

        // ---- 입구 버튼 ----

        // 조작 줄 아래에 한 줄을 더 둔다(용병 화면의 "장비 꾸러미" 와 같은 자리).
        private static void CreateStmKitButton(
            OtherPlayerProfileScreen screen,
            RectTransform loadoutSelector,
            Transform parent,
            ResultProfile profile,
            StmProfileDetailWire detail)
        {
            try
            {
                if (screen == null || loadoutSelector == null || parent == null || profile == null) return;

                if (StmKitButtonRoot != null)
                {
                    GameObject.Destroy(StmKitButtonRoot.gameObject);
                    StmKitButtonRoot = null;
                }

                RectTransform rowClone = GameObject.Instantiate(loadoutSelector, parent, true);
                rowClone.name = "pitFireTeam_StmKitRow";
                rowClone.anchoredPosition = loadoutSelector.anchoredPosition + GetProfileControlRowOffset(loadoutSelector, 2);
                rowClone.gameObject.SetActive(true);

                // 복제한 줄의 드롭다운 두 칸은 쓰지 않는다 - 버튼 하나만 올린다.
                foreach (var name in new[] { "Upper", "Lower" })
                {
                    Transform part = rowClone.Find(name);
                    if (part != null) part.gameObject.SetActive(false);
                }

                DefaultUIButton buttonTemplate = HideoutButtonField?.GetValue(screen) as DefaultUIButton;
                if (buttonTemplate == null)
                {
                    GameObject.Destroy(rowClone.gameObject);
                    pitFireTeam.Log.LogWarning("[STM] 장비 꾸러미 버튼을 만들지 못했다: 버튼 템플릿이 없다.");
                    return;
                }

                DefaultUIButton button = GameObject.Instantiate(buttonTemplate, rowClone, false);
                button.name = "pitFireTeam_StmKitButton";
                button.gameObject.SetActive(true);
                button.SetRawText(StmKitButtonText, 18);
                HideProfileButtonIconContainer(button);
                // 장비 편집이 막혀 있으면 꾸러미도 막힌다: 서버가 같은 관문(레이드·미결 전송)을 선다.
                button.Interactable = detail != null && detail.CanEditGear;
                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener(() => OpenStmKitPicker(screen, profile, detail));

                if (button.transform is RectTransform buttonRect && buttonTemplate.transform is RectTransform templateRect)
                {
                    buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                    buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                    buttonRect.pivot = new Vector2(0.5f, 0.5f);
                    buttonRect.anchoredPosition = Vector2.zero;
                    buttonRect.sizeDelta = templateRect.sizeDelta;
                    buttonRect.localScale = Vector3.one;
                }

                StmKitButtonRoot = rowClone;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 장비 꾸러미 버튼을 만들다 실패했다. 나머지 화면은 그대로다.");
                pitFireTeam.Log.LogError(ex);
            }
        }

        // ---- 목록 ----

        private static async void OpenStmKitPicker(OtherPlayerProfileScreen screen, ResultProfile profile, StmProfileDetailWire detail)
        {
            if (StmKitBusy || screen == null || profile == null || detail == null) return;
            StmKitBusy = true;
            try
            {
                ShowStmKitMessage(screen, StmKitBusyText);

                string body = JsonConvert.SerializeObject(new { aid = profile.AccountId, profileId = detail.ProfileId });
                string json = await Task.Run(() => RequestHandler.PostJson(StmBuildsRoute, body));
                if (screen == null) return;

                string failure = ReadStmCloseFailure(json, StmKitLoadFailedToast);
                if (failure != null)
                {
                    CloseStmKitOverlay();
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                StmGearBuildsWire builds = DeserializeBodySuccess<StmGearBuildsWire>(json)?.data;
                ShowStmKitList(screen, profile, detail, builds);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 빌드 목록을 불러오다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                CloseStmKitOverlay();
                AddTeammateCreationFlow.ShowToast(StmKitLoadFailedToast);
            }
            finally
            {
                StmKitBusy = false;
            }
        }

        private static void ShowStmKitList(OtherPlayerProfileScreen screen, ResultProfile profile, StmProfileDetailWire detail, StmGearBuildsWire builds)
        {
            var teammate = builds?.TeammateBuilds ?? new List<StmGearBuildRowWire>();
            var player = builds?.PlayerBuilds ?? new List<StmGearBuildRowWire>();
            if (teammate.Count == 0 && player.Count == 0)
            {
                ShowStmKitMessage(screen, StmKitNoBuildsText);
                return;
            }

            var panel = CreateStmKitPanel(screen, out DefaultUIButton buttonTemplate);
            if (panel == null) return;

            float fromTop = 58f;
            string teammateHeading = string.Format(
                StmKitTeammateGroupFormat,
                string.IsNullOrWhiteSpace(builds?.TeammateNickname) ? "팀원" : builds.TeammateNickname);

            // 두 묶음을 머리글로 갈라 세운다. 섞어 놓고 꼬리표로 구분하지 않는다.
            fromTop = AddStmKitGroup(panel, buttonTemplate, teammateHeading, teammate, fromTop,
                row => OpenStmKitQuote(screen, profile, detail, row, "teammate"));
            AddStmKitGroup(panel, buttonTemplate, StmKitPlayerGroupText, player, fromTop,
                row => OpenStmKitQuote(screen, profile, detail, row, "player"));
        }

        private static float AddStmKitGroup(
            RectTransform panel,
            DefaultUIButton buttonTemplate,
            string heading,
            List<StmGearBuildRowWire> rows,
            float fromTop,
            Action<StmGearBuildRowWire> onPick)
        {
            if (rows == null || rows.Count == 0) return fromTop;

            CreateStmKitHeading(panel, heading, fromTop);
            fromTop += 30f;

            // 열 줄까지만 보인다. 넘치게 그리는 대신 끊고, 끊었다는 사실을 말한다.
            foreach (var row in rows.Take(StmKitMaxRows))
            {
                var picked = row;
                float bottomY = StmKitPanelHeight - fromTop - StmKitRowHeight;
                DefaultUIButton button = CreateOverlayButton(
                    buttonTemplate,
                    panel,
                    new Vector2((StmKitPanelWidth - StmKitRowWidth) / 2f, bottomY),
                    new Vector2(StmKitRowWidth, StmKitRowHeight));
                button.name = $"pitFireTeam_StmKitRow_{picked.Id}";
                button.SetRawText(picked.Name ?? picked.Id, 18);
                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener(() => onPick(picked));
                fromTop += 42f;
            }

            if (rows.Count > StmKitMaxRows)
            {
                CreateStmKitHeading(panel, $"그 밖에 {rows.Count - StmKitMaxRows}개가 더 있습니다(여기 보이지 않음).", fromTop);
                fromTop += 26f;
            }

            return fromTop + 14f;
        }

        // 위에서 fromTop 만큼 내려온 자리에 한 줄짜리 머리글을 둔다. CreateOverlayText 는 창 전체로 늘어나는
        // 모양이라(앵커가 0,0-1,1) 목록에는 쓸 수 없다 - 여러 줄이 같은 자리에 겹친다.
        private static void CreateStmKitHeading(RectTransform panel, string text, float fromTop)
        {
            GameObject textObject = new GameObject("pitFireTeam_StmKitHeading", typeof(RectTransform), typeof(CustomTextMeshProUGUI));
            textObject.transform.SetParent(panel, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(24f, -26f);
            rect.offsetMax = new Vector2(-24f, 0f);
            rect.anchoredPosition = new Vector2(0f, -fromTop);
            rect.localScale = Vector3.one;

            CustomTextMeshProUGUI label = textObject.GetComponent<CustomTextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.color = new Color(0.62f, 0.62f, 0.58f, 1f);
            label.raycastTarget = false;
        }

        // ---- 값 ----

        private static async void OpenStmKitQuote(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            StmGearBuildRowWire row,
            string owner)
        {
            if (StmKitBusy || row == null) return;
            StmKitBusy = true;
            try
            {
                ShowStmKitMessage(screen, StmKitBusyText);

                string body = JsonConvert.SerializeObject(new
                {
                    aid = profile.AccountId,
                    profileId = detail.ProfileId,
                    buildId = row.Id,
                    owner,
                });
                string json = await Task.Run(() => RequestHandler.PostJson(StmKitQuoteRoute, body));
                if (screen == null) return;

                string failure = ReadStmCloseFailure(json, StmKitQuoteFailedToast);
                if (failure != null)
                {
                    // 값을 못 매긴 이유(상인이 팔지 않는 물건 등)는 서버가 사람 말로 보낸다. 창에 남겨 읽게 한다.
                    ShowStmKitMessage(screen, failure);
                    return;
                }

                StmKitQuoteWire quote = DeserializeBodySuccess<StmKitQuoteWire>(json)?.data;
                if (quote == null)
                {
                    ShowStmKitMessage(screen, StmKitQuoteFailedToast);
                    return;
                }

                ShowStmKitQuote(screen, profile, detail, quote, owner);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 꾸러미 값을 묻다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                ShowStmKitMessage(screen, StmKitQuoteFailedToast);
            }
            finally
            {
                StmKitBusy = false;
            }
        }

        private static void ShowStmKitQuote(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            StmKitQuoteWire quote,
            string owner)
        {
            var panel = CreateStmKitPanel(screen, out DefaultUIButton buttonTemplate);
            if (panel == null) return;

            var changed = (quote.Slots ?? new List<StmKitSlotWire>())
                .Where(s => s != null && !string.Equals(s.Outcome, "Unchanged", StringComparison.Ordinal))
                .Select(s => s.SlotId)
                .ToList();

            string summary =
                $"{quote.BuildName}\n\n" +
                $"값: {quote.Total:N0} ₽   (가진 돈 {quote.PlayerRoubles:N0} ₽)\n" +
                $"살 물건 {quote.ItemsToBuy}점, 이 팀원 스태시로 돌아갈 헌 장비 {quote.ItemsToStash}점\n" +
                $"바뀌는 칸: {(changed.Count == 0 ? "없음" : string.Join(", ", changed))}\n\n" +
                "물건은 전부 새로 삽니다. 돈은 지금 플레이어 계정(스태시)에서 나갑니다.";

            CreateOverlayText(
                "pitFireTeam_StmKitQuoteText",
                panel,
                new Vector2(24f, 92f),
                new Vector2(-24f, -58f),
                TextAlignmentOptions.TopLeft,
                summary,
                18f,
                new Color(0.9f, 0.88f, 0.84f, 1f));

            DefaultUIButton back = CreateOverlayButton(buttonTemplate, panel, new Vector2(50f, 42f), new Vector2(220f, 38f));
            back.name = "pitFireTeam_StmKitBackButton";
            back.SetRawText(StmKitBackButtonText, 18);
            back.OnClick.RemoveAllListeners();
            back.OnClick.AddListener(() => OpenStmKitPicker(screen, profile, detail));

            DefaultUIButton buy = CreateOverlayButton(buttonTemplate, panel, new Vector2(350f, 42f), new Vector2(220f, 38f));
            buy.name = "pitFireTeam_StmKitBuyButton";
            buy.SetRawText(StmKitBuyButtonText, 18);
            // 살 수 없는 경우는 서버가 정한다. 화면은 그 판정을 그대로 쓴다(여기서 다시 세지 않는다).
            buy.Interactable = quote.Affordable && quote.ItemsToBuy > 0;
            buy.OnClick.RemoveAllListeners();
            buy.OnClick.AddListener(() => ShowStmConfirm(
                screen,
                StmKitConfirmTitle,
                $"{quote.BuildName} 을(를) {quote.Total:N0} ₽ 에 삽니다.\n" +
                $"헌 장비 {quote.ItemsToStash}점은 이 팀원의 스태시로 돌아갑니다.\n" +
                "돈은 되돌릴 수 없습니다.",
                StmKitConfirmButtonText,
                () => SubmitStmKitBuy(screen, profile, detail, quote, owner)));
        }

        // ---- 구매 ----

        private static async void SubmitStmKitBuy(
            OtherPlayerProfileScreen screen,
            ResultProfile profile,
            StmProfileDetailWire detail,
            StmKitQuoteWire quote,
            string owner)
        {
            if (StmKitBusy) return;
            StmKitBusy = true;
            try
            {
                ShowStmKitMessage(screen, StmKitBuyingText);

                string body = JsonConvert.SerializeObject(new
                {
                    aid = profile.AccountId,
                    profileId = detail.ProfileId,
                    buildId = quote.BuildId,
                    owner,
                    // 화면이 보여 준 값. 서버가 다시 매겨 이것과 다르면 거절한다.
                    expectedTotal = quote.Total,
                });
                string json = await Task.Run(() => RequestHandler.PostJson(StmKitBuyRoute, body));
                if (screen == null) return;

                string failure = ReadStmCloseFailure(json, StmKitBuyFailedToast);
                if (failure != null)
                {
                    // 거절 사유는 "아무것도 바뀌지 않았다" 이거나 무엇이 바뀌었는지를 담고 있다. 토스트는 금방
                    // 사라지므로 창에도 남긴다.
                    ShowStmKitMessage(screen, failure);
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                StmKitBuyWire bought = DeserializeBodySuccess<StmKitBuyWire>(json)?.data;
                CloseStmKitOverlay();
                AddTeammateCreationFlow.ShowToast(bought == null
                    ? "장비를 샀습니다."
                    : $"{bought.BuildName} 을(를) 샀습니다. {bought.Paid:N0} ₽, 물건 {bought.ItemsBought}점.");
                MarkSquadRosterDirty(profile?.AccountId);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 꾸러미 구매 요청이 실패했다.");
                pitFireTeam.Log.LogError(ex);
                // 요청이 나간 뒤의 실패는 "샀는지 모른다" 다. 샀다고도 안 샀다고도 말하지 않는다.
                ShowStmKitMessage(screen, "요청이 끝났는지 알 수 없습니다. 팀원 화면을 다시 열어 장비와 돈을 확인하세요.");
            }
            finally
            {
                StmKitBusy = false;
            }
        }

        // ---- 창 ----

        private static RectTransform CreateStmKitPanel(OtherPlayerProfileScreen screen, out DefaultUIButton buttonTemplate)
        {
            buttonTemplate = null;
            if (screen == null) return null;

            CloseStmKitOverlay();

            buttonTemplate = BackButtonField?.GetValue(screen) as DefaultUIButton;
            if (buttonTemplate == null)
            {
                pitFireTeam.Log.LogWarning("[STM] 장비 꾸러미 창을 열지 못했다: 버튼 템플릿이 없다.");
                return null;
            }

            GameObject root = new GameObject("pitFireTeam_StmKitOverlay", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(screen.transform, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.SetAsLastSibling();

            Image backdrop = root.GetComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.55f);
            backdrop.raycastTarget = true;

            GameObject panelObject = new GameObject("pitFireTeam_StmKitPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(root.transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(StmKitPanelWidth, StmKitPanelHeight);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.04f, 0.04f, 0.04f, 0.98f);
            panelImage.raycastTarget = true;

            CreateOverlayText(
                "pitFireTeam_StmKitTitle",
                panel,
                new Vector2(24f, -44f),
                new Vector2(-70f, -10f),
                TextAlignmentOptions.MidlineLeft,
                StmKitPickerTitle,
                20f,
                new Color(0.86f, 0.86f, 0.84f, 1f));

            DefaultUIButton close = CreateOverlayButton(buttonTemplate, panel, new Vector2(StmKitPanelWidth - 108f, StmKitPanelHeight - 46f), new Vector2(84f, 32f));
            close.name = "pitFireTeam_StmKitCloseButton";
            close.SetRawText("닫기", 16);
            close.OnClick.RemoveAllListeners();
            close.OnClick.AddListener(CloseStmKitOverlay);

            StmKitOverlayRoot = root;
            return panel;
        }

        private static void ShowStmKitMessage(OtherPlayerProfileScreen screen, string message)
        {
            var panel = CreateStmKitPanel(screen, out _);
            if (panel == null) return;

            CreateOverlayText(
                "pitFireTeam_StmKitMessage",
                panel,
                new Vector2(24f, 24f),
                new Vector2(-24f, -58f),
                TextAlignmentOptions.TopLeft,
                message,
                18f,
                new Color(0.9f, 0.88f, 0.84f, 1f));
        }

        private static void CloseStmKitOverlay()
        {
            if (StmKitOverlayRoot == null) return;
            GameObject.Destroy(StmKitOverlayRoot);
            StmKitOverlayRoot = null;
        }
    }
}
