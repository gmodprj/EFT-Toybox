using System;

namespace pitTeam.Shared
{
    public enum PendingRefusalVerdict
    {
        // 이 팀원의 화면이고 아직 신선하다. 보여 준다.
        Show,
        // 다른 팀원의 것이거나 주인을 알 수 없다. 보이지 않고 대기도 지우지 않는다(주인의 화면이 기다린다).
        NotOwner,
        // 너무 오래됐다. 다른 세션의 잔재이거나 이미 다시 편집해 해결된 일일 수 있다. 버린다.
        Expired,
    }

    // 편집기가 사라진 뒤 도착한 저장 거절 사유를 지금 보여도 되는가. 잃어버린 사유보다 **틀린** 사유가 나쁘므로 주인을 먼저 본다.
    // 주인이나 화면의 팀원을 모르면(null·빈 값) 일치로 치지 않는다 - 서수 비교에서 null == null 이 참이 되는 틈을 막는다.
    // 의도적으로 catch 가 없다: 입력이 닿는 모든 경로는 반환으로 끝난다.
    public static class PendingRefusalPolicy
    {
        public static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);

        public static bool SameOwner(string? owner, string? viewer)
        {
            return !string.IsNullOrWhiteSpace(owner)
                && !string.IsNullOrWhiteSpace(viewer)
                && string.Equals(owner, viewer, StringComparison.Ordinal);
        }

        // 시각이 미래(시계가 뒤로 감)여도 만료로 치지 않는다 - 사유를 잃는 쪽으로 넘어지지 않는다.
        public static PendingRefusalVerdict Judge(string? owner, DateTime atUtc, string? viewer, DateTime nowUtc)
        {
            if (!SameOwner(owner, viewer)) return PendingRefusalVerdict.NotOwner;
            return nowUtc - atUtc > MaxAge ? PendingRefusalVerdict.Expired : PendingRefusalVerdict.Show;
        }
    }
}
