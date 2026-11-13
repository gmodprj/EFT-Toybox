using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 2026-11-13 사용자 요청 — 편집기에서 **지금 입고 있는 것을 꾸러미(장비 빌드)로 담는다.**
// 라우트 `/stm/members/build-save`.
//
// Aid 와 ProfileId 를 둘 다 받는 것은 빌드 목록·스태시 조회와 같은 이유다: Aid 로 해석하고,
// ProfileId 는 화면이 보고 있던 팀원과 서버가 해석한 팀원이 같은지 맞춰 보는 데 쓴다.
//
// **장비를 싣지 않는다.** 서버가 그 팀원 프로필에 **이미 저장돼 있는** 장비를 읽어 담는다 —
// 화면이 보낸 트리를 믿고 담으면, 편집기에서 저장하지 않은 것까지 꾸러미에 들어가 화면과
// 프로필이 어긋난다. 창은 저장을 먼저 끝낸 뒤 이 라우트를 부른다(사용자 결정, 2026-11-13).
public record StmGearBuildSaveRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
}

public record StmGearBuildSaveResponse
{
    /// <summary>만들어진 빌드의 id.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    /// <summary>붙인 이름. 자동으로 짓는다 — `Sarge 11-13 15:30`.</summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>담은 것의 수(장비 뿌리는 세지 않는다).</summary>
    [JsonPropertyName("itemCount")] public int ItemCount { get; set; }
}
