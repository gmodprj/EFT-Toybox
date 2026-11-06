namespace pitTeam.Shared
{
    // 꾸러미 구매를 시작해도 되는가. 하나라도 걸리면 **아무것도 쓰지 않고** 멈춘다.
    public enum GearKitPurchaseGate
    {
        Allowed,
        // 레이드 중에는 프로필을 건드리지 않는다(장비 편집과 같은 관문).
        RaidInProgress,
        // 끝나지 않은 전송 기록이 이 팀원을 막고 있다.
        JournalBlocked,
        // 살 것이 없다. 이미 그 빌드를 입고 있다는 뜻이고, 실패가 아니라 할 일이 없는 것이다.
        NothingToBuy,
        // 화면이 보여 준 값과 지금 값이 다르다. 상인 가격·거래 등급·장비 상태가 그 사이에 바뀌었다.
        QuoteChanged,
        // 돈이 모자란다.
        InsufficientFunds,
    }

    // 구매 직전의 관문. 순수하고, 던지지 않는다.
    //
    // 왜 값을 다시 매기고 **보여 준 값과 견주는가**: 화면이 견적을 띄운 뒤 구매를 누르기까지 시간이 있고,
    // 그 사이에 상인 등급·평판·팀원 장비가 바뀌면 값이 달라진다. 달라진 값을 말없이 청구하면 플레이어는
    // 보지 못한 금액을 낸 것이 된다. PIT 의 고용 흐름도 같은 이유로 만료된 견적을 거절한다
    // (socialUi.TeammateHireQuoteExpired).
    public static class GearKitPurchasePolicy
    {
        public static GearKitPurchaseGate Decide(
            bool raidInProgress,
            bool journalBlocked,
            int itemsToBuy,
            int quotedTotal,
            int freshTotal,
            int playerRoubles)
        {
            // 순서가 뜻을 가진다: 레이드와 저널은 "지금은 안 된다", 나머지는 "이 구매는 안 된다" 다.
            if (raidInProgress) return GearKitPurchaseGate.RaidInProgress;
            if (journalBlocked) return GearKitPurchaseGate.JournalBlocked;
            if (itemsToBuy <= 0) return GearKitPurchaseGate.NothingToBuy;
            if (quotedTotal != freshTotal) return GearKitPurchaseGate.QuoteChanged;
            if (playerRoubles < freshTotal) return GearKitPurchaseGate.InsufficientFunds;
            return GearKitPurchaseGate.Allowed;
        }

        // 플레이어가 읽는 말. 비지 않는다 - 호출자가 "사유 없음 = 통과" 로 읽는 틈을 두지 않는다.
        public static string Describe(GearKitPurchaseGate gate, int freshTotal = 0, int playerRoubles = 0)
        {
            switch (gate)
            {
                case GearKitPurchaseGate.Allowed:
                    return string.Empty;
                case GearKitPurchaseGate.RaidInProgress:
                    return "레이드 중에는 팀원의 장비를 살 수 없습니다. 레이드가 끝난 뒤에 하세요.";
                case GearKitPurchaseGate.JournalBlocked:
                    return "이 팀원에게 끝나지 않은 아이템 전송 기록이 남아 있어 장비를 살 수 없습니다.";
                case GearKitPurchaseGate.NothingToBuy:
                    return "이 팀원은 이미 그 장비를 그대로 입고 있습니다. 살 것이 없습니다.";
                case GearKitPurchaseGate.QuoteChanged:
                    return "값이 바뀌었습니다. 목록을 다시 열어 새 값을 확인한 뒤 사 주세요.";
                case GearKitPurchaseGate.InsufficientFunds:
                    return $"돈이 모자랍니다. {freshTotal:N0} ₽ 가 필요한데 {playerRoubles:N0} ₽ 를 갖고 있습니다.";
                default:
                    return "지금은 이 장비를 살 수 없습니다.";
            }
        }
    }
}
