using System;
using System.Collections;
using System.Collections.Generic;
using EFT.UI;
using HarmonyLib;
using TMPro;
using TraderListView.Patches;
using UnityEngine;
using UnityEngine.UI;

namespace TraderListView.Ui
{
    /// <summary>
    /// 상인 화면 헤더 한 줄: <c>[상점 새로고침][검색창][✎][▦|☰]</c> (설계 §5, 사용자 결정 2026-11-18).
    /// Trader Search·Assort Editor 가 자리를 잡은 뒤 다시 놓는다. 둘 중 없는 것은 건너뛴다.
    /// 격자 모드에서는 헤더 아래에 정렬 막대를 둔다.
    /// </summary>
    public sealed class HeaderLayout : MonoBehaviour
    {
        private const string SearchFieldName = "TraderSearchField";
        private const string EditButtonName = "UTAE Edit Assorts Button";
        private const string EditorTypeName = "UltimateTraderAssortEditor.Client.Editor.TraderEditorController";
        private const float Gap = 4f;
        private const float SortBarHeight = 26f;

        private static readonly (SortKey key, string label)[] SortButtons =
        {
            (SortKey.Default, "기본"), (SortKey.Price, "가격"), (SortKey.Name, "이름"),
            (SortKey.Stock, "재고"), (SortKey.Loyalty, "등급"),
        };

        private TraderDealScreen _screen;
        private TMP_FontAsset _font;
        private RectTransform _toggle;
        private TextMeshProUGUI _toggleLabel;
        private RectTransform _sortBar;
        private readonly List<(SortKey key, string label, Button button, TextMeshProUGUI text)> _sortItems =
            new List<(SortKey, string, Button, TextMeshProUGUI)>();
        private RectTransform _scrollRect;
        private Vector2? _scrollOffsetMax;
        private bool _lastListAllowed = true;
        private float _nextPoll;
        private Coroutine _pending;

        /// <summary>"Grid"/"List" 로 바뀔 때.</summary>
        public event Action<string> ViewModeChanged;

        /// <summary>목록을 보여도 되는 상황(구매 모드이고 Assort Editor 편집 중이 아님)이 바뀔 때.</summary>
        public event Action<bool> ListAllowedChanged;

        public bool ListAllowed => _screen != null && _screen.TradeMode == ETradeMode.Purchase && !IsEditing();

        public static HeaderLayout Attach(TraderDealScreen screen)
        {
            var layout = screen.GetComponent<HeaderLayout>() ?? screen.gameObject.AddComponent<HeaderLayout>();
            layout.Init(screen);
            return layout;
        }

        private void Init(TraderDealScreen screen)
        {
            if (_screen == screen && _toggle != null)
            {
                return;
            }

            _screen = screen;
            _font = UiKit.BorrowFont(screen._updateAssort);
            _scrollRect = screen._traderScroll != null ? (RectTransform)screen._traderScroll.transform : null;
            BuildToggle();
            BuildSortBar();
            screen.OnTradingModeChanged += _ => UpdateVisibility();
        }

        /// <summary>Show 직후 한 번, 다른 모드의 프레임 끝 재배치가 끝난 뒤 한 번 더.</summary>
        public void OnShown()
        {
            Relayout();
            UpdateVisibility();
            if (_pending != null)
            {
                StopCoroutine(_pending);
            }

            _pending = StartCoroutine(RelayoutLater());
        }

        private IEnumerator RelayoutLater()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            _pending = null;
            Relayout();
            UpdateVisibility();
        }

        public void Relayout()
        {
            try
            {
                RelayoutCore();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("헤더 배치 실패: " + e);
            }
        }

