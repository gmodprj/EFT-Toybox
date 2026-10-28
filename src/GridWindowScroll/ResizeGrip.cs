using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridWindowScroll
{
    /// <summary>
    /// 뷰포트 오른쪽 아래에 붙는 손잡이. 끌면 보이는 칸 수가 그 자리에서 바뀌고,
    /// 손을 떼면 상자별로 기억한다.
    /// </summary>
    /// <remarks>
    /// 뷰포트 바깥(부모의 자식)에 둔다. 뷰포트 안에 두면 마스크에 잘리거나 끝 칸을 가린다.
    /// 부모에 레이아웃 그룹이 있을 수 있으므로 자리는 레이아웃에서 빼고 직접 잡는다.
    /// </remarks>
    internal class ResizeGrip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        internal const string GripName = "GridScrollGrip";
        private const float Size = 14f;

        private RectTransform _self;
        private RectTransform _viewport;
        private Canvas _canvas;

        private RectTransform _content;
        private string _templateId;
        private Vector2 _naturalCells;
        private int _columns;
        private int _rows;

        /// <summary>손잡이를 가운데가 아닌 곳에서 잡았을 때 모서리가 튀지 않게 하는 보정.</summary>
        private Vector2 _grabOffset;

        /// <summary>한 부모 아래에 뷰포트가 둘 이상일 수 있어, 어느 뷰포트의 손잡이인지 밝혀 둔다.</summary>
        internal RectTransform Viewport => _viewport;

        internal static ResizeGrip Attach(RectTransform viewport)
        {
            var go = new GameObject(GripName, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(viewport.parent, false);
            // 앵커를 한 점으로 둬야 sizeDelta 가 곧 크기다. 자리는 SnapToCorner 에서 월드 좌표로 잡는다.
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Size, Size);

            // 레이아웃 그룹이 자리를 정하지 못하게 한다.
            go.AddComponent<LayoutElement>().ignoreLayout = true;

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.35f);
            image.raycastTarget = true;

            var grip = go.AddComponent<ResizeGrip>();
            grip._self = rt;
            grip._viewport = viewport;
            grip._canvas = viewport.GetComponentInParent<Canvas>();
            return grip;
        }

        /// <summary>창이 다시 쓰일 때마다 지금 상자의 값으로 갈아 준다.</summary>
        internal void Refresh(RectTransform content, string templateId, Vector2 naturalCells, int columns, int rows)
        {
            _content = content;
            _templateId = templateId;
            _naturalCells = naturalCells;
            _columns = columns;
            _rows = rows;

            var image = GetComponent<Image>();
            if (image != null) image.enabled = GridWindowScrollPlugin.ShowGrip.Value;
        }

        private void LateUpdate() => SnapToCorner();

        /// <summary>뷰포트 자리는 부모 레이아웃이 정하므로 모서리를 따라간다.</summary>
        internal void SnapToCorner()
        {
            if (_viewport == null || _self == null) return;

            var corners = new Vector3[4];
            _viewport.GetWorldCorners(corners);
            _self.position = corners[3];   // 0=왼아래 1=왼위 2=오른위 3=오른아래
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 잡은 자리와 지금 모서리의 차. 이걸 빼고 재야 처음 한 번에 모서리가 튀지 않는다.
            if (TryLocalPoint(eventData, out var pointer) && TryCorners(out _, out var bottomRight))
                _grabOffset = pointer - bottomRight;
            else
                _grabOffset = Vector2.zero;
        }

        /// <summary>
        /// 커서가 가리키는 자리에 모서리를 맞춘다. 끈 거리를 더해 가는 방식은 배율을 한 번
        /// 잘못 잡으면 커서와 모서리가 계속 벌어지므로 쓰지 않는다.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (_content == null) return;
            if (!TryLocalPoint(eventData, out var pointer)) return;
            if (!TryCorners(out var topLeft, out _)) return;

            var cell = GridScroll.CellPixels();
            if (cell <= 0f) return;

            var wanted = pointer - _grabOffset - topLeft;
            var columns = Mathf.RoundToInt(wanted.x / cell);
            var rows = Mathf.RoundToInt(-wanted.y / cell);   // 캔버스 좌표는 위가 +y 다

            // 격자보다 크게 늘릴 이유가 없고, 너무 줄이면 아무것도 안 보인다.
            columns = Mathf.Clamp(columns, GridScroll.MinCells, Mathf.Max(GridScroll.MinCells, Mathf.RoundToInt(_naturalCells.x)));
            rows = Mathf.Clamp(rows, GridScroll.MinCells, Mathf.Max(GridScroll.MinCells, Mathf.RoundToInt(_naturalCells.y)));

            if (columns == _columns && rows == _rows) return;

            if (GridWindowScrollPlugin.Verbose.Value)
                GridWindowScrollPlugin.Log.LogInfo(
                    $"끌기 — 커서 ({pointer.x:0},{pointer.y:0}) 왼위 ({topLeft.x:0},{topLeft.y:0}) "
                  + $"잡은차 ({_grabOffset.x:0},{_grabOffset.y:0}) → {columns}x{rows}칸 "
                  + $"(한 칸 {cell:0}, 캔버스 배율 {(_canvas != null ? _canvas.scaleFactor : 0f):0.##})");

            _columns = columns;
            _rows = rows;
            GridScroll.ApplyCells(_viewport, _content, _columns, _rows);
            SnapToCorner();   // 다음 프레임까지 기다리면 끄는 동안 손잡이가 뒤처져 보인다
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (GridWindowScrollPlugin.RememberPerContainer.Value && !string.IsNullOrEmpty(_templateId))
            {
                GridWindowScrollPlugin.Sizes.Set(_templateId, _columns, _rows);
                GridWindowScrollPlugin.Sizes.Save();
                GridWindowScrollPlugin.Log.LogInfo($"상자 {_templateId} 의 보이는 칸을 {_columns}x{_rows} 로 기억했다");
            }
            else
            {
                // 상자별로 기억하지 않으면 전체 기본값을 바꾼다. 어느 쪽이든 끌면 남는다.
                GridWindowScrollPlugin.MaxColumns.Value = Mathf.Clamp(_columns, 3, 40);
                GridWindowScrollPlugin.MaxRows.Value = Mathf.Clamp(_rows, 3, 40);
                GridWindowScrollPlugin.Log.LogInfo($"보이는 칸 기본값을 {_columns}x{_rows} 로 바꿨다");
            }
        }

        /// <summary>
        /// 커서를 캔버스 뿌리의 좌표로 옮긴다. 캔버스 뿌리의 1 단위가 UI 1 픽셀이고 한 칸이
        /// 63 단위라, 배율을 따로 곱하거나 나눌 일이 없다.
        /// </summary>
        private bool TryLocalPoint(PointerEventData eventData, out Vector2 point)
        {
            point = Vector2.zero;
            var root = _canvas != null ? _canvas.transform as RectTransform : null;
            if (root == null) return false;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root, eventData.position, eventData.pressEventCamera, out point);
        }

        /// <summary>뷰포트의 왼위·오른아래 모서리를 캔버스 뿌리 좌표로 준다.</summary>
        private bool TryCorners(out Vector2 topLeft, out Vector2 bottomRight)
        {
            topLeft = bottomRight = Vector2.zero;
            var root = _canvas != null ? _canvas.transform as RectTransform : null;
            if (root == null || _viewport == null) return false;

            var corners = new Vector3[4];
            _viewport.GetWorldCorners(corners);
            topLeft = root.InverseTransformPoint(corners[1]);
            bottomRight = root.InverseTransformPoint(corners[3]);
            return true;
        }
    }
}
