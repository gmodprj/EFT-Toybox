namespace pitTeam.Shared
{
    // 등록된 프로필 팀원 한 건의 저장 필드. 서버의 StmProfileMember 를 SPT 타입 없이 옮긴 것이다 -
    // 이 판정은 서버 프로젝트 밖(테스트)에서 검증되어야 해서 모델 자체를 참조하지 않는다.
    public sealed record StmProfileMemberFields(string ProfileId, string Tactic, bool AutoJoin, bool HandEdited);

    // 바꾸고 싶은 필드만 값을 채운다. null 은 "건드리지 않는다".
    // ProfileId·Tactic 이 여기 없는 것은 의도다: 갱신 경로로는 바꿀 수 없다.
    public sealed record StmProfileMemberChange(bool? AutoJoin, bool? HandEdited);

    // 기존 등록의 한 필드를 바꾸는 규칙. 순수하고, 예외를 밖으로 던지지 않는다.
    public static class ProfileMemberUpdatePolicy
    {
        // 유일한 비자명 규칙: HandEdited 는 단방향이다(false -> true 만). 되돌릴 수 있으면
        // "편집했는데 임시 장비가 입혀진다" 가 다시 열린다. 사용자의 되돌리는 길은 등록을 지우고 다시 넣는 것이다.
        // current 가 null 이면 바꿀 대상이 없으니 null. change 가 null 이면 바꿀 것이 없으니 current 그대로.
        public static StmProfileMemberFields? Apply(StmProfileMemberFields? current, StmProfileMemberChange? change)
        {
            if (current == null) return null;
            if (change == null) return current;
            return current with
            {
                AutoJoin = change.AutoJoin ?? current.AutoJoin,
                HandEdited = current.HandEdited || change.HandEdited == true,
            };
        }
    }
}
