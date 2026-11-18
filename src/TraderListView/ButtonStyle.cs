namespace TraderListView
{
    /// <summary>
    /// 헤더 아이콘 버튼의 공통 규격(사용자 제안서 2026-11-18 §4.1·§5.1).
    /// 상태 우선순위: 비활성 &gt; 눌림 &gt; 활성 &gt; 마우스 오버 &gt; 기본 — 지금 상태(활성)가 마우스 오버에 가리지 않게.
    /// </summary>
    public static class ButtonStyle
    {
        public const string Default = "#202528";
        public const string Hover = "#343B3F";
        public const string Pressed = "#16191B";
        public const string Active = "#477D9D";
        public const string Disabled = "#181B1D";
        public const string Border = "#454B4E";
        public const string BorderFocus = "#477D9D";
        public const string Icon = "#C5C9C7";
        public const string IconDisabled = "#5A6063";
        public const string Placeholder = "#858B8E";

        public static string Background(bool active, bool hover, bool pressed, bool disabled)
        {
            if (disabled)
            {
                return Disabled;
            }

            if (pressed)
            {
                return Pressed;
            }

            if (active)
            {
                return Active;
            }

            return hover ? Hover : Default;
        }

        public static string IconColor(bool disabled) => disabled ? IconDisabled : Icon;
    }
}
