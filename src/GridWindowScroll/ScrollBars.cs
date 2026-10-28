using UnityEngine;
using UnityEngine.UI;

namespace GridWindowScroll
{
    /// <summary>
    /// 뷰포트 오른쪽·아래 끝에 붙는 스크롤바. 게임 프리팹을 가져오지 않고 직접 만든다 —
    /// 창마다 가져올 만한 바를 찾아 복제하는 쪽이 더 잘 깨진다.
    /// </summary>
    /// <remarks>
    /// 뷰포트의 자식으로 둔다. 내용(격자)과 형제라 스크롤해도 따라 움직이지 않고,
    /// 마스크 안이라 창 밖으로 비어져 나가지도 않는다. 끝 칸 위로 굵기만큼 겹친다.
    /// </remarks>
    internal static class ScrollBars
    {
        private const string VerticalName = "GridScrollBarV";
        private const string HorizontalName = "GridScrollBarH";

        private static readonly Color TrackColor = new Color(0f, 0f, 0f, 0.35f);
        private static readonly Color HandleColor = new Color(0.78f, 0.78f, 0.74f, 0.45f);

        internal static void Attach(RectTransform viewport, ScrollRect scroll)
        {
            scroll.verticalScrollbar = Build(viewport, Scrollbar.Direction.BottomToTop);
            scroll.horizontalScrollbar = Build(viewport, Scrollbar.Direction.LeftToRight);

            // 자동 숨김은 뷰포트를 자기 멋대로 늘이므로 쓰지 않는다. 보일지 말지는 직접 정한다.
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        /// <summary>스크롤이 필요한 쪽만 보이게 한다.</summary>
        internal static void UpdateVisibility(ScrollRect scroll)
        {
            var show = GridWindowScrollPlugin.ShowScrollBars.Value;
            Show(scroll.verticalScrollbar, show && scroll.vertical);
            Show(scroll.horizontalScrollbar, show && scroll.horizontal);

            Thicken(scroll.verticalScrollbar, true);
            Thicken(scroll.horizontalScrollbar, false);
        }

        private static void Show(Scrollbar bar, bool on)
        {
            if (bar != null && bar.gameObject.activeSelf != on) bar.gameObject.SetActive(on);
        }

        private static void Thicken(Scrollbar bar, bool vertical)
        {
            if (bar == null) return;
            var rt = (RectTransform)bar.transform;
            var thickness = GridWindowScrollPlugin.ScrollBarThickness.Value;
            rt.sizeDelta = vertical ? new Vector2(thickness, 0f) : new Vector2(0f, thickness);
        }

        private static Scrollbar Build(RectTransform viewport, Scrollbar.Direction direction)
        {
            var vertical = direction == Scrollbar.Direction.BottomToTop;
            var thickness = GridWindowScrollPlugin.ScrollBarThickness.Value;

            var go = new GameObject(vertical ? VerticalName : HorizontalName, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(viewport, false);

            if (vertical)
            {
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.sizeDelta = new Vector2(thickness, 0f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(0f, thickness);
            }
            rt.anchoredPosition = Vector2.zero;

            var track = go.AddComponent<Image>();
            track.color = TrackColor;

            // 손잡이는 Scrollbar 가 앵커를 직접 움직이므로 전용 부모(Sliding Area)가 필요하다.
            var area = new GameObject("Sliding Area", typeof(RectTransform));
            var areaRt = (RectTransform)area.transform;
            areaRt.SetParent(rt, false);
            Stretch(areaRt);

            var handle = new GameObject("Handle", typeof(RectTransform));
            var handleRt = (RectTransform)handle.transform;
            handleRt.SetParent(areaRt, false);
            Stretch(handleRt);
            // 색은 상태색이 곱해지므로 그림 자체는 흰색으로 두고 상태색에서 밝기·투명도를 정한다.
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            var bar = go.AddComponent<Scrollbar>();
            bar.direction = direction;
            bar.handleRect = handleRt;
            bar.targetGraphic = handleImage;
            bar.transition = Selectable.Transition.ColorTint;

            var colors = bar.colors;
            colors.normalColor = HandleColor;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.86f, 0.7f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.selectedColor = HandleColor;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.05f;
            bar.colors = colors;

            return bar;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
