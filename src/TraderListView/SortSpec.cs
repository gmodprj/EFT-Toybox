using System;

namespace TraderListView
{
    /// <summary>무엇으로 줄 세우나. <see cref="Default"/> 는 게임 기본 순서.</summary>
    public enum SortKey
    {
        Default,
        Price,
        Name,
        Stock,
        Loyalty,
    }

    /// <summary>정렬 키와 방향. 설정에는 <c>"Price:Desc"</c> 꼴의 글로 남긴다.</summary>
    public readonly struct SortSpec
    {
        public SortSpec(SortKey key, bool descending)
        {
            Key = key;
            Descending = descending;
        }

        public SortKey Key { get; }

        public bool Descending { get; }

        public static SortSpec DefaultAsc => new SortSpec(SortKey.Default, false);

        public string ToConfig() => Key + ":" + (Descending ? "Desc" : "Asc");

        /// <summary>못 읽으면 <see cref="DefaultAsc"/>.</summary>
        public static SortSpec Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return DefaultAsc;
            }

            var parts = text.Split(':');
            if (parts.Length != 2 || !Enum.TryParse(parts[0], ignoreCase: false, out SortKey key))
            {
                return DefaultAsc;
            }

            switch (parts[1])
            {
                case "Asc":
                    return new SortSpec(key, false);
                case "Desc":
                    return new SortSpec(key, true);
                default:
                    return DefaultAsc;
            }
        }
    }
}
