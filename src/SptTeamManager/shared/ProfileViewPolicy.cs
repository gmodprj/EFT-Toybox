using System;

namespace pitTeam.Shared
{
    public enum ProfileViewRoute
    {
        // 등록을 읽을 수 있다. 평소의 순서 그대로: STM 해석기를 PIT 보다 먼저 묻는다(불변식 3).
        StmFirst,
        // 등록을 읽을 수 없지만 PIT 가 자기 용병으로 아는 Aid 다. PIT 의 응답을 낸다.
        ServePit,
        // 등록을 읽을 수 없고 PIT 의 용병도 아니다: 등록된 런처 프로필일 수 있다. 조회를 거절한다.
        Refuse,
    }

    // 프로필 조회 라우트에서, 등록(STM)을 읽을 수 없는 상태의 길을 정한다.
    //
    // 왜 갈라 두는가: 이 라우트의 꼬리는 SPT 의 응답이고, SPT 는 aid 로 다른 런처 프로필을 직접 찾아 인벤토리 전체를 싣는다.
    // 그래서 "이 Aid 가 등록된 프로필일 수 있다" 면 막아야 한다. 하지만 PIT 의 용병 데이터는 PIT 의 것이라 거기서 다른 프로필의 스태시가
    // 샐 수 없다 - 용병 조회까지 막으면 STM 의 문제가 PIT 의 기능을 깬다(이 프로젝트의 실패 조건).
    // 용병과 등록 프로필이 Aid 를 나눠 가진 상태에서 이 완화가 용병을 보여 주는 것은 틀린 화면 데이터일 뿐 유출도 아이템 손실도
    // 아니므로 더 싼 오류 쪽이다.
    //
    // 평소(등록을 읽을 수 있음)의 순서는 건드리지 않는다: pitKnowsAid 는 StmFirst 일 때 호출되지 않는다(테스트로 고정).
    // 편집 쪽 가드(StmProfileOperationGuard)는 이 정책의 대상이 아니다 - 보이지 않는 용병을 고치면 장비가 사라질 수 있어 거기서는
    // 모를 때 계속 거부한다.
    public static class ProfileViewPolicy
    {
        public static ProfileViewRoute Decide(bool registrationsUsable, Func<bool> pitKnowsAid)
        {
            if (registrationsUsable)
            {
                return ProfileViewRoute.StmFirst;
            }

            // PIT 에 물을 수 없으면 용병인지 알 수 없다: 닫힌 쪽.
            if (pitKnowsAid == null)
            {
                return ProfileViewRoute.Refuse;
            }

            return pitKnowsAid() ? ProfileViewRoute.ServePit : ProfileViewRoute.Refuse;
        }
    }
}
