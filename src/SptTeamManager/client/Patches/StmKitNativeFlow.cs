using EFT.UI;
using Newtonsoft.Json;
using pitTeam.Modules;
using SPT.Common.Http;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ResultProfile = EFT.OtherPlayerProfile;

namespace pitTeam.Patches
{
    // 프로필 팀원의 장비 꾸러미를 **게임의 기본 장비 프리셋 화면**으로 연다(계획 4 Task 4-b).
    //
    // 용병 쪽이 쓰는 화면이 바로 그것이다: PIT 의 TeammateEquipmentBuildsScreenFlow 가 EFT 의
    // EquipmentBuildsScreen 을 띄우고 그 위에 가격·3D 미리보기·구매를 얹는다. 자체 창으로 흉내 내면
    // 모양이 다를 수밖에 없어(2026-11-02 인게임에서 확인) 같은 화면을 쓴다.
    //
    // **PIT 의 흐름을 그대로 타되 두 자리에서만 갈라진다**(인계 §3-4: PIT 파일에는 분기를 앞에 거는 것만 한다):
    //   · 줄의 가격 라벨 - STM 모드에서는 **아무 숫자도 쓰지 않는다**. PIT 의 키트 값은 용병용 할인 규칙이
    //     붙은 값이고 우리가 청구하는 값과 다르다. 보여 준 숫자와 청구가 다른 것이 가장 나쁜 쪽이므로,
    //     고르기 전에는 값을 말하지 않고 고른 뒤 서버가 매긴 값만 보여 준다.
    //   · 구매 - PIT 의 키트 구매(플레이어 스태시를 출처로 쓰는 길)가 아니라 STM 의 경로로 간다.
    //
    // 지금은 **플레이어 자신의 빌드**만 이 화면에 뜬다. 그 화면이 session.EquipmentBuildsStorage 를 읽는데
    // 그것은 플레이어의 보관소이기 때문이다(EquipmentBuildsScreen 역컴파일로 확인). 팀원 본인의 빌드를
    // 이 목록에 끼워 넣는 것은 플레이어의 보관소를 건드리는 일이라 따로 한다 - 그때까지 팀원 빌드는
    // 상세 화면의 작은 창에서 고른다.
    internal static class StmKitNativeFlow
    {
        private const string StmKitQuoteRoute = "/stm/members/kit-quote";
        private const string StmKitBuyRoute = "/stm/members/kit-buy";

        // 기본 화면에 뜨는 것은 플레이어의 빌드다. 서버에 어느 쪽인지 말해 주는 값이고, 팀원 빌드를
        // 끼워 넣게 되면 고른 줄에 따라 갈라진다.
        private const string Owner = "player";

        private const string QuoteFailedToast = "값을 매기지 못했습니다";
        private const string BuyFailedToast = "장비를 사지 못했습니다";

        private static ResultProfile _profile;
        private static StmProfileDetailWire _detail;
        private static bool _busy;
        private static GameObject _overlay;

        internal static bool IsActive => _profile != null && _detail != null;

        // 기본 화면을 열기 직전에 부른다. PIT 의 Open 이 그 다음에 화면을 띄운다.
        internal static void Begin(ResultProfile profile, StmProfileDetailWire detail)
        {
            _profile = profile;
            _detail = detail;
            CloseOverlay();
        }

        // 화면을 떠날 때. 모드가 남아 있으면 용병 화면에서도 STM 분기가 걸린다.
        internal static void End()
        {
            _profile = null;
            _detail = null;
            CloseOverlay();
        }

        // PIT 의 HandleBuyRequest 가 STM 모드일 때 넘겨준다. true 면 PIT 는 자기 구매를 하지 않는다.
        internal static bool HandleBuyRequest(string selectedBuildId, string selectedBuildName)
        {
            if (!IsActive) return false;

            if (string.IsNullOrWhiteSpace(selectedBuildId))
            {
                EFT.Communications.NotificationManager.DisplayWarningNotification(
                    "고른 빌드를 읽지 못했습니다.", EFT.Communications.ENotificationDurationType.Default);
                return true;
            }

            RequestQuote(selectedBuildId, selectedBuildName);
            return true;
        }

        private static async void RequestQuote(string buildId, string buildName)
        {
            if (_busy) return;
            _busy = true;
            try
            {
                ShowMessage("값을 묻는 중입니다...");

                string body = JsonConvert.SerializeObject(new
                {
                    aid = _profile.AccountId,
                    profileId = _detail.ProfileId,
                    buildId,
                    owner = Owner,
                });
                string json = await Task.Run(() => RequestHandler.PostJson(StmKitQuoteRoute, body));

                string failure = ReadFailure(json, QuoteFailedToast);
                if (failure != null)
                {
                    ShowMessage(failure);
                    return;
                }

                StmKitQuoteWire quote = ReadData<StmKitQuoteWire>(json);
                if (quote == null)
                {
                    ShowMessage(QuoteFailedToast);
                    return;
                }

                ShowQuote(quote, buildName);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 기본 화면에서 꾸러미 값을 묻다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                ShowMessage(QuoteFailedToast);
            }
            finally
            {
                _busy = false;
            }
        }

