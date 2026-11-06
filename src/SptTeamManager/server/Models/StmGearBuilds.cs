using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
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
//
// Root·Items 는 **팀원 빌드에만** 싣는다. 게임의 기본 장비 프리셋 화면은 플레이어의 보관소만 알기 때문에,
// 팀원 빌드를 그 화면에 보이려면 클라이언트가 EquipmentBuild 를 직접 만들어야 하고 그러려면 아이템이 필요하다
// (EquipmentBuild 생성자가 (id, name, root, FlatItem[], buildType) 를 받는다 - 역컴파일로 확인).
// 플레이어 빌드는 게임이 이미 갖고 있으므로 싣지 않는다 - 같은 것을 두 번 보내지 않는다.
public record StmGearBuildRow
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    // 이 빌드를 이 팀원에게 입히는 데 드는 값. **용병 쪽 화면이 쓰는 PIT 의 값과 다르다** - 이미 입고 있는
    // 칸은 사지 않으므로 팀원마다 달라진다. -1 은 "값을 매길 수 없다"(상인이 안 파는 물건이 끼어 있다)이고,
    // 그 빌드는 사지지도 않는다. 0 이상이면 구매 창이 청구할 값과 같은 계산에서 나온 값이다.
    [JsonPropertyName("price")] public int Price { get; set; } = -1;
    [JsonPropertyName("root")] public string Root { get; set; } = string.Empty;
    [JsonPropertyName("items")] public List<Item>? Items { get; set; }
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
