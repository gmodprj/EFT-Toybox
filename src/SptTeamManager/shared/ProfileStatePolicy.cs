using System;

namespace pitTeam.Shared
{
    // 등록된 프로필 팀원이 지금 어떤 상태인지를 플레이어의 말로 바꾼다.
    // AutoJoinChangeable 은 서버의 판정(해석기는 Ready 인 팀원만 받아들인다)을 화면이 미리 아는 것일 뿐이고, 거절하는
    // 쪽은 여전히 서버다. 이 정책이 열어 주는 것은 없다 - 닫힌 상태에서 이유를 말해 줄 뿐이다.
    public sealed record ProfileStateNotice(bool AutoJoinChangeable, string Message);

    public static class ProfileStatePolicy
    {
        public static ProfileStateNotice Describe(RosterRowStatus status)
        {
            switch (status)
            {
                case RosterRowStatus.Ready:
                    return new ProfileStateNotice(true, string.Empty);
                case RosterRowStatus.CurrentSession:
                    return new ProfileStateNotice(false,
                        "이 프로필은 지금 로그인해 있는 프로필입니다. 자기 자신은 팀원이 될 수 없어서 자동 합류를 바꿀 수 없습니다.");
                case RosterRowStatus.Empty:
                    return new ProfileStateNotice(false,
                        "이 프로필은 게임에 한 번 들어가야 팀원이 될 수 있습니다. 그때까지 자동 합류를 바꿀 수 없습니다.");
                case RosterRowStatus.Missing:
                    return new ProfileStateNotice(false,
                        "이 프로필은 더 이상 없습니다. 자동 합류를 바꿀 수 없습니다. 목록에서 빼 주세요.");
                default:
                    // Unreadable 과 이 코드가 모르는 모든 값. 열어 주지 않는 쪽으로 닫는다.
                    return new ProfileStateNotice(false,
                        "이 프로필을 읽을 수 없습니다. 읽을 수 있을 때까지 자동 합류를 바꿀 수 없습니다.");
            }
        }

        // 옵션 조회가 오류로 돌아왔을 때, 그 errmsg 가 STM 의 거절 사유(상태 이름)인지 가려 읽는다. errmsg 에는 PIT 의 문장
        // ("Unable to find teammate…")도 오는데, 그것을 상태로 읽으면 평범한 실패가 "읽을 수 없는 프로필" 로 둔갑한다.
        // 그래서 DescribeReason 과 달리 모르는 값은 안내가 아니라 "해당 없음"(false)이다 - 정확히 아는 이름만 읽는다.
        // "Unavailable" 은 서버가 등록 갱신에 실패했을 때 쓰는 이름이라 읽을 수 없음과 같은 안내를 준다.
        public static bool TryDescribeRefusal(string? errmsg, out ProfileStateNotice notice)
        {
            notice = new ProfileStateNotice(false, string.Empty);
            switch (errmsg)
            {
                case "CurrentSession":
                    notice = Describe(RosterRowStatus.CurrentSession);
                    return true;
                case "Empty":
                    notice = Describe(RosterRowStatus.Empty);
                    return true;
                case "Missing":
                    notice = Describe(RosterRowStatus.Missing);
                    return true;
                case "Unreadable":
                case "Unavailable":
                    notice = Describe(RosterRowStatus.Unreadable);
                    return true;
                default:
                    return false;
            }
        }

        // 서버가 거절 사유로 돌려준 문자열(해석기의 사유는 로스터 와이어의 상태 이름과 같은 어휘다)을 읽는다.
        // 로스터와 같은 해석(StmRosterWire.ParseStatus)을 쓴다 - 어휘를 둘로 나누지 않는다.
        // 비었거나 모르는 사유, 그리고 사유 자리에 Ready 가 온 경우는 모두 읽을 수 없음이다: 거절의 사유가 "문제 없음" 일 수는 없다.
        public static ProfileStateNotice DescribeReason(string? reason)
        {
            var status = string.IsNullOrWhiteSpace(reason) ? RosterRowStatus.Unreadable : StmRosterWire.ParseStatus(reason);
            return status == RosterRowStatus.Ready ? Describe(RosterRowStatus.Unreadable) : Describe(status);
        }
    }
}
