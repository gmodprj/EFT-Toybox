using System.Text.Json.Serialization;

namespace pitTeam.Server.Models;

// 프로필 두 개를 고치는 전송 하나의 기록. 저널에 "먼저" 적고 그 다음 실물을 움직인다.
// Stage 는 enum 이 아니라 문자열이다: 저장된 값이 알려진 이름이 아니면 TransferJournalPolicy 가 "모르는 단계" 로
// 닫는다. enum 으로 읽으면 직렬화기 설정에 따라 알 수 없는 값이 조용히 0(Prepared) 으로 떨어질 수 있는데,
// 그게 하필 가장 낙관적인 단계다.
// ItemIds 는 옮기는 아이템의 id 들 - 복구가 "출처에 아직 있나 / 목적지에 있나" 를 묻는 유일한 단서다.
// 그래서 전송은 아이템 id 를 바꾸지 않아야 한다(바꾸면 복구가 양쪽 어디에서도 못 찾아 멈추고 보고한다).
// 속성 이름은 고정한다: 디스크에 남은 미결 항목이 이름 변경으로 고아가 되면 안 된다.
public record StmTransferJournalEntry(
    [property: JsonPropertyName("journalId")] string JournalId,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("sourceProfileId")] string SourceProfileId,
    [property: JsonPropertyName("destinationProfileId")] string DestinationProfileId,
    [property: JsonPropertyName("itemIds")] List<string> ItemIds,
    [property: JsonPropertyName("stage")] string Stage,
    [property: JsonPropertyName("createdAtUnixSeconds")] long CreatedAtUnixSeconds);

// stm-transfer-journal.json 의 모양. Entries 에 기본값을 두지 않는다: "entries" 가 없거나 null 이면 null 로
// 읽혀, 문서 없음(= 미결 없음)과 읽을 수 없는 문서를 구별할 수 있다(StmRaidReceiptsDocument 와 같은 이유).
public record StmTransferJournalDocument
{
    [JsonPropertyName("entries")]
    public List<StmTransferJournalEntry>? Entries { get; set; }
}
