using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TraderListView.Ui
{
    /// <summary>헤더·목록이 같이 쓰는 작은 uGUI 부품 만들기. 글꼴은 화면에 이미 있는 글자에서 빌린다.</summary>
    internal static class UiKit
    {
        public static readonly Color Text = new Color(0.78f, 0.78f, 0.74f, 1f);
        public static readonly Color TextActive = new Color(1f, 0.85f, 0.45f, 1f);
        public static readonly Color ButtonBack = new Color(0.10f, 0.10f, 0.10f, 0.92f);
        public static readonly Color ButtonBackActive = new Color(0.22f, 0.20f, 0.14f, 0.95f);

        public static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent != null ? parent.gameObject.layer : 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static TextMeshProUGUI NewLabel(Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions align)
        {
            var rt = NewRect("Label", parent);
            Stretch(rt);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null)
            {
                label.font = font;
            }

            label.fontSize = size;
            label.alignment = align;
            label.color = Text;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        public static Button NewButton(string name, Transform parent, TMP_FontAsset font, float fontSize, Action onClick, out TextMeshProUGUI label)
        {
            var rt = NewRect(name, parent);
            var back = rt.gameObject.AddComponent<Image>();
            back.color = ButtonBack;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = back;
            button.onClick.AddListener(() => onClick?.Invoke());
            label = NewLabel(rt, font, fontSize, TextAlignmentOptions.Center);
            return button;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>글꼴(대체 글꼴 포함)에 없는 기호를 글자로 바꿔 넣는다.</summary>
        public static void SetText(TMP_Text label, string text)
        {
            if (label == null)
            {
                return;
            }

            var font = label.font;
            label.text = GlyphFallback.Replace(text, cp => font == null || HasGlyph(font, cp));
        }

        private static bool HasGlyph(TMP_FontAsset font, int codepoint)
        {
            // 이 TMP 판은 char 만 받는다 — BMP 밖(🔒 같은 그림 글자)은 없는 것으로 친다.
            if (codepoint > 0xFFFF)
            {
                return false;
            }

            try
            {
                return font.HasCharacter((char)codepoint, searchFallbacks: true, tryAddCharacter: true);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>화면 안의 아무 TMP 글자에서 글꼴을 빌린다.</summary>
        public static TMP_FontAsset BorrowFont(Component root)
        {
            var text = root != null ? root.GetComponentInChildren<TMP_Text>(true) : null;
            return text != null ? text.font : null;
        }

        /// <summary>대상의 네 모서리를 기준 부모 좌표로.</summary>
        public static Rect BoundsIn(RectTransform target, RectTransform space)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
            foreach (var c in corners)
            {
                var p = space.InverseTransformPoint(c);
                xMin = Mathf.Min(xMin, p.x);
                yMin = Mathf.Min(yMin, p.y);
                xMax = Mathf.Max(xMax, p.x);
                yMax = Mathf.Max(yMax, p.y);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        /// <summary>
        /// 대상(부모가 달라도)을 기준 부모 좌표의 사각형에 놓는다. 레이아웃 그룹이 있으면 그 계산에서 뺀다.
        /// </summary>
        public static void PlaceIn(RectTransform target, RectTransform space, Rect rect)
        {
            var layout = target.GetComponent<LayoutElement>() ?? target.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            target.anchorMin = new Vector2(0.5f, 0.5f);
            target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
            var world = space.TransformPoint(new Vector3(rect.center.x, rect.center.y, 0f));
            target.position = world;
        }
    }
}
