using System;

namespace pitTeam.Shared
{
    // 상세 화면 입구의 판정 결과. CanEdit == false 면 Notice 가 플레이어가 읽는 이유다(비지 않는다).
    public sealed record GearEditEntry(bool CanEdit, string Notice);

    // 프로필 팀원의 상세 화면에서 "장비 편집" 을 열어도 되는가, 안 되면 무엇을 말하는가.
    //
    // 서버가 옵션 응답에 이 판정을 실어 보내고 화면은 그대로 보여 준다. 서버는 저장 때 같은 관문(레이드, 미결 전송)을
    // 다시 선다 - 여기는 화면이 저장에서 거절될 편집을 시켜 놓고 헛걸음하게 하지 않기 위한 것이다. 그래서 둘의 사유는
    // 같은 문장이어야 하고, 같은 상수를 쓰는 것으로 강제한다.
    //
    // 읽을 수 없는 프로필은 여기 오지 않는다: 그 팀원은 옵션 조회가 사유를 담은 오류로 돌아와 상태 안내가 이미 뜬다
    // (ProfileStatePolicy). 의도적으로 catch 가 없다.
    public static class GearEditEntryPolicy
    {
        // StmGearEditService 가 저장 때 거절하는 문장과 같다. 한 곳에 둔다.
        public const string RaidInProgressReason = "레이드가 진행 중일 때는 팀원의 장비를 고칠 수 없습니다. 레이드가 끝난 뒤에 다시 시도하세요.";

        // 이유를 말해 줄 값이 없을 때의 문구. 이유를 지어내지 않는다.
        public const string NeutralUnavailableNotice = "지금은 이 팀원의 장비를 편집할 수 없습니다.";

        public static GearEditEntry Decide(bool blocked, string? blockedNotice, bool raidInProgress)
        {
            // 막힘이 먼저다: 플레이어가 닫기 버튼으로 풀 수 있는 이유이고, 풀면 레이드 사유가 이어서 나온다.
            if (blocked)
            {
                return new GearEditEntry(false, NoticeWhenNotEditable(blockedNotice));
            }

            if (raidInProgress)
            {
                return new GearEditEntry(false, RaidInProgressReason);
            }

            return new GearEditEntry(true, string.Empty);
        }

        // 열 수 없을 때 화면에 올릴 문장. 서버가 이유를 싣지 않았으면 중립 문구다 - 값이 없다고 열어 주지 않는다.
        public static string NoticeWhenNotEditable(string? serverNotice)
        {
            return string.IsNullOrWhiteSpace(serverNotice) ? NeutralUnavailableNotice : serverNotice!;
        }
    }
}
