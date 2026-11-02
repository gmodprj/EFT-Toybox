namespace pitTeam.Shared
{
    public enum SaveOutcomeKind
    {
        // 서버가 쓰기를 확인했다.
        Confirmed,
        // 서버가 거절했다고 **말했다**. 아무것도 쓰이지 않았다.
        Refused,
        // 응답이 없거나 읽을 수 없다. 서버가 썼는지 모른다.
        Unknown,
    }

    public sealed record SaveOutcome(SaveOutcomeKind Kind, string Message);

    // 저장 응답의 뜻을 한 번 정한다. 원칙: 결과는 서버가 실제로 한 말에서 한 번 정해지고, 그 뒤의 어떤 단계(알리기 포함)도
    // 다시 분류하지 못한다. 읽지 못한 응답을 거절로 단정하지 않고(모름), 서버가 말한 거절을 모름으로 흐리지 않는다.
    // 의도적으로 catch 가 없다: 입력이 닿는 모든 경로는 반환으로 끝난다.
    public static class SaveOutcomePolicy
    {
        public const string RefusedWithoutReason = "서버가 이유를 알리지 않고 장비 저장을 거절했습니다.";

        public const string UnknownText = "저장 결과를 확인하지 못했습니다. 저장되었을 수 있으니 팀원 화면을 다시 열어 장비를 확인하세요.";

        // bodyPresent: 응답 본문이 비어 있지 않고 JSON 으로 읽혔는가. err/errmsg: 읽은 값(읽지 못했으면 err 가 null).
        public static SaveOutcome Classify(bool bodyPresent, int? err, string? errmsg)
        {
            if (!bodyPresent || err == null)
            {
                return new SaveOutcome(SaveOutcomeKind.Unknown, UnknownText);
            }

            if (err == 0)
            {
                return new SaveOutcome(SaveOutcomeKind.Confirmed, string.Empty);
            }

            return new SaveOutcome(SaveOutcomeKind.Refused, string.IsNullOrWhiteSpace(errmsg) ? RefusedWithoutReason : errmsg!);
        }
    }
}