        private void RelayoutCore()
        {
            var refresh = _screen._updateAssort != null ? (RectTransform)_screen._updateAssort.transform : null;
            var allItems = _screen._allItemsTab != null ? (RectTransform)_screen._allItemsTab.transform : null;
            var space = refresh != null ? refresh.parent as RectTransform : null;
            if (refresh == null || allItems == null || space == null)
            {
                return;
            }

            var r = UiKit.BoundsIn(refresh, space);
            var a = UiKit.BoundsIn(allItems, space);
            float h = r.height;
            float left = r.xMax + Gap;
            float right = a.xMin - Gap;
            if (right - left < h * 4)
            {
                return;
            }

            // 오른쪽부터: 전환 버튼(하나, 높이의 2.6배 폭) → 연필 → 검색창이 남은 폭.
            float x = right;
            var toggleRect = Rect.MinMaxRect(x - h * 2.6f, r.yMin, x, r.yMax);
            if (_toggle.parent != space)
            {
                _toggle.SetParent(space, false);
            }

            UiKit.PlaceIn(_toggle, space, toggleRect);
            x = toggleRect.xMin - Gap * 2; // 연필과 붙어 있으면 잘못 눌린다(게임 확인 2026-11-18).

            // 두 모드 모두 헤더 부모(space)의 직계 자식으로 붙인다 — 화면 전체를 훑지 않는다.
            var edit = space.Find(EditButtonName) as RectTransform;
            if (edit != null && edit.gameObject.activeSelf)
            {
                var editRect = Rect.MinMaxRect(x - h, r.yMin, x, r.yMax);
                UiKit.PlaceIn(edit, space, editRect);
                MakePencilOnly(edit);
                x = editRect.xMin - Gap;
            }

            var search = space.Find(SearchFieldName) as RectTransform;
            if (search != null)
            {
                UiKit.PlaceIn(search, space, Rect.MinMaxRect(left, r.yMin, Mathf.Max(left + h, x), r.yMax));
            }
        }

        /// <summary>EDIT ASSORTS 글자를 끄고 연필 아이콘 층을 가운데로.</summary>
        private static void MakePencilOnly(RectTransform edit)
        {
            foreach (var label in edit.GetComponentsInChildren<TMP_Text>(true))
            {
                label.enabled = false;
            }

            foreach (var text in edit.GetComponentsInChildren<Text>(true))
            {
                text.enabled = false;
            }

            var center = edit.TransformPoint(edit.rect.center);
            foreach (var image in edit.GetComponentsInChildren<Image>(true))
            {
                var rt = image.rectTransform;
                if (rt == edit || rt.anchorMin != rt.anchorMax || rt.rect.width > 40f)
                {
                    continue; // 버튼 바탕·늘어나는 층은 그대로.
                }

                var local = rt.parent.InverseTransformPoint(center);
                var pos = rt.localPosition;
                pos.x = local.x;
                rt.localPosition = pos;
            }
        }

        private void BuildToggle()
        {
            // 버튼 하나로 격자 ↔ 목록을 오간다. 정사각형 둘(▦|☰)은 8px 남짓으로 작아 연필과 함께 잘못 눌렸고,
            // ▦·☰ 글리프는 빈칸으로 그려졌다(게임 확인 2026-11-18). 글자는 "바꿀 곳"을 보인다.
            var button = UiKit.NewButton("TraderListView Toggle", _screen._updateAssort.transform.parent, _font, 13f,
                () => SetViewMode(Plugin.IsListMode ? "Grid" : "List"), out _toggleLabel);
            _toggle = (RectTransform)button.transform;
            _toggleLabel.enableAutoSizing = true;
            _toggleLabel.fontSizeMin = 9f;
            _toggleLabel.fontSizeMax = 13f;
            _toggleLabel.color = UiKit.TextActive;
            _toggleLabel.text = "목록 보기";
        }

        private void BuildSortBar()
        {
            if (_scrollRect == null)
            {
                return;
            }

            _sortBar = UiKit.NewRect("TraderListView SortBar", _scrollRect.parent);
            var row = _sortBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 2f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = true;
            row.padding = new RectOffset(2, 2, 2, 2);

            var caption = UiKit.NewLabel(_sortBar, _font, 13f, TextAlignmentOptions.MidlineLeft);
            caption.text = "정렬";
            caption.gameObject.AddComponent<LayoutElement>().preferredWidth = 40f;

            foreach (var (key, label) in SortButtons)
            {
                var k = key;
                var button = UiKit.NewButton("Sort " + key, _sortBar, _font, 13f, () => OnSortClicked(k), out var text);
                button.gameObject.AddComponent<LayoutElement>().preferredWidth = 64f;
                _sortItems.Add((key, label, button, text));
            }

            RefreshSortLabels();
        }

