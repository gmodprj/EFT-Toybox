using System;
using System.Collections;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TraderListView.Ui
{
    /// <summary>
    /// 헤더 아이콘 버튼의 공통 겉모습(사용자 제안서 2026-11-18 §4.1·§5·§6.1):
    /// 얇은 테두리 + 바탕, 상태(기본·마우스 오버·눌림·활성·비활성)에 따라 바탕 색이 바뀐다.
    /// 게임·다른 모드의 버튼(새로고침·연필)에도 같은 틀을 씌운다 — 그쪽의 바탕 층은 끄고 아이콘만 남긴다.
    /// </summary>
    public sealed class StyledButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private Image _fill;
        private Image _border;
        private bool _hover, _pressed, _lastActive, _lastDisabled;

        /// <summary>활성(지금 상태) 여부. 예: 편집 중, 지금 보기 방식.</summary>
        public Func<bool> IsActive;

        public Func<bool> IsDisabled;

        /// <summary>우리 버튼일 때만 쓴다. 게임·다른 모드 버튼은 자기 클릭 처리기가 따로 있다.</summary>
        public Action OnClick;

        /// <summary>대상에 테두리·바탕 층을 맨 아래에 깔고, 대상의 원래 바탕 층은 끈다.</summary>
        public static StyledButton Apply(RectTransform target, Func<bool> isActive, bool hideOwnBackground)
        {
            var styled = target.GetComponent<StyledButton>() ?? target.gameObject.AddComponent<StyledButton>();
            styled.IsActive = isActive;
            if (styled._fill == null)
            {
                if (hideOwnBackground)
                {
                    HideBackgroundLayers(target);
                }

                styled._border = Layer(target, "TLV Border", 0f, ParseHex(ButtonStyle.Border));
                styled._fill = Layer(target, "TLV Fill", 1f, ParseHex(ButtonStyle.Default));
                styled._border.transform.SetSiblingIndex(0);
                styled._fill.transform.SetSiblingIndex(1);
                styled._fill.raycastTarget = true; // 바탕을 껐으니 클릭은 이 층이 받는다(부모 처리기로 올라간다).
            }

            styled.Refresh(force: true);
            return styled;
        }

        /// <summary>대상 자신과 늘어나는(바탕) 자식 Image 를 끈다. 작은 아이콘 층은 남긴다.</summary>
        private static void HideBackgroundLayers(RectTransform target)
        {
            foreach (var image in target.GetComponentsInChildren<Image>(true))
            {
                var rt = image.rectTransform;
                bool stretched = rt.anchorMin != rt.anchorMax;
                if (rt == target || stretched)
                {
                    image.enabled = false;
                }
                else
                {
                    image.color = ParseHex(ButtonStyle.Icon);
                }
            }
        }

        private static Image Layer(RectTransform parent, string name, float inset, Color color)
        {
            var rt = UiKit.NewRect(name, parent);
            UiKit.Stretch(rt);
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void Update()
        {
            Refresh(force: false);
        }

        private void Refresh(bool force)
        {
            bool active = IsActive != null && IsActive();
            bool disabled = IsDisabled != null && IsDisabled();
            if (!force && active == _lastActive && disabled == _lastDisabled)
            {
                return;
            }

            _lastActive = active;
            _lastDisabled = disabled;
            Paint();
        }

        private void Paint()
        {
            if (_fill == null)
            {
                return;
            }

            _fill.color = ParseHex(ButtonStyle.Background(_lastActive, _hover, _pressed, _lastDisabled));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            Paint();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            _pressed = false;
            Paint();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            Paint();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            Paint();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && !(IsDisabled != null && IsDisabled()))
            {
                OnClick?.Invoke();
            }
        }

        public static Color ParseHex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
        }
    }

    /// <summary>마우스를 올리고 0.5초 뒤 게임의 간단 툴팁으로 글자를 보인다(§6.2). 아이콘 버튼용.</summary>
    public sealed class HoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private const float Delay = 0.5f;
        private Coroutine _pending;
        private bool _shown;

        public Func<string> Text;

        public void OnPointerEnter(PointerEventData eventData)
        {
            Cancel();
            _pending = StartCoroutine(ShowLater());
        }

        public void OnPointerExit(PointerEventData eventData) => Hide();

        public void OnPointerDown(PointerEventData eventData) => Hide();

        private void OnDisable() => Hide();

        private IEnumerator ShowLater()
        {
            yield return new WaitForSecondsRealtime(Delay);
            _pending = null;
            var context = ItemUiContext.Instance;
            var text = Text?.Invoke();
            if (context != null && !string.IsNullOrEmpty(text))
            {
                context.Tooltip.Show(text);
                _shown = true;
            }
        }

        private void Cancel()
        {
            if (_pending != null)
            {
                StopCoroutine(_pending);
                _pending = null;
            }
        }

        private void Hide()
        {
            Cancel();
            if (_shown)
            {
                _shown = false;
                var context = ItemUiContext.Instance;
                if (context != null)
                {
                    context.Tooltip.Close();
                }
            }
        }
    }

    /// <summary>
    /// 검색창 꾸밈(§4.5): 포커스가 있으면 테두리를 활성 색으로, 검색어가 있을 때만 지우기 버튼을 보인다.
    /// 검색창 자체(Trader Search)는 건드리지 않고 그 위·옆에 층을 더한다.
    /// </summary>
    public sealed class SearchDecor : MonoBehaviour
    {
        private TMP_InputField _input;
        private Image[] _border;
        private GameObject _clear;
        private bool _lastFocused, _lastHasText;

        public static SearchDecor Attach(RectTransform search, RectTransform clearButton)
        {
            var decor = search.GetComponent<SearchDecor>() ?? search.gameObject.AddComponent<SearchDecor>();
            decor._input = search.GetComponentInChildren<TMP_InputField>(true);
            decor._clear = clearButton != null ? clearButton.gameObject : null;
            if (decor._border == null)
            {
                // 테두리는 검색창 위에 얹는다(클릭은 통과). 안쪽은 비워 글자를 가리지 않는다.
                decor._border = Outline(search);
            }

            if (decor._input != null && decor._input.placeholder is TMP_Text placeholder)
            {
                placeholder.color = StyledButton.ParseHex(ButtonStyle.Placeholder);
            }

            decor.Refresh(force: true);
            return decor;
        }

        /// <summary>1px 선 넷(위·아래·왼쪽·오른쪽)으로 테두리를 그린다. 안쪽은 비워 글자를 가리지 않는다.</summary>
        private static Image[] Outline(RectTransform parent)
        {
            var frame = UiKit.NewRect("TLV Focus Border", parent);
            UiKit.Stretch(frame);
            frame.SetAsLastSibling();
            var edges = new (Vector2 min, Vector2 max, Vector2 size)[]
            {
                (new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f)),
                (new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f)),
                (new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f)),
                (new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0f)),
            };
            var images = new Image[edges.Length];
            for (int i = 0; i < edges.Length; i++)
            {
                var edge = UiKit.NewRect("Edge", frame);
                edge.anchorMin = edges[i].min;
                edge.anchorMax = edges[i].max;
                edge.pivot = new Vector2(edges[i].min.x, edges[i].min.y);
                edge.sizeDelta = edges[i].size;
                edge.anchoredPosition = Vector2.zero;
                images[i] = edge.gameObject.AddComponent<Image>();
                images[i].color = StyledButton.ParseHex(ButtonStyle.Border);
                images[i].raycastTarget = false;
            }

            return images;
        }

        /// <summary>검색어를 지운다 — Trader Search 가 값 변경을 받아 전체 목록으로 돌린다.</summary>
        public void Clear()
        {
            if (_input != null)
            {
                _input.text = string.Empty;
                _input.DeactivateInputField();
            }

            Refresh(force: true);
        }

        private void Update() => Refresh(force: false);

        private void Refresh(bool force)
        {
            bool focused = _input != null && _input.isFocused;
            bool hasText = _input != null && !string.IsNullOrEmpty(_input.text);
            if (!force && focused == _lastFocused && hasText == _lastHasText)
            {
                return;
            }

            _lastFocused = focused;
            _lastHasText = hasText;
            if (_border != null)
            {
                var color = StyledButton.ParseHex(focused ? ButtonStyle.BorderFocus : ButtonStyle.Border);
                foreach (var edge in _border)
                {
                    edge.color = color;
                }
            }

            if (_clear != null)
            {
                _clear.SetActive(hasText);
            }
        }
    }

    /// <summary>작은 Image 조각으로 그리는 아이콘(글꼴에 없는 기호 대신).</summary>
    internal static class IconArt
    {
        /// <summary>목록: 점 + 가로줄 세 줄.</summary>
        public static readonly Rect[] List =
        {
            new Rect(0.20f, 0.66f, 0.10f, 0.10f), new Rect(0.36f, 0.66f, 0.44f, 0.10f),
            new Rect(0.20f, 0.45f, 0.10f, 0.10f), new Rect(0.36f, 0.45f, 0.44f, 0.10f),
            new Rect(0.20f, 0.24f, 0.10f, 0.10f), new Rect(0.36f, 0.24f, 0.44f, 0.10f),
        };

        /// <summary>격자: 3×3 — 등급 탭의 2×2(▦)와 겹치지 않게(§4.4).</summary>
        public static readonly Rect[] Grid =
        {
            new Rect(0.20f, 0.62f, 0.16f, 0.16f), new Rect(0.42f, 0.62f, 0.16f, 0.16f), new Rect(0.64f, 0.62f, 0.16f, 0.16f),
            new Rect(0.20f, 0.42f, 0.16f, 0.16f), new Rect(0.42f, 0.42f, 0.16f, 0.16f), new Rect(0.64f, 0.42f, 0.16f, 0.16f),
            new Rect(0.20f, 0.22f, 0.16f, 0.16f), new Rect(0.42f, 0.22f, 0.16f, 0.16f), new Rect(0.64f, 0.22f, 0.16f, 0.16f),
        };

        /// <summary>지우기: ✕ — 두 막대를 45도로.</summary>
        public static RectTransform DrawClear(RectTransform parent)
        {
            var icon = UiKit.NewRect("ClearIcon", parent);
            UiKit.Stretch(icon);
            foreach (var angle in new[] { 45f, -45f })
            {
                var bar = UiKit.NewRect("Bar", icon);
                bar.anchorMin = new Vector2(0.5f, 0.5f);
                bar.anchorMax = new Vector2(0.5f, 0.5f);
                bar.sizeDelta = new Vector2(12f, 2f);
                bar.localRotation = Quaternion.Euler(0f, 0f, angle);
                var image = bar.gameObject.AddComponent<Image>();
                image.color = StyledButton.ParseHex(ButtonStyle.Icon);
                image.raycastTarget = false;
            }

            return icon;
        }

        public static RectTransform Draw(RectTransform parent, string name, Rect[] parts)
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
                image.color = StyledButton.ParseHex(ButtonStyle.Icon);
                image.raycastTarget = false;
            }

            return icon;
        }
    }
}
