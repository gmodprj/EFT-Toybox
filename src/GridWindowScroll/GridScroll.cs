using System;
using System.Collections;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GridWindowScroll
{
    /// <summary>
    /// 창이 내용을 채운 직후를 잡는다. 격자 GameObject 는 그 프레임에 막 만들어지고
    /// 크기는 레이아웃이 한 번 돌아야 정해지므로, 한 프레임 뒤에 손을 댄다.
    /// </summary>
    [HarmonyPatch]
    internal static class GridWindowPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GridWindow), nameof(GridWindow.ShowContents))]
        private static void AfterShowContents(GridWindow __instance) => Schedule(__instance);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GridWindow), nameof(GridWindow.ShowGrids))]
        private static void AfterShowGrids(GridWindow __instance) => Schedule(__instance);

        private static void Schedule(GridWindow window)
        {
            if (window == null) return;
            window.StartCoroutine(ApplyNextFrame(window));
        }

        private static IEnumerator ApplyNextFrame(GridWindow window)
        {
            yield return null;
            if (window == null) yield break;

            try
            {
                GridScroll.Apply(window);
            }
            catch (Exception e)
            {
                GridWindowScrollPlugin.Log.LogError("격자 스크롤 적용 실패 — 창은 그대로 둔다: " + e);
            }
        }
    }

    /// <summary>
    /// 격자 묶음(<see cref="ContainedGridsView"/>)을 잘라 내는 뷰포트로 감싼다.
    /// 격자 자체는 건드리지 않는다 — 칸 좌표와 드래그 판정이 격자의 지역 좌표로 계산되기 때문이다.
    /// 뷰포트는 한 번만 만들고, 창을 다시 열 때마다 크기만 다시 잡는다.
    /// </summary>
    internal static class GridScroll
    {
        private const string ViewportName = "GridScrollViewport";

        /// <summary>이보다 줄이면 아무것도 보이지 않는다. F12 설정 범위의 아래끝과 같다.</summary>
        internal const int MinCells = 3;

        internal static void Apply(GridWindow window)
        {
            foreach (var contained in window.GetComponentsInChildren<ContainedGridsView>(true))
            {
                if (!contained.gameObject.activeInHierarchy) continue;
                Handle(contained);
            }

            DropEmptyViewports(window);
        }

        /// <summary>
        /// 창이 격자 묶음을 새로 만들어 붙이면 먼저 쓰던 뷰포트가 빈 채로 남는다.
        /// 남아 있으면 그 크기만큼 창이 커지므로 치운다.
        /// </summary>
        private static void DropEmptyViewports(GridWindow window)
        {
            foreach (var rt in window.GetComponentsInChildren<RectTransform>(true))
            {
                // 스크롤바도 뷰포트의 자식이라 자식 수로는 못 센다. 격자 묶음이 있는지로 본다.
                if (rt.name != ViewportName || rt.GetComponentInChildren<ContainedGridsView>(true) != null) continue;

                var grip = FindGrip(rt);
                if (grip != null) UnityEngine.Object.Destroy(grip.gameObject);

                GridWindowScrollPlugin.Log.LogInfo("빈 격자 뷰포트를 치웠다");
                UnityEngine.Object.Destroy(rt.gameObject);
            }
        }

        private static void Handle(ContainedGridsView contained)
        {
            if (!(contained.transform is RectTransform content)) return;
            if (!(content.parent is RectTransform parent)) return;

            var viewport = parent.name == ViewportName ? parent : null;

            // 격자들이 실제로 차지하는 크기. 창이 내용을 다시 채웠을 수 있으므로 매번 다시 잰다.
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            var natural = MeasureContent(content);
            if (natural.x <= 1f || natural.y <= 1f) return;

            var cell = CellPixels();
            var naturalCells = new Vector2((natural.x - 1f) / cell, (natural.y - 1f) / cell);

            var templateId = TemplateIdOf(contained);
            ResolveCells(templateId, out var columns, out var rows);

            var target = TargetSize(natural, columns, rows);
            var needsScroll = natural.x > target.x + 0.5f || natural.y > target.y + 0.5f;

            if (viewport == null)
            {
                if (!needsScroll)
                {
                    Trace(contained, natural, target, "격자가 한 화면에 들어가므로 그대로 둔다");
                    return;
                }
                viewport = CreateViewport(content, parent, target);
            }

            Fit(viewport, content, natural, columns, rows, true);

            var grip = FindGrip(viewport) ?? ResizeGrip.Attach(viewport);
            grip.Refresh(content, templateId, naturalCells, columns, rows);

            Trace(contained, natural, target, needsScroll ? "스크롤" : "자르지 않음");
        }

        /// <summary>손잡이를 끌 때 그 자리에서 다시 맞춘다. 스크롤 위치는 건드리지 않는다.</summary>
        /// <remarks>
        /// 창은 가운데를 기준으로 커지고 작아진다. 그대로 두면 한 칸 바뀔 때마다 왼위 모서리가
        /// 반 칸씩 밀려, 끄는 오른아래 모서리는 커서의 절반만 움직인다(실측 — 한 칸 63 에
        /// 왼위가 32 이동). 그래서 크기를 바꾼 뒤 창을 그만큼 되밀어 **왼위를 붙잡아 둔다.**
        /// </remarks>
        internal static void ApplyCells(RectTransform viewport, RectTransform content, int columns, int rows)
        {
            if (viewport == null || content == null) return;

            var windowTransform = WindowTransformOf(viewport);
            var before = TopLeftWorld(viewport);

            Fit(viewport, content, MeasureContent(content), columns, rows, false);

            if (windowTransform == null) return;

            // 되밀 양을 재려면 레이아웃이 지금 바로 돌아야 한다.
            LayoutRebuilder.ForceRebuildLayoutImmediate(windowTransform);
            windowTransform.position += before - TopLeftWorld(viewport);
        }

        /// <summary>창의 뿌리. 뷰포트의 조상이 아니면 되밀어도 소용이 없으니 그때는 포기한다.</summary>
        private static RectTransform WindowTransformOf(RectTransform viewport)
        {
            var window = viewport.GetComponentInParent<GridWindow>();
            var windowTransform = window != null ? window.WindowTransform : null;
            return windowTransform != null && viewport.IsChildOf(windowTransform) ? windowTransform : null;
        }

        private static Vector3 TopLeftWorld(RectTransform rt)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            return corners[1];   // 0=왼아래 1=왼위 2=오른위 3=오른아래
        }

        /// <summary>상자별로 기억한 값이 있으면 그것, 없으면 전체 기본값.</summary>
        private static void ResolveCells(string templateId, out int columns, out int rows)
        {
            if (GridWindowScrollPlugin.RememberPerContainer.Value
                && GridWindowScrollPlugin.Sizes.TryGet(templateId, out columns, out rows))
                return;

            columns = GridWindowScrollPlugin.MaxColumns.Value;
            rows = GridWindowScrollPlugin.MaxRows.Value;
        }

        /// <summary>격자 묶음이 속한 아이템의 템플릿 id. 칸(슬롯)만 있는 묶음은 없을 수 있다.</summary>
        private static string TemplateIdOf(ContainedGridsView contained)
        {
            var views = contained.GridViews;
            if (views == null) return null;

            foreach (var view in views)
            {
                var parentItem = view?.Grid?.ParentItem;
                if (parentItem != null) return parentItem.StringTemplateId;
            }

            return null;
        }

        /// <summary>격자·칸들이 차지하는 범위. 뷰포트가 가둔 뒤에는 묶음 자신의 크기를 믿을 수 없어 자식으로 잰다.</summary>
        private static Vector2 MeasureContent(RectTransform content)
        {
            var any = false;
            var total = default(Bounds);

            foreach (Transform child in content)
            {
                if (!(child is RectTransform rt) || !child.gameObject.activeSelf) continue;
                var b = RectTransformUtility.CalculateRelativeRectTransformBounds(content, rt);
                if (!any) { total = b; any = true; }
                else total.Encapsulate(b);
            }

            if (!any) return content.rect.size;

            var size = new Vector2(total.size.x, total.size.y);

            // 레이아웃 그룹의 여백은 자식 범위에 들어가지 않으므로 더해 준다.
            var group = content.GetComponent<LayoutGroup>();
            if (group != null)
            {
                size.x += group.padding.left + group.padding.right;
                size.y += group.padding.top + group.padding.bottom;
            }

            return size;
        }

        private static Vector2 TargetSize(Vector2 natural, int columns, int rows)
        {
            if (!GridWindowScrollPlugin.Enabled.Value) return natural;

            var px = ItemViewFactory.GetCellPixelSize(new IntVec2(Math.Max(columns, MinCells), Math.Max(rows, MinCells)));
            return new Vector2(Mathf.Min(natural.x, px.X), Mathf.Min(natural.y, px.Y));
        }

        internal static float CellPixels()
        {
            // 공식에 상수항이 있어 두 크기의 차로 한 칸 크기를 구한다.
            return ItemViewFactory.GetCellPixelSize(new IntVec2(2, 1)).X
                 - ItemViewFactory.GetCellPixelSize(new IntVec2(1, 1)).X;
        }

        private static ResizeGrip FindGrip(RectTransform viewport)
        {
            if (!(viewport.parent is RectTransform parent)) return null;

            // 한 부모 아래에 뷰포트가 둘 이상일 수 있어 이름이 아니라 짝으로 찾는다.
            foreach (var grip in parent.GetComponentsInChildren<ResizeGrip>(true))
                if (grip.Viewport == viewport) return grip;

            return null;
        }

        private static RectTransform CreateViewport(RectTransform content, RectTransform parent, Vector2 target)
        {
            var index = content.GetSiblingIndex();

            var go = new GameObject(ViewportName, typeof(RectTransform));
            var viewport = (RectTransform)go.transform;
            viewport.SetParent(parent, false);
            viewport.SetSiblingIndex(index);
            viewport.anchorMin = viewport.anchorMax = viewport.pivot = new Vector2(0f, 1f);
            viewport.anchoredPosition = Vector2.zero;
            viewport.sizeDelta = target;

            if (GridWindowScrollPlugin.UseStencilMask.Value)
            {
                // 칸 아이콘이 RectMask2D 를 무시하고 비어져 나올 때 쓰는 길. 스텐실은 셰이더와 무관하게 자른다.
                var image = go.AddComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0f);
                go.AddComponent<Mask>().showMaskGraphic = false;
            }
            else
            {
                go.AddComponent<RectMask2D>();
            }

            content.SetParent(viewport, false);
            content.anchorMin = content.anchorMax = content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;

            var scroll = go.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            ScrollBars.Attach(viewport, scroll);

            GridWindowScrollPlugin.Log.LogInfo($"격자 뷰포트를 만들었다 — {parent.name}/{content.name}");
            return viewport;
        }

        /// <summary>뷰포트와 내용의 크기를 지금 격자와 칸 수에 맞춘다.</summary>
        private static void Fit(RectTransform viewport, RectTransform content, Vector2 natural,
                                int columns, int rows, bool resetScroll)
        {
            var target = TargetSize(natural, columns, rows);

            content.sizeDelta = natural;              // 앵커가 한 점이므로 sizeDelta 가 곧 실제 크기다
            viewport.sizeDelta = target;

            // 창 전체 크기는 부모 레이아웃이 정한다. 뷰포트가 자른 크기를 그대로 요구해야 창이 커지지 않는다.
            var element = viewport.GetComponent<LayoutElement>() ?? viewport.gameObject.AddComponent<LayoutElement>();
            element.minWidth = element.preferredWidth = target.x;
            element.minHeight = element.preferredHeight = target.y;
            element.flexibleWidth = element.flexibleHeight = 0f;

            var scroll = viewport.GetComponent<ScrollRect>();
            if (scroll != null)
            {
                scroll.horizontal = natural.x > target.x + 0.5f;
                scroll.vertical = natural.y > target.y + 0.5f;
                scroll.scrollSensitivity = CellPixels() * GridWindowScrollPlugin.WheelCells.Value;
                if (resetScroll && (scroll.horizontal || scroll.vertical))
                    scroll.normalizedPosition = new Vector2(0f, 1f);   // 열 때는 왼쪽 위에서 시작한다

                ScrollBars.UpdateVisibility(scroll);
            }

            LayoutRebuilder.MarkLayoutForRebuild(viewport);
        }

        private static void Trace(ContainedGridsView contained, Vector2 natural, Vector2 target, string what)
        {
            if (!GridWindowScrollPlugin.Verbose.Value) return;

            var cell = CellPixels();
            GridWindowScrollPlugin.Log.LogInfo(
                $"{contained.name}: 격자 {natural.x / cell:0.#}x{natural.y / cell:0.#}칸 ({natural.x:0}x{natural.y:0}px) "
              + $"→ 보임 {target.x / cell:0.#}x{target.y / cell:0.#}칸 ({target.x:0}x{target.y:0}px) — {what}");
        }
    }
}
