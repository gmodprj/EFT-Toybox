using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridWindowScroll
{
    /// <summary>
    /// 보조키(Shift·Ctrl)를 누른 채 끄는 동안에는 스크롤하지 않는 <see cref="ScrollRect"/>.
    ///
    /// **왜 필요한가.** `ScrollRect` 는 왼쪽 버튼 드래그를 자기 것으로 삼아 내용을 민다. 그런데
    /// UI Fixes 의 다중 선택도 **맨 왼쪽 드래그**로 선택 상자를 그린다(그 모드 설정의
    /// `Selection Box Key = Mouse0`). 그래서 스크롤이 생긴 상자·가방 안에서 여러 개를 고르려고
    /// 끌면 **선택 상자와 스크롤이 함께** 움직였다(2026-11-13 인게임 제보).
    ///
    /// 둘을 키로 가른다 — **보조키를 누른 드래그는 고르는 것**이고, 맨 드래그는 그대로 스크롤이다.
    /// 휠과 스크롤바는 아무 영향도 받지 않는다.
    ///
    /// **한 번 누르면 그 드래그 끝까지 간다.** 끄는 도중에 보조키를 떼어도 스크롤로 넘어가지
    /// 않는다(`suppressed`) — 중간에 바뀌면 화면이 튄다.
    /// </summary>
    internal class ModifierAwareScrollRect : ScrollRect
    {
        /// <summary>이번 드래그를 스크롤로 보지 않기로 했는가. 드래그가 끝날 때까지 유지된다.</summary>
        private bool suppressed;

        internal static bool ModifierHeld()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private static bool Splitting()
        {
            return GridWindowScrollPlugin.ModifierDragSelects != null
                && GridWindowScrollPlugin.ModifierDragSelects.Value;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            suppressed = Splitting() && ModifierHeld();
            if (suppressed) return;
            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (suppressed) return;
            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (suppressed)
            {
                suppressed = false;
                return;
            }

            base.OnEndDrag(eventData);
        }
    }
}
