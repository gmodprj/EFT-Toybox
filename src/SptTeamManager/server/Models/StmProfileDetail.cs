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
