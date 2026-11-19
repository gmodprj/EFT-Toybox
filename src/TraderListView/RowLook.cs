namespace TraderListView
{
    /// <summary>
    /// 목록 줄이 게임 격자 칸과 같게 보이고 움직이도록 하는 판단(게임 타입 없이 시험할 수 있게 뗐다).
    /// 근거는 게임 코드다 — 미조사 이름은 <c>GridItemView.GetItemName</c>·<c>TradingItemView</c> 툴팁,
    /// 실루엣 색은 <c>ItemView.OnBeginDrag</c>, 클릭 갈래는 <c>ItemView.OnClick</c>.
    /// </summary>
    public static class RowLook
    {
        public enum Button { Left, Right, Middle }

        public enum Action { None, Select, Inspect, ContextMenu }

        /// <summary>조사하지 않은 물건은 이름을 감춘다(격자는 "알 수 없는 아이템").</summary>
        public static string Name(bool examined, string realName, string unknownName) => examined ? realName : unknownName;

        /// <summary>아이콘 색(RGBA). 조사하지 않은 물건은 검은 실루엣.</summary>
        public static (float r, float g, float b, float a) IconTint(bool examined) =>
            examined ? (1f, 1f, 1f, 1f) : (0f, 0f, 0f, 0.85f);

        /// <summary>
        /// 왼쪽 한 번 = 고르기(구매 창), 왼쪽 두 번 = 상세 창, 오른쪽 = 문맥 메뉴(조사·상세정보 등).
        /// 가운데는 상인 칸에서 할 일이 없다.
        /// </summary>
        public static Action Route(Button button, int clickCount)
        {
            switch (button)
            {
                case Button.Left:
                    return clickCount >= 2 ? Action.Inspect : Action.Select;
                case Button.Right:
                    return Action.ContextMenu;
                default:
                    return Action.None;
            }
        }
    }
}
