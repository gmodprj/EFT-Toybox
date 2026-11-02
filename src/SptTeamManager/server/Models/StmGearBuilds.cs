using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 계획 4 Task 1 의 전선 모양. 라우트 `/stm/members/builds`.
//
// Aid 와 ProfileId 를 둘 다 받는 것은 스태시 조회(StmGearStashRequest)와 같은 이유다: Aid 로 해석하고,
// ProfileId 는 화면이 보고 있던 팀원과 서버가 해석한 팀원이 같은지 맞춰 보는 데 쓴다.
public record StmGearBuildsRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
}

// 목록 한 줄. 고르면 Id 로 다시 찾는다.
public record StmGearBuildRow
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

// **두 묶음을 각자의 자리에 담는다.** 한 목록에 꼬리표로 섞어 담지 않는다 - 사용자가 요구한
// "별도로 구분하여 표시" 를 자료 구조가 지킨다(GearBuildListPolicy 와 같은 이유).
public record StmGearBuildsResponse
{
    // 그 팀원 프로필에 저장된 장비 빌드.
    [JsonPropertyName("teammateBuilds")] public List<StmGearBuildRow> TeammateBuilds { get; set; } = [];
    // 지금 플레이어 프로필에 저장된 장비 빌드.
    [JsonPropertyName("playerBuilds")] public List<StmGearBuildRow> PlayerBuilds { get; set; } = [];
    // 화면 제목에 쓸 이름. 서버가 해석한 팀원이 누구인지 화면이 되읽을 수 있게 같이 보낸다.
    [JsonPropertyName("teammateNickname")] public string TeammateNickname { get; set; } = string.Empty;
    [JsonPropertyName("profileId")] public string ProfileId { get; set; } = string.Empty;
}
