using EFT;
using EFT.UI;
using Newtonsoft.Json;
using pitTeam.Modules;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
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
    // 팀원 빌드도 이 화면에 뜬다(2026-11-02). 화면이 session.EquipmentBuildsStorage 를 읽으므로 그 안의
    // 사전 **참조만** 잠깐 바꿔 끼우고 닫을 때 되돌린다 - 플레이어의 사전 객체 자체는 건드리지 않는다.
    // 어느 쪽 빌드가 실렸는지가 구매 요청의 owner 를 가른다(CurrentOwner).
    internal static class StmKitNativeFlow
    {
        private const string StmKitQuoteRoute = "/stm/members/kit-quote";
        private const string StmKitBuyRoute = "/stm/members/kit-buy";

        private const string OwnerPlayer = "player";
        private const string OwnerTeammate = "teammate";

        private const string QuoteFailedToast = "값을 매기지 못했습니다";
        private const string BuyFailedToast = "장비를 사지 못했습니다";

        private static ResultProfile _profile;
        private static StmProfileDetailWire _detail;
        private static bool _busy;
        private static GameObject _overlay;

        // 팀원 빌드를 보고 있는 동안만 채워진다. 구매 요청의 owner 가 이 값으로 갈라진다.
        private static bool _teammateBuilds;

        // 화면의 빌드 보관소를 잠깐 바꿔 끼운 상태. 원래 사전을 그대로 들고 있다가 되돌린다.
        // **플레이어의 사전 객체 자체는 건드리지 않는다** - 참조만 바꿔 끼우므로 원본은 손상되지 않는다.
        private static EFT.UI.Builds.EquipmentBuildsStorage _swappedStorage;
        private static Dictionary<MongoID, EFT.UI.Builds.EquipmentBuild> _originalBuilds;

        internal static bool IsActive => _profile != null && _detail != null;

        // 지금 화면에 실린 빌드가 누구 것인가. 구매 요청이 이 값으로 갈라진다 - 화면에 보이는 것과
        // 서버가 찾는 것이 어긋나면 엉뚱한 빌드를 입힌다.
        private static string CurrentOwner => _teammateBuilds ? OwnerTeammate : OwnerPlayer;

        // 기본 화면을 열기 직전에 부른다. PIT 의 Open 이 그 다음에 화면을 띄운다.
        //
        // teammateBuilds 가 있으면 **그 팀원의 빌드를 화면에 싣는다**: 화면은 session.EquipmentBuildsStorage 를
        // 읽으므로(역컴파일 확인) 그 안의 사전 참조를 잠깐 바꿔 끼운다. 플레이어의 사전 객체는 그대로 두고
        // 우리가 만든 사전을 대신 보여 주는 것이라, 되돌리면 흔적이 남지 않는다.
        internal static void Begin(
            ResultProfile profile,
            StmProfileDetailWire detail,
            StmGearBuildRowWire[] teammateBuilds,
            EFT.IEftSession session)
        {
            // 앞선 창이 제대로 닫히지 않았을 수 있다. 끼우기 전에 반드시 되돌린다(이중 안전장치).
            RestoreBuilds();

            _profile = profile;
            _detail = detail;
            _teammateBuilds = false;
            CloseOverlay();

            if (teammateBuilds == null || teammateBuilds.Length == 0) return;
            _teammateBuilds = TrySwapBuilds(session, teammateBuilds);
        }

        // 팀원 빌드를 EFT 의 EquipmentBuild 로 만들어 화면의 보관소에 끼운다.
        // 실패하면 false - 그러면 플레이어 빌드가 그대로 보이고, 구매도 플레이어 쪽으로 간다(거짓말하지 않는다).
        private static bool TrySwapBuilds(EFT.IEftSession session, StmGearBuildRowWire[] rows)
        {
            try
            {
                var storage = session?.EquipmentBuildsStorage;
                if (storage == null)
                {
                    pitFireTeam.Log.LogWarning("[STM] 팀원 빌드를 실을 보관소를 찾지 못했다. 플레이어 빌드가 보인다.");
                    return false;
                }

                var replacement = new Dictionary<MongoID, EFT.UI.Builds.EquipmentBuild>();
                foreach (var row in rows)
                {
                    if (row?.Items == null || row.Items.Length == 0) continue;
                    if (string.IsNullOrWhiteSpace(row.Id) || string.IsNullOrWhiteSpace(row.Root)) continue;

                    var id = new MongoID(row.Id);
                    replacement[id] = new EFT.UI.Builds.EquipmentBuild(
                        id,
                        string.IsNullOrWhiteSpace(row.Name) ? row.Id : row.Name,
                        new MongoID(row.Root),
                        row.Items,
                        EFT.Builds.EEquipmentBuildType.Custom);
                }

                if (replacement.Count == 0) return false;

                _originalBuilds = storage._equipmentBuilds;
                _swappedStorage = storage;
                storage._equipmentBuilds = replacement;
                pitFireTeam.Log.LogInfo($"[STM] 기본 화면에 팀원 빌드 {replacement.Count}개를 실었다(원래 목록은 닫을 때 되돌린다).");
                return true;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 팀원 빌드를 화면에 싣다 실패했다. 원래 목록을 되돌린다.");
                pitFireTeam.Log.LogError(ex);
                RestoreBuilds();
                return false;
            }
        }

        // 끼워 둔 것을 반드시 되돌린다. 되돌리지 못하면 플레이어 자신의 프리셋 화면에 남의 빌드가 남으므로,
        // 실패해도 참조는 비워 다음 시도가 원본을 덮어쓰지 않게 한다.
        private static void RestoreBuilds()
        {
            if (_swappedStorage == null || _originalBuilds == null)
            {
                _swappedStorage = null;
                _originalBuilds = null;
                return;
            }

            try
            {
                _swappedStorage._equipmentBuilds = _originalBuilds;
                pitFireTeam.Log.LogInfo("[STM] 기본 화면의 빌드 목록을 플레이어의 것으로 되돌렸다.");
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 빌드 목록을 되돌리지 못했다. 게임을 다시 켜면 원래대로 돌아온다.");
                pitFireTeam.Log.LogError(ex);
            }
            finally
            {
                _swappedStorage = null;
                _originalBuilds = null;
            }
        }

        // 화면을 떠날 때. 모드가 남아 있으면 용병 화면에서도 STM 분기가 걸린다.
        internal static void End()
        {
            RestoreBuilds();
            _profile = null;
            _detail = null;
            _teammateBuilds = false;
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
                    owner = CurrentOwner,
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
                    owner = CurrentOwner,
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

            // **열려 있는 그 화면에 붙인다.** 처음에는 FindObjectOfType<Canvas>() 로 아무 캔버스나 잡았는데,
            // 유니티가 어느 것을 주는지 정해져 있지 않다 - 2026-11-02 인게임에서 `구매` 를 열한 번 눌렀는데
            // 창이 만들어지되 **보이지 않았다**(오류도 남지 않는다: 캔버스를 찾기는 했으니까).
            // PIT 은 같은 화면에서 TeammateEquipmentBuildsScreenFlow._activeScreen 에 붙인다. 같은 방식을 쓴다.
            Transform parent = TeammateEquipmentBuildsScreenFlow._activeScreen?.transform
                ?? GameObject.FindObjectOfType<Canvas>()?.transform;
            if (parent == null)
            {
                pitFireTeam.Log.LogWarning("[STM] 꾸러미 창을 띄울 자리를 찾지 못했다: 화면도 캔버스도 없다.");
                return null;
            }

            GameObject root = new GameObject("pitFireTeam_StmKitNativeOverlay", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 기본 화면보다 위에 그린다. 화면의 다른 버튼이 눌리지 않게 뒤를 막는다.
            Canvas overlayCanvas = root.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 30000;

            rootRect.SetAsLastSibling();
            pitFireTeam.Log.LogInfo($"[STM] 꾸러미 창을 '{parent.name}' 위에 띄웠다.");

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