        private static async void SubmitBuy(StmKitQuoteWire quote)
        {
            if (_busy) return;
            _busy = true;
            try
            {
                ShowMessage("사는 중입니다. 끝날 때까지 창을 닫지 마세요...");

                string body = JsonConvert.SerializeObject(new
                {
                    aid = _profile.AccountId,
                    profileId = _detail.ProfileId,
                    buildId = quote.BuildId,
                    owner = Owner,
                    // 화면이 보여 준 값. 서버가 다시 매겨 다르면 거절한다.
                    expectedTotal = quote.Total,
                });
                string json = await Task.Run(() => RequestHandler.PostJson(StmKitBuyRoute, body));

                string failure = ReadFailure(json, BuyFailedToast);
                if (failure != null)
                {
                    ShowMessage(failure);
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                StmKitBuyWire bought = ReadData<StmKitBuyWire>(json);
                CloseOverlay();
                AddTeammateCreationFlow.ShowToast(bought == null
                    ? "장비를 샀습니다."
                    : $"{bought.BuildName} 을(를) 샀습니다. {bought.Paid:N0} ₽, 물건 {bought.ItemsBought}점.");
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 기본 화면에서 꾸러미 구매 요청이 실패했다.");
                pitFireTeam.Log.LogError(ex);
                // 요청이 나간 뒤의 실패는 "샀는지 모른다" 다.
                ShowMessage("요청이 끝났는지 알 수 없습니다. 팀원 화면을 다시 열어 장비와 돈을 확인하세요.");
            }
            finally
            {
                _busy = false;
            }
        }

        // ---- 창 ----

        private static void ShowQuote(StmKitQuoteWire quote, string buildName)
        {
            string name = string.IsNullOrWhiteSpace(quote.BuildName) ? buildName : quote.BuildName;
            string text =
                $"{name}\n\n" +
                $"값: {quote.Total:N0} ₽   (가진 돈 {quote.PlayerRoubles:N0} ₽)\n" +
                $"살 물건 {quote.ItemsToBuy}점, 이 팀원 스태시로 돌아갈 헌 장비 {quote.ItemsToStash}점\n\n" +
                "물건은 전부 새로 삽니다. 돈은 플레이어 계정(스태시)에서 나갑니다.\n" +
                "돈은 되돌릴 수 없습니다.";

            var panel = CreatePanel(text);
            if (panel == null) return;

            CreateButton(panel, new Vector2(40f, 20f), new Vector2(180f, 36f), "닫기", CloseOverlay);

            bool canBuy = quote.Affordable && quote.ItemsToBuy > 0;
            var buy = CreateButton(panel, new Vector2(260f, 20f), new Vector2(180f, 36f),
                canBuy ? "구매" : "살 수 없음", canBuy ? (Action)(() => SubmitBuy(quote)) : null);
            if (!canBuy) buy.interactable = false;
        }

        private static void ShowMessage(string message)
        {
            var panel = CreatePanel(message);
            if (panel == null) return;
            CreateButton(panel, new Vector2(150f, 20f), new Vector2(180f, 36f), "닫기", CloseOverlay);
        }

        // 기본 화면 위에 얹는 작은 창. 화면 자체는 EFT 의 것이므로 거기에 손대지 않고 위에만 그린다.
        private static RectTransform CreatePanel(string text)
        {
            CloseOverlay();

            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                pitFireTeam.Log.LogWarning("[STM] 꾸러미 창을 띄울 캔버스를 찾지 못했다.");
                return null;
            }

            GameObject root = new GameObject("pitFireTeam_StmKitNativeOverlay", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster));
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 기본 화면보다 위에 그린다. 화면의 다른 버튼이 눌리지 않게 뒤를 막는다.
            Canvas overlayCanvas = root.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 30000;

            Image backdrop = root.GetComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.6f);
            backdrop.raycastTarget = true;

            GameObject panelObject = new GameObject("pitFireTeam_StmKitNativePanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(root.transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(480f, 260f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.05f, 0.05f, 0.05f, 0.98f);
            panelImage.raycastTarget = true;

            GameObject textObject = new GameObject("pitFireTeam_StmKitNativeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(panel, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 66f);
            textRect.offsetMax = new Vector2(-24f, -20f);

            TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.color = new Color(0.9f, 0.88f, 0.84f, 1f);
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;

            _overlay = root;
            return panel;
        }

        private static Button CreateButton(RectTransform panel, Vector2 position, Vector2 size, string text, Action onClick)
        {
            GameObject buttonObject = new GameObject($"pitFireTeam_StmKitNativeButton_{text}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panel, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image background = buttonObject.GetComponent<Image>();
            background.color = new Color(0.16f, 0.16f, 0.15f, 1f);

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(rect, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.9f, 0.88f, 0.84f, 1f);
            label.raycastTarget = false;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.onClick.RemoveAllListeners();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            return button;
        }

        private static void CloseOverlay()
        {
            if (_overlay == null) return;
            GameObject.Destroy(_overlay);
            _overlay = null;
        }

        // ---- 응답 읽기 ----

        private static string ReadFailure(string json, string fallback)
        {
            if (string.IsNullOrWhiteSpace(json)) return fallback;
            try
            {
                var root = Newtonsoft.Json.Linq.JToken.Parse(json);
                int err = root["err"] != null ? root["err"].ToObject<int>() : -1;
                if (err == 0) return null;
                string errmsg = root["errmsg"]?.ToString();
                return string.IsNullOrWhiteSpace(errmsg) ? fallback : errmsg;
            }
            catch
            {
                return fallback;
            }
        }

        private static T ReadData<T>(string json) where T : class
        {
            try
            {
                var root = Newtonsoft.Json.Linq.JToken.Parse(json);
                var data = root["data"];
                return data?.ToObject<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}
