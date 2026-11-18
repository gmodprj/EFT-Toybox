namespace TraderListView
{
    /// <summary>
    /// 상인 물건 한 줄의 값. 정렬과 목록 표시가 이것만 본다(EFT 를 모른다).
    /// <see cref="Tag"/> 에 게임의 <c>Item</c> 을 담아 두고, 게임 쪽 코드만 꺼내 쓴다.
    /// </summary>
    public sealed class OfferRow
    {
        public OfferRow(string name, double? rubPrice, long stock, int loyaltyLevel, bool locked, int defaultIndex, object tag)
        {
            Name = name ?? string.Empty;
            RubPrice = rubPrice;
            Stock = stock;
            LoyaltyLevel = loyaltyLevel;
            Locked = locked;
            DefaultIndex = defaultIndex;
            Tag = tag;
        }

        /// <summary>화면에 보이는 이름(게임 locale).</summary>
        public string Name { get; }

        /// <summary>루블 환산 가격. 모르면 null — 정렬에서 방향과 상관없이 맨 뒤.</summary>
        public double? RubPrice { get; }

        /// <summary>살 수 있는 수(재고, 구매 제한이 있으면 그것까지 반영한 값).</summary>
        public long Stock { get; }

        /// <summary>이 물건이 걸린 신뢰 등급.</summary>
        public int LoyaltyLevel { get; }

        /// <summary>지금 등급으로는 못 사는지.</summary>
        public bool Locked { get; }

        /// <summary>게임 기본 순서(격자에 놓이던 순서)에서의 자리.</summary>
        public int DefaultIndex { get; }

        /// <summary>게임 쪽 원본(<c>EFT.InventoryLogic.Item</c>). 순수 코드는 손대지 않는다.</summary>
        public object Tag { get; }
    }
}
