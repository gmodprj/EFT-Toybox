using System;
using UnityEngine;
using UnityEngine.UI;

namespace GogglesWheel
{
    /// <summary>
    /// 휠을 한 장의 그물로 직접 그린다 — 고리, 조각, 조각 사이 선, 데드존 원, 그리고 가리키는 선.
    ///
    /// 왜 스프라이트를 안 쓰나 — 모드에 그림 파일을 들고 다니면 해상도마다 흐려지고,
    /// 게임이 쓰는 색과 어긋난다. 설치된 `Tylevo.FieldAttachments` 도 같은 방식으로
    /// <c>MaskableGraphic.OnPopulateMesh</c> 에서 꼭짓점을 직접 쌓는다(실측).
    /// 색은 <see cref="WheelPalette"/> 가 게임에서 빌려 온다.
    /// </summary>
    public sealed class WheelGraphic : MaskableGraphic
    {
        /// <summary>조각 하나를 몇 조각으로 쪼개 그릴지. 테두리가 각지지 않을 만큼.</summary>
        private const int MinArcSteps = 8;
        private const int ArcStepsPerCircle = 72;

        private const float EdgeWidth = 1.6f;
        private const float SelectedEdgeWidth = 2.4f;
        private const float PointerWidth = 2f;
        private const float PointerTip = 5f;

        private int _count;
        private int _selected = -1;
        private float _innerRadius = 96f;
        private float _outerRadius = 210f;
        private float _deadZoneRadius = 42f;
        private Vector2 _pointer;
        private WheelPalette _palette = new WheelPalette();

        /// <summary>그릴 것이 바뀔 때마다 부른다. 값이 같으면 다시 그리지 않는다.</summary>
        public void Style(int count, int selected, Vector2 pointer, float innerRadius, float outerRadius, float deadZoneRadius, WheelPalette palette)
        {
            var same = _count == count
                && _selected == selected
                && _pointer == pointer
                && Mathf.Approximately(_innerRadius, innerRadius)
                && Mathf.Approximately(_outerRadius, outerRadius)
                && Mathf.Approximately(_deadZoneRadius, deadZoneRadius)
                && ReferenceEquals(_palette, palette);

            if (same)
            {
                return;
            }

            _count = count;
            _selected = selected;
            _pointer = pointer;
            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
            _deadZoneRadius = deadZoneRadius;
            _palette = palette ?? new WheelPalette();

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();

            if (_count <= 0)
            {
                return;
            }

            var half = TargetPicker.SectorHalfWidth(_count);
            var steps = Math.Max(MinArcSteps, ArcStepsPerCircle / _count);

            var normalFill = WithAlpha(_palette.Normal, 0.80f);
            var selectedFill = WithAlpha(_palette.Selected, 0.85f);
            var edge = WithAlpha(_palette.Disabled, 0.90f);
            var selectedEdge = _palette.Text;

            for (var i = 0; i < _count; i++)
            {
                var center = TargetPicker.AngleForIndex(i, _count);
                var from = (center - half) * Mathf.Deg2Rad;
                var to = (center + half) * Mathf.Deg2Rad;
                var picked = i == _selected;

                Wedge(mesh, from, to, steps, picked ? selectedFill : normalFill);

                // 테두리는 안·밖 호와 조각 사이 칸막이. 고른 조각만 밝게 두른다.
                var width = picked ? SelectedEdgeWidth : EdgeWidth;
                var tint = picked ? selectedEdge : edge;

                Arc(mesh, from, to, steps, _outerRadius, width, tint);
                Arc(mesh, from, to, steps, _innerRadius, width, tint);

                if (_count > 1)
                {
                    Spoke(mesh, from, width, tint);
                }
            }

            // 데드존 — 여기에 두고 떼면 취소다. 커서가 안에 있으면 채워 보여 준다.
            var inside = _selected < 0;
            Disc(mesh, _deadZoneRadius, steps * 2, WithAlpha(inside ? _palette.Selected : _palette.Normal, inside ? 0.55f : 0.70f));
            Circle(mesh, _deadZoneRadius, steps * 2, inside ? selectedEdge : edge, inside ? SelectedEdgeWidth : EdgeWidth);

            Pointer(mesh);
        }

        /// <summary>가운데에서 커서까지 가는 선과 그 끝의 점. 실제 마우스 포인터는 레이드 중 잠겨 있다.</summary>
        private void Pointer(VertexHelper mesh)
        {
            var tip = _pointer * _outerRadius;
            if (tip.sqrMagnitude <= 1f)
            {
                return;
            }

            var tint = _palette.Text;
            Stroke(mesh, Vector2.zero, tip, PointerWidth, tint);
            Disc(mesh, PointerTip, 12, tint, tip);
        }

        private void Wedge(VertexHelper mesh, float from, float to, int steps, Color fill)
        {
            for (var s = 0; s < steps; s++)
            {
                var a = Mathf.Lerp(from, to, (float)s / steps);
                var b = Mathf.Lerp(from, to, (float)(s + 1) / steps);

                var innerA = Point(a, _innerRadius);
                var innerB = Point(b, _innerRadius);
                var outerA = Point(a, _outerRadius);
                var outerB = Point(b, _outerRadius);

                Quad(mesh, innerA, outerA, outerB, innerB, fill);
            }
        }

        private void Arc(VertexHelper mesh, float from, float to, int steps, float radius, float width, Color tint)
        {
            for (var s = 0; s < steps; s++)
            {
                var a = Mathf.Lerp(from, to, (float)s / steps);
                var b = Mathf.Lerp(from, to, (float)(s + 1) / steps);
                Stroke(mesh, Point(a, radius), Point(b, radius), width, tint);
            }
        }

        private void Spoke(VertexHelper mesh, float angle, float width, Color tint)
        {
            Stroke(mesh, Point(angle, _innerRadius), Point(angle, _outerRadius), width, tint);
        }

        private void Circle(VertexHelper mesh, float radius, int steps, Color tint, float width)
        {
            Arc(mesh, 0f, Mathf.PI * 2f, steps, radius, width, tint);
        }

        private static void Disc(VertexHelper mesh, float radius, int steps, Color fill)
        {
            Disc(mesh, radius, steps, fill, Vector2.zero);
        }

        private static void Disc(VertexHelper mesh, float radius, int steps, Color fill, Vector2 center)
        {
            for (var s = 0; s < steps; s++)
            {
                var a = (float)s / steps * Mathf.PI * 2f;
                var b = (float)(s + 1) / steps * Mathf.PI * 2f;

                var start = mesh.currentVertCount;
                mesh.AddVert(center, fill, Vector2.zero);
                mesh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, fill, Vector2.zero);
                mesh.AddVert(center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, fill, Vector2.zero);
                mesh.AddTriangle(start, start + 1, start + 2);
            }
        }

        private static Vector2 Point(float angle, float radius)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color fill)
        {
            var start = mesh.currentVertCount;
            mesh.AddVert(a, fill, Vector2.zero);
            mesh.AddVert(b, fill, Vector2.zero);
            mesh.AddVert(c, fill, Vector2.zero);
            mesh.AddVert(d, fill, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }

        /// <summary>두 점을 잇는 두께 있는 선. 선을 네모로 펴서 그린다.</summary>
        private static void Stroke(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
        {
            var along = b - a;
            if (along.sqrMagnitude <= 0f)
            {
                return;
            }

            var across = new Vector2(-along.y, along.x).normalized * width / 2f;
            Quad(mesh, a - across, a + across, b + across, b - across, tint);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
