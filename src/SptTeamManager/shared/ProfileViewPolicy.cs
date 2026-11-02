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
    // 팀원 조회 한 번의 결과를 네 가지로 가른다. 핵심은 ReadFailed 와 NonMember 의 구별이다: 해석기는 등록·카탈로그를 읽다 던지면
    // 식별 전 실패로 삼키고 "사유 없는 false" 를 주는데, 그 모양이 확인된 비팀원(읽기가 다 성공했고 이 Aid 가 등록에 없음)과 똑같다.
    // 둘을 같게 보면 읽기 한 번의 실패(문서 손상, 데이터베이스 오류)가 SPT 의 응답을 그대로 흘려보낸다.
    public enum ProfileLookupOutcome
    {
        Resolved,
        // 읽기가 모두 성공했고 이 Aid 는 등록된 프로필이 아니다. PIT 길로 흘러야 하는 유일한 경우다.
        NonMember,
        // 등록된 프로필 팀원인데 쓸 수 없다(사유가 있다).
        Unavailable,
        // 등록·카탈로그를 읽지 못했다. 이 Aid 가 무엇인지 모른다 - 닫힌 쪽.
        ReadFailed,
    }

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
        public static ProfileLookupOutcome ClassifyLookup(bool readFailed, bool resolved, string? unavailableReason)
        {
            if (readFailed) return ProfileLookupOutcome.ReadFailed;
            if (resolved) return ProfileLookupOutcome.Resolved;
            return string.IsNullOrWhiteSpace(unavailableReason) ? ProfileLookupOutcome.NonMember : ProfileLookupOutcome.Unavailable;
        }

        // 팩토리가 "읽기 실패" 를 호출자에게 알리는 사유 값. 상태 이름(CurrentSession 등)과 겹치지 않고, "Unreadable"(식별된
        // 팀원을 읽을 수 없음)과도 다르다 - 호출자마다 이 신호에 맞는 길(PIT 용병 먼저 / PIT 길)을 고르게 하려는 것이다.
        public const string ReadFailedReason = "ReadFailed";

        // 조회 결과 하나로 길을 정한다. 읽기 실패만 길이 갈린다(PIT 용병이면 PIT 응답, 아니면 거절). 그 밖의 결과는 호출자가 평소처럼
        // 결과를 따른다(StmFirst) - 확인된 비팀원이 PIT 길로 흐르는 것도 거기에 든다. 정의 밖 분류는 닫힌 쪽(읽기 실패와 같다).
        public static ProfileViewRoute RouteAfterLookup(ProfileLookupOutcome outcome, Func<bool> pitKnowsAid)
        {
            switch (outcome)
            {
                case ProfileLookupOutcome.Resolved:
                case ProfileLookupOutcome.NonMember:
                case ProfileLookupOutcome.Unavailable:
                    return ProfileViewRoute.StmFirst;
                default:
                    return Decide(registrationsUsable: false, pitKnowsAid);
            }
        }

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