        public void OnSortClicked(SortKey key)
        {
            var current = Plugin.CurrentSort;
            var next = current.Key == key && key != SortKey.Default
                ? new SortSpec(key, !current.Descending)
                : new SortSpec(key, false);
            Plugin.Sort.Value = next.ToConfig();
            RefreshSortLabels();
            TraderGridSortPatch.Refresh(_screen);
        }

        public void RefreshSortLabels()
        {
            var current = Plugin.CurrentSort;
            foreach (var (key, label, button, text) in _sortItems)
            {
                bool active = current.Key == key;
                string arrow = active && key != SortKey.Default ? (current.Descending ? " ▼" : " ▲") : string.Empty;
                UiKit.SetText(text, label + arrow);
                text.color = active ? UiKit.TextActive : UiKit.Text;
                ((Image)button.targetGraphic).color = active ? UiKit.ButtonBackActive : UiKit.ButtonBack;
            }
        }

        private void SetViewMode(string mode)
        {
            if (Plugin.ViewMode.Value != mode)
            {
                Plugin.ViewMode.Value = mode;
            }

            UpdateVisibility();
            ViewModeChanged?.Invoke(mode);
        }

        public void UpdateVisibility()
        {
            if (_screen == null)
            {
                return;
            }

            bool allowed = ListAllowed;
            bool list = allowed && Plugin.IsListMode;
            _toggle.gameObject.SetActive(allowed);
            _toggleLabel.text = list ? "격자 보기" : "목록 보기";
            SetSortBarVisible(allowed && !list);

            if (allowed != _lastListAllowed)
            {
                _lastListAllowed = allowed;
                ListAllowedChanged?.Invoke(allowed);
            }
        }

        /// <summary>정렬 막대만큼 상인 격자 스크롤의 위를 줄인다(덮지 않는다).</summary>
        private void SetSortBarVisible(bool on)
        {
            if (_sortBar == null || _scrollRect == null)
            {
                return;
            }

            if (_scrollOffsetMax == null)
            {
                _scrollOffsetMax = _scrollRect.offsetMax;
            }

            var top = _scrollOffsetMax.Value;
            _scrollRect.offsetMax = on ? new Vector2(top.x, top.y - SortBarHeight) : top;
            _sortBar.gameObject.SetActive(on);
            if (on)
            {
                // 스크롤의 위 끝 앵커에 붙여, 원래 스크롤 위쪽 SortBarHeight 만큼을 차지한다.
                _sortBar.anchorMin = new Vector2(_scrollRect.anchorMin.x, _scrollRect.anchorMax.y);
                _sortBar.anchorMax = _scrollRect.anchorMax;
                _sortBar.pivot = new Vector2(0.5f, 1f);
                _sortBar.offsetMin = new Vector2(_scrollRect.offsetMin.x, top.y - SortBarHeight);
                _sortBar.offsetMax = new Vector2(top.x, top.y);
                _sortBar.SetAsLastSibling();
            }
        }

        private void Update()
        {
            // Assort Editor 편집 시작·끝을 따라간다(이벤트가 없어 4분의 1초마다 본다).
            if (Time.unscaledTime < _nextPoll)
            {
                return;
            }

            _nextPoll = Time.unscaledTime + 0.25f;
            if (ListAllowed != _lastListAllowed)
            {
                UpdateVisibility();
            }
        }

        private const string EditorPluginGuid = "com.goatboy.ultimate-trader-assort-editor";

        // 타입·속성 찾기는 한 번만(못 찾은 결과도 담는다). 모드가 없으면 타입을 찾지도 않는다 —
        // AccessTools.TypeByName 은 못 찾으면 전 어셈블리를 훑고 경고를 찍는다.
        private static bool _editorResolved;
        private static Type _editorType;
        private static System.Reflection.PropertyInfo _editorIsEditing;

        private static void ResolveEditor()
        {
            if (_editorResolved)
            {
                return;
            }

            _editorResolved = true;
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(EditorPluginGuid))
            {
                return;
            }

            _editorType = AccessTools.TypeByName(EditorTypeName);
            _editorIsEditing = _editorType != null ? AccessTools.Property(_editorType, "IsEditing") : null;
        }

        private bool IsEditing()
        {
            ResolveEditor();
            if (_editorType == null || _editorIsEditing == null)
            {
                return false;
            }

            var editor = _screen.GetComponent(_editorType);
            return editor != null && _editorIsEditing.GetValue(editor) is bool b && b;
        }
    }
}
