using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 계획 4 Task 2 의 전선 모양. 라우트 `/stm/members/kit-quote`.
//
// 어느 쪽 빌드인지를 **열쇠와 함께** 받는다: 같은 id 가 두 프로필에 있을 수 있다(프로필을 복사해 만든 경우).
// owner 는 "teammate" 또는 "player" 다. 모르는 값은 거절한다 - 추측해서 엉뚱한 프로필의 빌드를 입히지 않는다.
public record StmGearKitQuoteRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
    [JsonPropertyName("buildId")] public string? BuildId { get; set; }
    [JsonPropertyName("owner")] public string? Owner { get; set; }
}

// 칸 하나가 어떻게 되는지. 화면이 "무엇이 바뀌는가" 를 사람 말로 보여 주는 데 쓴다.
public record StmGearKitSlotRow
{
    [JsonPropertyName("slotId")] public string SlotId { get; set; } = string.Empty;
    // Unchanged | Replaced | Added | Removed
    [JsonPropertyName("outcome")] public string Outcome { get; set; } = string.Empty;
    [JsonPropertyName("buyCount")] public int BuyCount { get; set; }
    [JsonPropertyName("stashCount")] public int StashCount { get; set; }
}

public record StmGearKitQuoteResponse
{
    [JsonPropertyName("buildId")] public string BuildId { get; set; } = string.Empty;
    [JsonPropertyName("buildName")] public string BuildName { get; set; } = string.Empty;
    [JsonPropertyName("owner")] public string Owner { get; set; } = string.Empty;
    // 플레이어 계정에서 나갈 돈(루블). 살 것이 없으면 0 이다.
    [JsonPropertyName("total")] public int Total { get; set; }
    // 플레이어가 지금 가진 돈으로 살 수 있는가. 화면이 버튼을 죽이는 데 쓴다. 서버는 구매 때 다시 센다.
    [JsonPropertyName("affordable")] public bool Affordable { get; set; }
    [JsonPropertyName("playerRoubles")] public int PlayerRoubles { get; set; }
    [JsonPropertyName("slots")] public List<StmGearKitSlotRow> Slots { get; set; } = [];
    // 살 물건 수(부착물 포함). 값이 0 이고 Total 이 0 이면 이미 그 빌드를 입고 있다는 뜻이다.
    [JsonPropertyName("itemsToBuy")] public int ItemsToBuy { get; set; }
    // 그 팀원 스태시로 돌아갈 물건 수.
    [JsonPropertyName("itemsToStash")] public int ItemsToStash { get; set; }
}
