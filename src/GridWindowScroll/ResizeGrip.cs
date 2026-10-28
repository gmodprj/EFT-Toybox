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
        private RectTransform _content;
        private Canvas _canvas;

        private string _templateId;
        private Vector2 _naturalCells;
        private int _columns;
        private int _rows;

        private Vector2 _pointerAtStart;
        private int _columnsAtStart;
        private int _rowsAtStart;

        /// <summary>한 부모 아래에 뷰포트가 둘 이상일 수 있어, 어느 뷰포트의 손잡이인지 밝혀 둔다.</summary>
        internal RectTransform Viewport => _viewport;

        internal static ResizeGrip Attach(RectTransform viewport)
        {
            var go = new GameObject(GripName, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(viewport.parent, false);
            // 앵커를 한 점으로 둬야 sizeDelta 가 곧 크기다. 자리는 LateUpdate 에서 월드 좌표로 잡는다.
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Size, Size);

            // 레이아웃 그룹이 자리를 정하지 못하게 한다. 자리는 LateUpdate 에서 직접 잡는다.
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

        private void LateUpdate()
        {
            if (_viewport == null || _self == null) return;

            // 뷰포트 자리는 부모 레이아웃이 정하므로 매 프레임 모서리를 따라간다.
            var corners = new Vector3[4];
            _viewport.GetWorldCorners(corners);
            _self.position = corners[3];   // 0=왼아래 1=왼위 2=오른위 3=오른아래
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _pointerAtStart = eventData.position;
            _columnsAtStart = _columns;
            _rowsAtStart = _rows;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_content == null) return;

            var cell = GridScroll.CellPixels() * (_canvas != null ? _canvas.scaleFactor : 1f);
            if (cell <= 0f) return;

            var moved = eventData.position - _pointerAtStart;
            var columns = _columnsAtStart + Mathf.RoundToInt(moved.x / cell);
            var rows = _rowsAtStart + Mathf.RoundToInt(-moved.y / cell);   // 아래로 끌면 세로가 늘어난다

            // 격자보다 크게 늘릴 이유가 없고, 너무 줄이면 아무것도 안 보인다.
            columns = Mathf.Clamp(columns, GridScroll.MinCells, Mathf.Max(GridScroll.MinCells, Mathf.RoundToInt(_naturalCells.x)));
            rows = Mathf.Clamp(rows, GridScroll.MinCells, Mathf.Max(GridScroll.MinCells, Mathf.RoundToInt(_naturalCells.y)));

            if (columns == _columns && rows == _rows) return;

            _columns = columns;
            _rows = rows;
            GridScroll.ApplyCells(_viewport, _content, _columns, _rows);
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
    }
}
