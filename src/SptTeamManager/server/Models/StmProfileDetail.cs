using System.Text.Json.Serialization;
using pitTeam.Shared;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 저널 읽기 결과(서버 안에서만 쓴다, 와이어에 실리지 않는다). 항목마다 "플레이어가 지금 닫을 수 있나" 의 판정이 붙는다.
// JournalReadable == false 는 "항목이 없다" 가 아니라 "문서를 못 읽었다" 다 - 닫힌 쪽(편집 막힘)으로 다룬다.
public sealed record StmBlockingEntry(StmTransferJournalEntry Entry, TransferCloseVerdict Verdict);

public sealed record StmBlockingEntries(bool JournalReadable, IReadOnlyList<StmBlockingEntry> Entries);

// 프로필 팀원 상세 화면에 실리는 STM 쪽 정보. 옵션 응답의 "stm" 에 붙는다(PIT 용병의 응답에는 없다 = null).
// 안내문은 TransferEntryClosePolicy 가 만든 플레이어의 말이다. 화면은 그대로 보여 주기만 하고 판정하지 않는다.
// 속성 이름은 고정한다(클라이언트가 같은 이름으로 읽는다).
public record StmProfileDetail
{
    [JsonPropertyName("profileId")] public string ProfileId { get; set; } = string.Empty;
    // 이 팀원의 편집이 전송 저널에 막혀 있는가. 저널을 못 읽어도 true(의심스러우면 막는다).
    [JsonPropertyName("blocked")] public bool Blocked { get; set; }
    [JsonPropertyName("blockedNotice")] public string BlockedNotice { get; set; } = string.Empty;
    // 닫을 수 있는 항목이 있을 때만 값이 있다.
    [JsonPropertyName("journalId")] public string? JournalId { get; set; }
    [JsonPropertyName("canClose")] public bool CanClose { get; set; }
    [JsonPropertyName("closeWarning")] public string? CloseWarning { get; set; }
    // 저널 문서를 읽을 수 없을 때의 출구(문서를 통째로 버린다). 이때만 true 이고 경고문이 따라온다.
    [JsonPropertyName("canDiscardJournal")] public bool CanDiscardJournal { get; set; }
    [JsonPropertyName("discardWarning")] public string? DiscardWarning { get; set; }
}

// 읽을 수 없는 저널 문서를 플레이어가 버리겠다고 확인했을 때의 요청. Acknowledged 기본값은 false.
public record StmDiscardJournalRequest : IRequestData
{
    [JsonPropertyName("acknowledged")] public bool Acknowledged { get; set; }
}

// 플레이어가 막힌 전송 항목을 닫겠다고 확인했을 때의 요청. Acknowledged 는 기본값이 false 이고, 화면은 경고창에서
// 확인 버튼을 눌렀을 때만 true 로 보낸다. 서버는 이 값만 믿지 않고 정책으로 다시 판정한다.
public record StmCloseTransferRequest : IRequestData
{
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
    [JsonPropertyName("journalId")] public string? JournalId { get; set; }
    [JsonPropertyName("acknowledged")] public bool Acknowledged { get; set; }
}

// 프로필 팀원의 장비 편집 요청. 한 번에 여러 건을 받아 **쓰기 한 번**으로 끝낸다(SaveProfile 한 번 +
// 디스크 재독 한 번). 쪼개 부르면 중간에 하나가 실패했을 때 앞의 것은 이미 적용돼 '반쯤 입은' 상태가 남는다.
// 각 건은 '아이템 하나를 어디로' 이고, 최종 집합은 **서버가** 만든다 - PIT 의 default-equipment 처럼
// 클라이언트가 최종 인벤토리를 제출하는 모양은 제출된 것이 곧 새 상태가 되어 '빠진 아이템' 이 조용히
// 사라진다(그 구멍을 메우려고 보존 검사를 따로 넣었다). 서버가 자기 프로필을 기준으로 옮기면 사라질 것이 없다.
// Aid 가 함께 오는 것은 해석기가 Aid 로 찾기 때문이고(읽기 한 곳), ProfileId 는 믿지 않고 대조한다.
public record StmGearEditRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
    [JsonPropertyName("edits")] public List<StmGearEditItem>? Edits { get; set; }
}

// 한 건: 이 아이템을 이 부모의 이 슬롯으로. TargetParentId 가 비면 장비 루트 아래를 뜻한다.
// Location(격자 좌표)이 없는 것은 의도다 - 이 경로는 슬롯에 넣는 조작만 표현한다(ProfileGearEditPolicy).
public record StmGearEditItem
{
    [JsonPropertyName("itemId")] public string? ItemId { get; set; }
    [JsonPropertyName("targetSlotId")] public string? TargetSlotId { get; set; }
    [JsonPropertyName("targetParentId")] public string? TargetParentId { get; set; }
}
