using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 계획 4 Task 3 의 전선 모양. 라우트 `/stm/members/kit-buy`.
//
// ExpectedTotal 은 **화면이 사용자에게 보여 준 값**이다. 서버는 사기 직전에 다시 매겨 이 값과 견주고,
// 다르면 거절한다(GearKitPurchasePolicy). 보지 못한 금액을 청구하지 않기 위한 것이고, 그래서
// "그냥 지금 값으로 사라" 는 뜻의 요청은 만들지 않았다.
public record StmGearKitPurchaseRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
    [JsonPropertyName("buildId")] public string? BuildId { get; set; }
    [JsonPropertyName("owner")] public string? Owner { get; set; }
    [JsonPropertyName("expectedTotal")] public int ExpectedTotal { get; set; }
}

public record StmGearKitPurchaseResponse
{
    [JsonPropertyName("buildId")] public string BuildId { get; set; } = string.Empty;
    [JsonPropertyName("buildName")] public string BuildName { get; set; } = string.Empty;
    // 실제로 빠져나간 돈.
    [JsonPropertyName("paid")] public int Paid { get; set; }
    [JsonPropertyName("itemsBought")] public int ItemsBought { get; set; }
    // 그 팀원 스태시로 돌아간 헌 장비 수.
    [JsonPropertyName("itemsStashed")] public int ItemsStashed { get; set; }
}
