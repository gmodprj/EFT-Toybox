namespace pitTeam.Shared
{
    // 등록된 프로필 팀원 한 건의 저장 필드. 서버의 StmProfileMember 를 SPT 타입 없이 옮긴 것이다 -
    // 이 판정은 서버 프로젝트 밖(테스트)에서 검증되어야 해서 모델 자체를 참조하지 않는다.
    public sealed record StmProfileMemberFields(string ProfileId, string Tactic, bool AutoJoin, bool HandEdited);

    // 바꾸고 싶은 필드만 값을 채운다. null 은 "건드리지 않는다".
    // ProfileId 가 여기 없는 것은 의도다: 등록을 가리키는 열쇠라 갱신 경로로 바꿀 수 없다.
    // Tactic 은 2026-11-02 에 열었다 - 그 전에는 저장만 되고 바꿀 길이 없었다(상세 화면이 전술 조작을 거부했다).
    public sealed record StmProfileMemberChange(bool? AutoJoin, bool? HandEdited, string? Tactic = null);

    // 기존 등록의 한 필드를 바꾸는 규칙. 순수하고, 예외를 밖으로 던지지 않는다.
    public static class ProfileMemberUpdatePolicy
    {
        // 유일한 비자명 규칙: HandEdited 는 단방향이다(false -> true 만). 되돌릴 수 있으면
        // "편집했는데 임시 장비가 입혀진다" 가 다시 열린다. 사용자의 되돌리는 길은 등록을 지우고 다시 넣는 것이다.
        // current 가 null 이면 바꿀 대상이 없으니 null. change 가 null 이면 바꿀 것이 없으니 current 그대로.
        //
        // Tactic: 빈 문자열·공백은 "건드리지 않는다" 로 읽는다. 빈 값을 저장하면 투입 때 PIT 의 기본값으로
        // 떨어지는데(NormalizeCombatTactic), 그것은 "지우기" 가 아니라 **조용한 되돌리기**다 - 사용자가 고른
        // 전술이 사라진 줄 모른 채 기본값으로 싸우게 된다. 값이 올바른 전술인지는 서버가 PIT 의 목록으로
        // 판정해 정규화한 뒤 넘긴다(이 자리에서 목록을 또 적으면 둘이 갈라진다 - 인계 §3-8).
        public static StmProfileMemberFields? Apply(StmProfileMemberFields? current, StmProfileMemberChange? change)
        {
            if (current == null) return null;
            if (change == null) return current;
            return current with
            {
                AutoJoin = change.AutoJoin ?? current.AutoJoin,
                HandEdited = current.HandEdited || change.HandEdited == true,
                Tactic = string.IsNullOrWhiteSpace(change.Tactic) ? current.Tactic : change.Tactic!.Trim(),
            };
        }
    }
}
