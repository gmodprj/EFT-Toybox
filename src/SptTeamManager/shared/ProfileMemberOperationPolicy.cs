namespace pitTeam.Shared
{
    // 상세 화면이 서버에 요청하는 조작 중, 런처 프로필 팀원에게는 뜻이 없는 것들.
    // 장비 편집(default-equipment)은 여기 없다: 그것은 프로필 팀원에게 뜻이 있는 조작이고 별도 경로가 판정한다.
    public enum ProfileMemberOperation
    {
        Rename,
        Suit,
        Aggression,
        Proficiency,
        Tactic,
        Insurance,
        Loadout,
    }

    // 프로필 팀원에게 이 조작을 거부할 때 플레이어에게 보여 줄 사유.
    //
    // 왜 조용히 성공하지 않고 거부하는가: 이 조작들은 PIT 가 자기 용병 문서를 고치는 길이다. 프로필 팀원의 Aid 로
    // 불리면 (a) 용병이 없어 not-found 로 던지거나, (b) 더 나쁘게는 같은 Aid 를 쓰는 용병이 있어 화면에 보이지
    // 않는 용병의 이름·외형이 바뀐다. 어느 쪽이든 플레이어가 본 팀원에게는 아무 일도 일어나지 않는다.
    //
    // 반환값은 절대 비지 않는다: 호출자가 "사유 없음 = 거부 안 함" 으로 읽는 틈을 두지 않는다. 열거에 없는 값도 거부다.
    public static class ProfileMemberOperationPolicy
    {
        // 이 Aid 가 프로필 팀원인지 판별하지 못했을 때(등록을 읽을 수 없는 등). "아니다" 로 읽어 PIT 로 넘기면 같은 Aid 의
        // 보이지 않는 용병이 바뀔 수 있으므로, 모를 때도 거부한다.
        public static string UndeterminedReason(ProfileMemberOperation operation) =>
            "이 팀원이 프로필 팀원인지 확인할 수 없어 조작을 거부했습니다. 잠시 뒤 다시 시도하세요.";

        public static string RefusalReason(ProfileMemberOperation operation)
        {
            switch (operation)
            {
                case ProfileMemberOperation.Rename:
                    return "런처 프로필의 이름은 그 프로필의 것입니다. 팀 화면에서는 바꿀 수 없습니다.";
                case ProfileMemberOperation.Suit:
                    return "런처 프로필의 외형은 그 프로필의 것입니다. 팀 화면에서는 바꿀 수 없습니다.";
                case ProfileMemberOperation.Aggression:
                case ProfileMemberOperation.Proficiency:
                case ProfileMemberOperation.Tactic:
                    return "이 설정은 고용한 용병 팀원용입니다. 프로필 팀원에게는 아직 적용되지 않습니다.";
                case ProfileMemberOperation.Insurance:
                    return "프로필 팀원의 보험은 그 프로필 자체의 보험입니다. 팀 화면에서는 구매할 수 없습니다.";
                case ProfileMemberOperation.Loadout:
                    return "프로필 팀원에게는 장비 프리셋이 없습니다. 장비는 장비 편집으로 바꿉니다.";
                default:
                    return "이 조작은 프로필 팀원에게 지원하지 않습니다.";
            }
        }
    }
}
