using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using TMPro;
using TraderListView.Patches;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TraderListView.Ui
{
    /// <summary>
    /// 상인 물건 목록(설계 §5, 방식 B). 줄을 누르면 <c>Assortment.SelectItem</c> 만 부른다 —
    /// 사는 것은 언제나 원래 구매 창이다. 집합은 상인 격자의 <c>ApplyFilter</c> 결과와 같다.
    /// 보이는 줄만 만들어 다시 쓴다(수천 줄).
    /// </summary>
    public sealed class OfferListView : MonoBehaviour
    {
        internal const float RowHeight = 48f;
        internal const float HeaderHeight = 26f;
        private const float IconWidth = 52f, PriceWidth = 170f, StockWidth = 120f, LoyaltyWidth = 80f;

        private static readonly (SortKey key, string label)[] Columns =
        {
            (SortKey.Default, "기본"), (SortKey.Name, "이름"), (SortKey.Price, "가격"),
            (SortKey.Stock, "재고"), (SortKey.Loyalty, "등급"),
        };

        private TraderDealScreen _screen;
        private HeaderLayout _header;
        private TMP_FontAsset _font;
        private RectTransform _root, _viewport, _content;
        private ScrollRect _scroll;
        private CanvasGroup _gridHider;
        private TextMeshProUGUI _empty;
        private readonly List<(SortKey key, string label, TextMeshProUGUI text)> _columnLabels =
            new List<(SortKey, string, TextMeshProUGUI)>();
        private readonly List<OfferRowView> _pool = new List<OfferRowView>();

        private static readonly HarmonyLib.AccessTools.FieldRef<TraderDealScreen, Trader> TraderRef =
            HarmonyLib.AccessTools.FieldRefAccess<TraderDealScreen, Trader>("_trader");

        private IReadOnlyList<Item> _items = Array.Empty<Item>();
        private List<OfferRow> _rows = new List<OfferRow>();
        private Assortment _assortment;
        private Action _unsubscribeSelection, _unsubscribeUpdated;
        private bool _visible;
        private int _dataVersion, _boundVersion = -1;
        private VirtualWindow _boundWindow;

        /// <summary>줄 겉모습을 바꿀 고리(Task 6 의 플리 겉모습). (줄, 짝수/홀수)</summary>
        public static Action<GameObject, bool> RowSkin;

        /// <summary>열 머리 겉모습을 바꿀 고리.</summary>
        public static Action<GameObject> HeaderSkin;

        public static OfferListView Attach(TraderDealScreen screen, HeaderLayout header)
        {
            var view = screen.GetComponent<OfferListView>() ?? screen.gameObject.AddComponent<OfferListView>();
            view.Init(screen, header);
            view.OnShown();
            view.ApplyMode();
            return view;
        }

        /// <summary>지금 화면이 보여 주는 상인의 물건 묶음(상인을 바꾸면 달라진다).</summary>
        private Assortment CurrentAssortment()
        {
            var trader = _screen != null ? TraderRef(_screen) : null;
            return trader != null ? trader.CurrentAssortment : null;
        }

        /// <summary>
        /// 화면이 열릴 때: 목록이 다른 상인(또는 불러오기 전)의 것이면 비운다 —
        /// 옛 상인의 줄을 눌러 지금 구매 창과 다른 물건을 고르는 일이 없게(설계 §2).
        /// </summary>
        private void OnShown()
        {
            var current = CurrentAssortment();
            if (current == _assortment)
            {
                return;
            }

            SetAssortment(null);
            var trader = TraderRef(_screen);
            bool loading = current == null || (trader != null && trader.AssortmentLoading);
            SetItems(Array.Empty<Item>(), loading ? "불러오는 중..." : "물건이 없습니다");
        }

        private void Init(TraderDealScreen screen, HeaderLayout header)
        {
            if (_screen == screen && _root != null)
            {
                return;
            }

            _screen = screen;
            _header = header;
            _font = UiKit.BorrowFont(screen._updateAssort);
            Build();
            header.ViewModeChanged += _ => ApplyMode();
            header.ListAllowedChanged += _ => ApplyMode();
            TraderGridSortPatch.ListApplied += OnListApplied;
        }

        private void OnDestroy()
        {
            TraderGridSortPatch.ListApplied -= OnListApplied;
            _unsubscribeSelection?.Invoke();
            _unsubscribeUpdated?.Invoke();
        }

        private void ApplyMode()
        {
            SetVisible(_header != null && _header.ListAllowed && Plugin.IsListMode);
        }

        public void SetVisible(bool on)
        {
            _visible = on;
            if (_root == null)
            {
                return;
            }

            // 격자는 끄지 않는다(FilterPanel·ApplyFilter 가 계속 돌아야 목록이 따라온다) — 보이지만 않게.
            _gridHider.alpha = on ? 0f : 1f;
            _gridHider.blocksRaycasts = !on;
            _gridHider.interactable = !on;
            _root.gameObject.SetActive(on);
            if (on)
            {
                MatchScrollArea();
                TraderGridSortPatch.Refresh(_screen);
            }
        }

        private void OnListApplied(TradingGridView grid, IReadOnlyList<Item> items)
        {
            if (_screen == null || grid != _screen._traderGridView)
            {
                return;
            }

            SetAssortment(TraderGridSortPatch.AssortmentOf(grid));
            SetItems(items);
        }

        public void SetItems(IReadOnlyList<Item> items) => SetItems(items, "물건이 없습니다");

        private void SetItems(IReadOnlyList<Item> items, string emptyText)
        {
            _items = items ?? Array.Empty<Item>();
            int loyalty = TraderGridSortPatch.TraderLoyalty(_assortment);
            _rows = new List<OfferRow>(_items.Count);
            for (int i = 0; i < _items.Count; i++)
            {
                _rows.Add(OfferRowFactory.Create(_items[i], _assortment, i, loyalty));
            }

            _dataVersion++;
            if (_content == null)
            {
                return;
            }

            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rows.Count * RowHeight);
            _empty.text = emptyText;
            _empty.gameObject.SetActive(_rows.Count == 0);
            RefreshColumnLabels();
            Rebind();
        }

        private void SetAssortment(Assortment assortment)
        {
            if (assortment == _assortment)
            {
                return;
            }

            _unsubscribeSelection?.Invoke();
            _unsubscribeSelection = null;
            _unsubscribeUpdated?.Invoke();
            _unsubscribeUpdated = null;
            _assortment = assortment;
            if (assortment != null)
            {
                _unsubscribeSelection = assortment.SelectedItemChanged.Subscribe(UpdateSelection);
                _unsubscribeUpdated = assortment.AssortmentUpdated.Subscribe(OnAssortmentUpdated);
            }
        }

        /// <summary>
        /// 구매·새로고침 뒤: 물건 집합이 같으면 게임이 격자를 다시 채우지 않으므로(Id 해시) 목록이 스스로 고친다.
        /// 재고로 정렬 중이면 순서도 바뀌니 격자부터 다시 줄 세운다.
        /// </summary>
        private void OnAssortmentUpdated()
        {
            if (!_visible)
            {
                return;
            }

            if (Plugin.CurrentSort.Key == SortKey.Stock)
            {
                TraderGridSortPatch.Refresh(_screen);
            }
            else
            {
                SetItems(_items);
            }
        }

        private void UpdateSelection()
        {
            var selected = _assortment?.SelectedItem;
            foreach (var row in _pool)
            {
                row.SetSelected(selected != null && row.Item == selected);
            }
        }

        internal void OnRowClicked(OfferRowView row)
        {
            // 목록이 지금 상인의 것이 아니면(상인을 바꾸는 중) 고르지 않는다.
            if (_assortment == null || row.Item == null || _assortment != CurrentAssortment())
            {
                return;
            }

            try
            {
                _assortment.SelectItem(row.Item);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("목록에서 물건 고르기 실패: " + e);
            }
        }

        /// <summary>격자와 같은 가격 툴팁(<c>TradingItemView.ShowTooltip</c> 와 같은 인자).</summary>
        internal void ShowItemTooltip(Item item)
        {
            var context = ItemUiContext.Instance;
            if (context == null || item == null)
            {
                return;
            }

            try
            {
                var price = _assortment != null ? _assortment.GetBarterPrice(item).GetValueOrDefault() : default;
                string text = Comfort.Common.Singleton<ItemFactory>.Instance.BriefItemName(item, item.Name.Localized());
                if (item.StackObjectsCount <= 0)
                {
                    text += "\n<color=red>(" + "OUT OF STOCK".Localized().ToLower() + ")</color>";
                }

                context.PriceTooltip.Show(item.Owner.OwnerType, text, price.Amount, price.CurrencyId);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("목록 툴팁 실패: " + e.Message);
            }
        }

        internal void HideAllTooltips()
        {
            var context = ItemUiContext.Instance;
            if (context == null)
            {
                return;
            }

            context.PriceTooltip.Close();
            context.Tooltip.Close();
        }

        internal void ShowTooltip(string text)
        {
            var context = ItemUiContext.Instance;
            if (context != null && !string.IsNullOrEmpty(text))
            {
                context.Tooltip.Show(text);
            }
        }

        internal void HideTooltip()
        {
            var context = ItemUiContext.Instance;
            if (context != null)
            {
                context.Tooltip.Close();
            }
        }

        internal string PriceTooltip(Item item)
        {
            var parts = OfferRowFactory.Parts(item, _assortment);
            if (parts.Count < 2)
            {
                return null;
            }

            return string.Join("\n", parts.Select(p => OfferRowFactory.ShortName(p.TemplateId) + " ×" + p.Count));
        }

        internal string PriceText(Item item) => TraderListView.PriceText.Price(OfferRowFactory.Parts(item, _assortment), OfferRowFactory.ShortName);

        // ---------- 가상 스크롤 ----------

        private void Rebind()
        {
            if (_viewport == null)
            {
                return;
            }

            // 스크롤 매 프레임 다시 묶지 않는다 — 창(첫 줄·칸 수)이나 데이터가 바뀔 때만.
            var window = VirtualWindow.Compute(_content.anchoredPosition.y, RowHeight, _viewport.rect.height, _rows.Count);
            if (window.Equals(_boundWindow) && _boundVersion == _dataVersion)
            {
                return;
            }

            _boundWindow = window;
            _boundVersion = _dataVersion;
            int first = window.First;
            int needed = window.Slots;
            while (_pool.Count < needed)
            {
                _pool.Add(OfferRowView.Create(this, _content, _font, _pool.Count));
            }

            for (int i = 0; i < _pool.Count; i++)
            {
                int index = first + i;
                var view = _pool[i];
                if (i >= needed || index >= _rows.Count)
                {
                    view.Hide();
                    continue;
                }

                view.Bind(_rows[index], index);
            }

            UpdateSelection();
        }

        private void MatchScrollArea()
        {
            var scroll = (RectTransform)_screen._traderScroll.transform;
            _root.anchorMin = scroll.anchorMin;
            _root.anchorMax = scroll.anchorMax;
            _root.pivot = scroll.pivot;
            _root.offsetMin = scroll.offsetMin;
            _root.offsetMax = scroll.offsetMax;
            _root.SetAsLastSibling();
        }

        private void Build()
        {
            var scrollTransform = _screen._traderScroll.transform;
            _gridHider = scrollTransform.GetComponent<CanvasGroup>() ?? scrollTransform.gameObject.AddComponent<CanvasGroup>();

            _root = UiKit.NewRect("TraderListView List", scrollTransform.parent);
            var back = _root.gameObject.AddComponent<Image>();
            back.color = new Color(0.03f, 0.03f, 0.03f, 0.85f);

            // 열 머리
            var header = UiKit.NewRect("Header", _root);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.offsetMin = new Vector2(0f, -HeaderHeight);
            header.offsetMax = Vector2.zero;
            header.gameObject.AddComponent<Image>().color = UiKit.ButtonBack;
            foreach (var (key, label) in Columns)
            {
                var k = key;
                var cell = UiKit.NewButton("Col " + key, header, _font, 13f, () => _header?.OnSortClicked(k), out var text);
                ((Image)cell.targetGraphic).color = new Color(0, 0, 0, 0);
                OfferRowView.PlaceColumn((RectTransform)cell.transform, key);
                _columnLabels.Add((key, label, text));
            }

            HeaderSkin?.Invoke(header.gameObject);

            // 스크롤 영역
            _viewport = UiKit.NewRect("Viewport", _root);
            UiKit.Stretch(_viewport);
            _viewport.offsetMax = new Vector2(0f, -HeaderHeight);
            _viewport.gameObject.AddComponent<RectMask2D>();
            _viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f); // 휠 입력을 받는다.

            _content = UiKit.NewRect("Content", _viewport);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.offsetMin = Vector2.zero;
            _content.offsetMax = Vector2.zero;

            _scroll = _root.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = _viewport;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = RowHeight;
            _scroll.onValueChanged.AddListener(_ => Rebind());

            _empty = UiKit.NewLabel(_viewport, _font, 16f, TextAlignmentOptions.Center);
            _empty.text = "물건이 없습니다";
            _empty.gameObject.SetActive(false);

            _root.gameObject.SetActive(false);
        }

        private void RefreshColumnLabels()
        {
            var current = Plugin.CurrentSort;
            foreach (var (key, label, text) in _columnLabels)
            {
                bool active = current.Key == key;
                string arrow = active && key != SortKey.Default ? (current.Descending ? " ▼" : " ▲") : string.Empty;
                UiKit.SetText(text, label + arrow);
                text.color = active ? UiKit.TextActive : UiKit.Text;
            }

            _header?.RefreshSortLabels();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_visible)
            {
                Rebind();
            }
        }

        // ---------- 한 줄 ----------

        internal sealed class OfferRowView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
        {
            private OfferListView _owner;
            private RectTransform _rt;
            private Image _back, _icon, _selected;
            private TextMeshProUGUI _name, _price, _stock, _loyalty;
            private TooltipZone _priceZone;
            private Action _unsubscribeIcon;

            public Item Item { get; private set; }

            public static OfferRowView Create(OfferListView owner, RectTransform content, TMP_FontAsset font, int poolIndex)
            {
                var rt = UiKit.NewRect("Row " + poolIndex, content);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RowHeight);
                var view = rt.gameObject.AddComponent<OfferRowView>();
                view._owner = owner;
                view._rt = rt;
                view._back = rt.gameObject.AddComponent<Image>();

                var sel = UiKit.NewRect("Selected", rt);
                UiKit.Stretch(sel);
                view._selected = sel.gameObject.AddComponent<Image>();
                view._selected.color = new Color(1f, 0.85f, 0.45f, 0.18f);
                view._selected.raycastTarget = false;

                var icon = UiKit.NewRect("Icon", rt);
                PlaceColumn(icon, SortKey.Default);
                icon.offsetMin += new Vector2(4f, 4f);
                icon.offsetMax -= new Vector2(4f, 4f);
                view._icon = icon.gameObject.AddComponent<Image>();
                view._icon.preserveAspect = true;
                view._icon.raycastTarget = false;

                view._name = Cell(rt, font, SortKey.Name, TextAlignmentOptions.MidlineLeft);
                view._price = Cell(rt, font, SortKey.Price, TextAlignmentOptions.MidlineRight);
                view._stock = Cell(rt, font, SortKey.Stock, TextAlignmentOptions.MidlineRight);
                view._loyalty = Cell(rt, font, SortKey.Loyalty, TextAlignmentOptions.Center);

                var priceZone = UiKit.NewRect("PriceZone", rt);
                PlaceColumn(priceZone, SortKey.Price);
                priceZone.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                view._priceZone = priceZone.gameObject.AddComponent<TooltipZone>();
                view._priceZone.Owner = owner;
                return view;
            }

            private static TextMeshProUGUI Cell(RectTransform row, TMP_FontAsset font, SortKey column, TextAlignmentOptions align)
            {
                var rt = UiKit.NewRect(column.ToString(), row);
                PlaceColumn(rt, column);
                rt.offsetMin += new Vector2(6f, 0f);
                rt.offsetMax -= new Vector2(6f, 0f);
                var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
                if (font != null)
                {
                    label.font = font;
                }

                label.fontSize = 15f;
                label.alignment = align;
                label.color = UiKit.Text;
                label.raycastTarget = false;
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Ellipsis;
                return label;
            }

            /// <summary>열 자리: 아이콘(=기본) | 이름(늘어남) | 가격 | 재고 | 등급.</summary>
            internal static void PlaceColumn(RectTransform rt, SortKey column)
            {
                float right = LoyaltyWidth + StockWidth + PriceWidth;
                switch (column)
                {
                    case SortKey.Default:
                        Span(rt, 0f, IconWidth, false);
                        break;
                    case SortKey.Name:
                        rt.anchorMin = new Vector2(0f, 0f);
                        rt.anchorMax = new Vector2(1f, 1f);
                        rt.offsetMin = new Vector2(IconWidth, 0f);
                        rt.offsetMax = new Vector2(-right, 0f);
                        break;
                    case SortKey.Price:
                        Span(rt, right, PriceWidth, true);
                        break;
                    case SortKey.Stock:
                        Span(rt, LoyaltyWidth + StockWidth, StockWidth, true);
                        break;
                    case SortKey.Loyalty:
                        Span(rt, LoyaltyWidth, LoyaltyWidth, true);
                        break;
                }
            }

            /// <summary>왼쪽(또는 오른쪽 끝)에서 offset 만큼 떨어진 width 폭의 세로 띠.</summary>
            private static void Span(RectTransform rt, float offset, float width, bool fromRight)
            {
                rt.anchorMin = new Vector2(fromRight ? 1f : 0f, 0f);
                rt.anchorMax = new Vector2(fromRight ? 1f : 0f, 1f);
                rt.offsetMin = new Vector2(fromRight ? -offset : offset, 0f);
                rt.offsetMax = new Vector2(fromRight ? -offset + width : offset + width, 0f);
            }

            public void Bind(OfferRow row, int index)
            {
                gameObject.SetActive(true);
                _rt.anchoredPosition = new Vector2(0f, -index * RowHeight);
                var item = row.Tag as Item;
                bool itemChanged = item != Item;
                Item = item;
                bool alternate = index % 2 == 1;
                _back.color = alternate ? new Color(1f, 1f, 1f, 0.04f) : new Color(0f, 0f, 0f, 0f);
                RowSkin?.Invoke(gameObject, alternate);

                UiKit.SetText(_name, row.Name);
                UiKit.SetText(_price, Item != null ? _owner.PriceText(Item) : "-");
                int? max = Item != null && Item.BuyRestrictionMax > 0 ? Item.BuyRestrictionMax : (int?)null;
                int? current = max.HasValue ? Item.BuyRestrictionCurrent : (int?)null;
                UiKit.SetText(_stock, TraderListView.PriceText.Stock(Item?.StackObjectsCount ?? row.Stock, max, current));
                UiKit.SetText(_loyalty, TraderListView.PriceText.Loyalty(row.LoyaltyLevel, row.Locked));
                _name.color = row.Locked ? new Color(0.6f, 0.45f, 0.45f, 1f) : UiKit.Text;
                _priceZone.Text = Item != null ? _owner.PriceTooltip(Item) : null;
                if (itemChanged)
                {
                    BindIcon();
                }
            }

            /// <summary>
            /// 게임의 캐시된 아이콘(<c>ItemViewFactory.LoadItemIcon</c>)을 쓴다 — 격자 칸과 같은 길.
            /// 아직 만들어지는 중이면 <c>Changed</c> 를 받아 갈아 끼운다. 물건이 바뀔 때만 부른다.
            /// </summary>
            private void BindIcon()
            {
                UnbindIcon();
                if (Item == null)
                {
                    _icon.enabled = false;
                    return;
                }

                try
                {
                    var icon = ItemViewFactory.LoadItemIcon(Item);
                    void Apply()
                    {
                        if (this == null)
                        {
                            return;
                        }

                        _icon.sprite = icon.Sprite;
                        _icon.enabled = icon.Sprite != null;
                    }

                    Apply();
                    _unsubscribeIcon = icon.Changed.Subscribe(Apply);
                }
                catch (Exception)
                {
                    _icon.enabled = false; // 아이콘을 못 얻어도 줄은 그대로 쓴다.
                }
            }

            private void UnbindIcon()
            {
                _unsubscribeIcon?.Invoke();
                _unsubscribeIcon = null;
            }

            public void Hide()
            {
                Item = null;
                UnbindIcon();
                gameObject.SetActive(false);
            }

            private void OnDestroy() => UnbindIcon();

            public void SetSelected(bool on) => _selected.enabled = on;

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    _owner.OnRowClicked(this);
                }
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (Item != null)
                {
                    _owner.ShowItemTooltip(Item);
                }
            }

            public void OnPointerExit(PointerEventData eventData) => _owner.HideAllTooltips();
        }

        /// <summary>물물교환 가격 칸 위에서 재료 목록을 띄운다.</summary>
        internal sealed class TooltipZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
        {
            public OfferListView Owner;
            public string Text;

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    Owner.ShowTooltip(Text);
                }
            }

            public void OnPointerExit(PointerEventData eventData) => Owner.HideAllTooltips();

            // 가격 칸을 눌러도 줄을 누른 것과 같게.
            public void OnPointerClick(PointerEventData eventData)
            {
                var row = GetComponentInParent<OfferRowView>();
                if (row != null)
                {
                    row.OnPointerClick(eventData);
                }
            }
        }
    }
}
