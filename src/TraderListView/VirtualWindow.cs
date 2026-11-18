using System;

namespace TraderListView
{
    /// <summary>
    /// 가상 스크롤의 창: 맨 위에 보이는 줄 번호와, 만들어 둘 줄 칸 수(보이는 수 + 여유 2).
    /// 값이 같으면 다시 묶을 필요가 없다 — 스크롤 매 프레임 다시 묶지 않으려고 견준다.
    /// </summary>
    public readonly struct VirtualWindow : IEquatable<VirtualWindow>
    {
        public VirtualWindow(int first, int slots)
        {
            First = first;
            Slots = slots;
        }

        public int First { get; }

        public int Slots { get; }

        public static VirtualWindow Compute(float scrollTop, float rowHeight, float viewportHeight, int rowCount)
        {
            if (rowHeight <= 0f || viewportHeight <= 0f)
            {
                return new VirtualWindow(0, 0);
            }

            int first = Math.Max(0, (int)Math.Floor(scrollTop / rowHeight));
            if (rowCount > 0)
            {
                first = Math.Min(first, rowCount - 1);
            }

            int slots = (int)Math.Ceiling(viewportHeight / rowHeight) + 2;
            return new VirtualWindow(first, slots);
        }

        public bool Equals(VirtualWindow other) => First == other.First && Slots == other.Slots;

        public override bool Equals(object obj) => obj is VirtualWindow other && Equals(other);

        public override int GetHashCode() => (First * 397) ^ Slots;
    }
}
