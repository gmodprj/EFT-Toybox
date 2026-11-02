namespace pitTeam.Shared
{
    // 상세 화면이 서버에 요청하는 조작 중, 런처 프로필 팀원에게 PIT 의 용병 경로로는 뜻이 없는 것들.
    // DefaultEquipmentCommit(장비 편집 저장)이 여기 들어온 것은 계획 3 Task 6 의 결정이다: 장비 편집
    // 자체는 프로필 팀원에게 뜻이 있지만, **PIT 의 저장 경로**는 플레이어 스태시를 출처로 쓰므로
    // 프로필 팀원에게 쓸 수 없다. 프로필 팀원의 장비는 STM 자신의 경로로만 바뀐다.
    public enum ProfileMemberOperation
    {
        Rename,
        Suit,
        Aggression,
        Proficiency,
        Tactic,
        Insurance,
        Loadout,
        // 키트 구매·장비 수리. 둘 다 플레이어 프로필(돈·수리 키트)과 팀원 쪽을 함께 고치는 조작이라
        // 프로필 팀원에게는 프로필 두 개를 오가는 전송이 된다 - 설계 §6-5 가 다음으로 미룬 그것이다.
        // 손대지 않고 두면 같은 Aid 의 용병에게 적용되므로(계획 3 Task 5 이월) 거부로 닫는다.
        BuyKit,
        RepairEquipment,
        DefaultEquipmentCommit,
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
                case ProfileMemberOperation.BuyKit:
                    return "프로필 팀원에게는 키트를 사 줄 수 없습니다. 장비는 그 프로필 스태시에 있는 것으로만 꾸립니다.";
                case ProfileMemberOperation.RepairEquipment:
                    return "프로필 팀원의 장비 수리는 그 프로필로 로그인해서 합니다. 팀 화면에서는 수리할 수 없습니다.";
                // PIT 의 저장 경로는 제출된 PlayerStashItems 로 **플레이어** 스태시 트리를 갈아 끼우고
                // 결과를 PIT 의 용병 문서에 쓴다. 프로필 팀원에게는 둘 다 틀렸다: 출처는 그 팀원
                // 스태시뿐이고(§6-4), 장비가 가야 할 곳은 그 런처 프로필이다. 조용히 성공한 척하면
                // 같은 Aid 의 **보이지 않는 용병** 장비가 바뀐다(인계 문서 불변식 3).
                case ProfileMemberOperation.DefaultEquipmentCommit:
                    return "프로필 팀원의 장비는 용병 장비 저장 경로로 바꿀 수 없습니다. 그 경로는 플레이어 스태시를 출처로 쓰지만, 프로필 팀원의 장비 출처는 그 팀원 자신의 스태시뿐입니다.";
                default:
                    return "이 조작은 프로필 팀원에게 지원하지 않습니다.";
            }
        }
    }
}
