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
    /// 상인 화면 헤더 한 줄: <c>[↻][✎]  [☰|⋮⋮⋮]  [검색창][✕]  [▦ I II III ♛]</c> — 사용자 제안서(2026-11-18)의 그룹 배치·공통 버튼 규격.
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
        private RectTransform _toggle; // 보기 방식 두 칸 묶음 [목록][격자]
        private RectTransform _clear;  // 검색어 지우기
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

        private const float InnerGap = 3f;  // 같은 그룹 안 버튼 사이
        private const float GroupGap = 10f; // 그룹 사이

        private void RelayoutCore()
        {
            var refresh = _screen._updateAssort != null ? (RectTransform)_screen._updateAssort.transform : null;
            var allItems = _screen._allItemsTab != null ? (RectTransform)_screen._allItemsTab.transform : null;
            var space = refresh != null ? refresh.parent as RectTransform : null;
            if (refresh == null || allItems == null || space == null)
            {
                return;
            }

            LogStructureOnce(space, refresh, allItems);

            // 사용자 제안서(2026-11-18) §3 의 그룹 배치. 버튼은 등급 탭과 같은 정사각형(s×s):
            //   [↻][✎]  [☰|⋮⋮⋮]  [검색창 .............][✕]  [▦][I][II][III][♛]
            //   목록 관리·편집   보기 방식   검색                    (게임의 등급 탭)
            var r = UiKit.BoundsIn(refresh, space);
            var a = UiKit.BoundsIn(allItems, space);
            float s = a.height;
            float yMin = a.yMin, yMax = a.yMax;
            float right = a.xMin - GroupGap;
            // 묶음 전체를 오른쪽(등급 탭 쪽)으로 붙인다(사용자 요청 2026-11-18 "전체 위치를 우측으로").
            // ✕ 를 검색창 안에 넣어 따로 비워 두던 칸이 없어진 만큼 왼쪽 시작을 오른쪽으로 민다.
            float x = r.xMin + s + GroupGap;
            if (right - x < s * 7)
            {
                x = r.xMin;
            }

            if (right - x < s * 7)
            {
                return;
            }

            // 그룹 A·B: 새로고침 · 편집
            PlaceSquare(refresh, space, ref x, yMin, yMax, s);
            StyledButton.Apply(refresh, null, hideOwnBackground: true);
            AddTooltip(refresh, () => "상점 새로고침");
            x += InnerGap;

            // 두 모드 모두 헤더 부모(space)의 직계 자식으로 붙인다 — 화면 전체를 훑지 않는다.
            var edit = space.Find(EditButtonName) as RectTransform;
            if (edit != null && edit.gameObject.activeSelf)
            {
                PlaceSquare(edit, space, ref x, yMin, yMax, s);
                StyledButton.Apply(edit, IsEditing, hideOwnBackground: true);
                AddTooltip(edit, () => IsEditing() ? "편집 모드 끝내기" : "편집 모드");
            }

            // 그룹 C: 보기 방식 두 칸
            x += GroupGap - InnerGap;
            if (_toggle.parent != space)
            {
                _toggle.SetParent(space, false);
            }

            UiKit.PlaceIn(_toggle, space, Rect.MinMaxRect(x, yMin, x + s * 2, yMax));
            x += s * 2 + GroupGap;

            // 그룹 D: 검색창(등급 탭 바로 앞까지). ✕ 는 검색창 안 돋보기 왼쪽에 얹는다.
            var search = space.Find(SearchFieldName) as RectTransform;
            if (search != null)
            {
                float searchRight = Mathf.Max(x + s * 3, right);
                UiKit.PlaceIn(search, space, Rect.MinMaxRect(x, yMin, searchRight, yMax));
                if (_clear.parent != space)
                {
                    _clear.SetParent(space, false);
                }

                float c = s * 0.8f;
                float clearRight = searchRight - s * 0.9f; // 돋보기 칸을 비켜서
                float cy = (yMin + yMax) * 0.5f;
                UiKit.PlaceIn(_clear, space, Rect.MinMaxRect(clearRight - c, cy - c * 0.5f, clearRight, cy + c * 0.5f));
                _clear.SetAsLastSibling();
                var decor = SearchDecor.Attach(search, _clear);
                var clearButton = _clear.GetComponent<Button>();
                clearButton.onClick.RemoveAllListeners();
                clearButton.onClick.AddListener(decor.Clear);
            }
            else
            {
                _clear.gameObject.SetActive(false);
            }
        }

        /// <summary>정사각형 칸에 놓고, 글자를 끄고, 작은 아이콘 층을 가운데로.</summary>
        private static void PlaceSquare(RectTransform target, RectTransform space, ref float x, float yMin, float yMax, float s)
        {
            // 글자 길이에 맞춰 폭을 다시 늘리는 부품이 있으면 끈다.
            var fitter = target.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
            }

            UiKit.PlaceIn(target, space, Rect.MinMaxRect(x, yMin, x + s, yMax));
            MakeIconOnly(target, s);
            x += s;
        }

        /// <summary>글자를 끄고 작은(점 앵커) 아이콘 층을 버튼 가운데로.</summary>
        private static void MakeIconOnly(RectTransform button, float size)
        {
            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true))
            {
                label.enabled = false;
            }

            foreach (var text in button.GetComponentsInChildren<Text>(true))
            {
                text.enabled = false;
            }

            // 아이콘 층들의 기준점이 아니라 **그림이 실제로 차지하는 사각형**의 가운데를 버튼 가운데에 맞춘다
            // (기준점만 옮기면 피벗이 가운데가 아닌 층이 어긋난다 — 게임 확인 2026-11-18, 연필).
            var icons = new System.Collections.Generic.List<RectTransform>();
            foreach (var image in button.GetComponentsInChildren<Image>(true))
            {
                var rt = image.rectTransform;
                if (rt == button || rt.anchorMin != rt.anchorMax || rt.rect.width > size || rt.name.StartsWith("TLV ") || !image.enabled)
                {
                    continue; // 바탕·늘어나는 층·우리 층·꺼진 층은 그대로.
                }

                icons.Add(rt);
            }

            if (icons.Count == 0)
            {
                return;
            }

            var corners = new Vector3[4];
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue), max = new Vector3(float.MinValue, float.MinValue);
            foreach (var rt in icons)
            {
                rt.GetWorldCorners(corners);
                foreach (var c in corners)
                {
                    min = Vector3.Min(min, c);
                    max = Vector3.Max(max, c);
                }
            }

            var buttonCenter = button.TransformPoint(button.rect.center);
            var delta = buttonCenter - (min + max) * 0.5f;
            delta.z = 0f;
            foreach (var rt in icons)
            {
                rt.position += delta;
            }
        }

        private static bool _structureLogged;

        /// <summary>헤더 구조를 한 번만 로그에 남긴다 — 배치가 어긋나면 이것으로 본다.</summary>
        private static void LogStructureOnce(RectTransform space, RectTransform refresh, RectTransform allItems)
        {
            if (_structureLogged)
            {
                return;
            }

            _structureLogged = true;
            try
            {
                var sb = new System.Text.StringBuilder("헤더 구조: 부모 ").Append(space.name)
                    .Append(" HLG=").Append(space.GetComponent<HorizontalLayoutGroup>() != null)
                    .Append(" / 새로고침 ").Append(UiKit.BoundsIn(refresh, space))
                    .Append(" CSF=").Append(refresh.GetComponent<ContentSizeFitter>() != null)
                    .Append(" / 등급 탭 ").Append(UiKit.BoundsIn(allItems, space)).Append(" / 형제:");
                for (int i = 0; i < space.childCount; i++)
                {
                    var c = space.GetChild(i) as RectTransform;
                    if (c != null)
                    {
                        sb.Append(" [").Append(c.name).Append(c.gameObject.activeSelf ? "" : "(꺼짐)").Append("]");
                    }
                }

                Plugin.Log.LogInfo(sb.ToString());
            }
            catch (Exception)
            {
                // 기록은 덤이다.
            }
        }

        private static void AddTooltip(RectTransform target, Func<string> text)
        {
            var hover = target.GetComponent<HoverTooltip>() ?? target.gameObject.AddComponent<HoverTooltip>();
            hover.Text = text;
        }

        private void BuildToggle()
        {
            // 보기 방식 두 칸 묶음 [목록][격자](§4.4). 지금 보기 쪽이 활성 색. 격자 그림은 3×3 —
            // 등급 탭의 2×2(▦)와 겹치지 않게. ▦·☰ 글리프는 빈칸으로 그려져(게임 확인 2026-11-18) Image 로 그린다.
            var parent = _screen._updateAssort.transform.parent;
            _toggle = UiKit.NewRect("TraderListView ViewMode", parent);
            var list = SegmentButton(_toggle, "List", 0f, IconArt.List, () => Plugin.IsListMode, "List", "목록 보기");
            var grid = SegmentButton(_toggle, "Grid", 0.5f, IconArt.Grid, () => !Plugin.IsListMode, "Grid", "격자 보기");

            // 검색창 안에 얹는 ✕ — 틀 없이 아이콘만(검색창 바탕 위라서).
            _clear = UiKit.NewRect("TraderListView ClearSearch", parent);
            var hit = _clear.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);
            var clearButton = _clear.gameObject.AddComponent<Button>();
            clearButton.transition = Selectable.Transition.None;
            IconArt.DrawClear(_clear);
            AddTooltip(_clear, () => "검색어 지우기");
            _clear.gameObject.SetActive(false);
        }

        private RectTransform SegmentButton(RectTransform group, string name, float x0, Rect[] art, Func<bool> active, string mode, string tip)
        {
            var rt = UiKit.NewRect(name, group);
            rt.anchorMin = new Vector2(x0, 0f);
            rt.anchorMax = new Vector2(x0 + 0.5f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            IconArt.Draw(rt, name + "Icon", art);
            var button = StyledButton.Apply(rt, active, hideOwnBackground: false);
            button.OnClick = () => SetViewMode(mode);
            AddTooltip(rt, () => tip);
            return rt;
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
