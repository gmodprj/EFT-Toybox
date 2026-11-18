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
    /// 상인 화면 헤더 한 줄: <c>[↻][✎][☰][검색창 ...][▦ I II III ♛]</c> — 버튼 셋을 등급 탭과 같은 정사각형으로(사용자 요청 2026-11-18).
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

            LogStructureOnce(space, refresh, allItems);

            // 기준은 등급 탭(▦ I II III ♛)의 정사각형. 새로고침·연필·전환을 같은 크기로 맞추고
            // 남은 폭을 같은 높이의 검색창이 쓴다(사용자 요청 2026-11-18):
            //   [↻][✎][☰] [검색창 ............] [▦][I][II][III][♛]
            var r = UiKit.BoundsIn(refresh, space);
            var a = UiKit.BoundsIn(allItems, space);
            float s = a.height;
            float yMin = a.yMin, yMax = a.yMax;
            float x = r.xMin;
            float right = a.xMin - Gap;
            if (right - x < s * 5)
            {
                return;
            }

            var tabBack = TabBackground(allItems);

            PlaceSquare(refresh, space, ref x, yMin, yMax, s, tabBack);
            AddTooltip(refresh, () => "상점 새로고침");

            // 두 모드 모두 헤더 부모(space)의 직계 자식으로 붙인다 — 화면 전체를 훑지 않는다.
            var edit = space.Find(EditButtonName) as RectTransform;
            if (edit != null && edit.gameObject.activeSelf)
            {
                PlaceSquare(edit, space, ref x, yMin, yMax, s, tabBack);
            }

            if (_toggle.parent != space)
            {
                _toggle.SetParent(space, false);
            }

            UiKit.PlaceIn(_toggle, space, Rect.MinMaxRect(x, yMin, x + s, yMax));
            ApplyBackground(_toggle, tabBack);
            x += s + Gap;

            var search = space.Find(SearchFieldName) as RectTransform;
            if (search != null)
            {
                UiKit.PlaceIn(search, space, Rect.MinMaxRect(x, yMin, Mathf.Max(x + s, right), yMax));
            }
        }

        /// <summary>정사각형 칸에 놓고, 글자를 끄고, 작은 아이콘 층을 가운데로, 바탕을 등급 탭 것으로.</summary>
        private static void PlaceSquare(RectTransform target, RectTransform space, ref float x, float yMin, float yMax, float s, Image tabBack)
        {
            // 글자 길이에 맞춰 폭을 다시 늘리는 부품이 있으면 끈다.
            var fitter = target.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
            }

            UiKit.PlaceIn(target, space, Rect.MinMaxRect(x, yMin, x + s, yMax));
            MakeIconOnly(target, s);
            ApplyBackground(target, tabBack);
            x += s + Gap;
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

            var center = button.TransformPoint(button.rect.center);
            foreach (var image in button.GetComponentsInChildren<Image>(true))
            {
                var rt = image.rectTransform;
                if (rt == button || rt.anchorMin != rt.anchorMax || rt.rect.width > size)
                {
                    continue; // 버튼 바탕·늘어나는 층은 그대로.
                }

                var local = rt.parent.InverseTransformPoint(center);
                rt.localPosition = new Vector3(local.x, local.y, rt.localPosition.z);
            }
        }

        /// <summary>등급 탭의 바탕(그림이 있는 가장 큰 Image).</summary>
        private static Image TabBackground(RectTransform tab)
        {
            Image best = null;
            float bestArea = 0f;
            foreach (var image in tab.GetComponentsInChildren<Image>(true))
            {
                var area = image.rectTransform.rect.width * image.rectTransform.rect.height;
                if (image.sprite != null && area > bestArea)
                {
                    best = image;
                    bestArea = area;
                }
            }

            return best;
        }

        /// <summary>버튼 자기 바탕(루트 Image)을 등급 탭 바탕의 그림·색으로.</summary>
        private static void ApplyBackground(RectTransform button, Image tabBack)
        {
            var own = button.GetComponent<Image>();
            if (own == null || tabBack == null)
            {
                return;
            }

            own.sprite = tabBack.sprite;
            own.type = tabBack.type;
            own.color = tabBack.color;
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

        private RectTransform _gridIcon, _listIcon;

        private void BuildToggle()
        {
            // 등급 탭과 같은 정사각형 아이콘 버튼 하나로 격자 ↔ 목록을 오간다. ▦·☰ 글리프는 빈칸으로 그려져
            // (게임 확인 2026-11-18) 작은 Image 로 직접 그린다: 격자 = 네모 넷, 목록 = 가로줄 셋. 보이는 그림은 "바꿀 곳".
            var button = UiKit.NewButton("TraderListView Toggle", _screen._updateAssort.transform.parent, _font, 13f,
                () => SetViewMode(Plugin.IsListMode ? "Grid" : "List"), out var label);
            label.enabled = false;
            _toggle = (RectTransform)button.transform;
            _gridIcon = DrawIcon(_toggle, "GridIcon", new[]
            {
                new Rect(0.22f, 0.54f, 0.24f, 0.24f), new Rect(0.54f, 0.54f, 0.24f, 0.24f),
                new Rect(0.22f, 0.22f, 0.24f, 0.24f), new Rect(0.54f, 0.22f, 0.24f, 0.24f),
            });
            _listIcon = DrawIcon(_toggle, "ListIcon", new[]
            {
                new Rect(0.22f, 0.66f, 0.56f, 0.10f), new Rect(0.22f, 0.45f, 0.56f, 0.10f), new Rect(0.22f, 0.24f, 0.56f, 0.10f),
            });
            AddTooltip(_toggle, () => Plugin.IsListMode ? "격자 보기" : "목록 보기");
        }

        /// <summary>버튼 안에 비율 사각형들로 아이콘을 그린다.</summary>
        private static RectTransform DrawIcon(RectTransform parent, string name, Rect[] parts)
        {
            var icon = UiKit.NewRect(name, parent);
            UiKit.Stretch(icon);
            foreach (var p in parts)
            {
                var piece = UiKit.NewRect("Piece", icon);
                piece.anchorMin = new Vector2(p.xMin, p.yMin);
                piece.anchorMax = new Vector2(p.xMax, p.yMax);
                piece.offsetMin = Vector2.zero;
                piece.offsetMax = Vector2.zero;
                var image = piece.gameObject.AddComponent<Image>();
                image.color = UiKit.Text;
                image.raycastTarget = false;
            }

            return icon;
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
            // 바꿀 곳의 그림을 보인다: 목록이면 격자 그림, 격자면 목록 그림.
            _gridIcon.gameObject.SetActive(list);
            _listIcon.gameObject.SetActive(!list);
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

    /// <summary>마우스를 올리면 게임의 간단 툴팁으로 글자를 보인다(글자가 없는 아이콘 버튼용).</summary>
    public sealed class HoverTooltip : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
    {
        public Func<string> Text;

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            var context = ItemUiContext.Instance;
            var text = Text?.Invoke();
            if (context != null && !string.IsNullOrEmpty(text))
            {
                context.Tooltip.Show(text);
            }
        }

        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            var context = ItemUiContext.Instance;
            if (context != null)
            {
                context.Tooltip.Close();
            }
        }
    }
}
