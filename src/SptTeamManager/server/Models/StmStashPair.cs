using System.Collections.Generic;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 계획 6 B3 의 조회. 라우트 `/stm/members/stash-pair`.
//
// 화면이 좌우 두 칸을 그리려면 양쪽 스태시의 **바닥에 놓인 것**과 그 이름이 필요하다. 이름을
// 서버가 붙여 보내는 이유는 하나다 — 클라이언트에서 아이템 이름을 얻으려면 EFT 쪽 API 를 더듬어야
// 하는데, 서버에는 이미 `LocaleService` 로 꺼내는 자리가 있다(꾸러미 견적이 쓰는 것과 같은 자리).
// 화면이 얇아지고, 틀릴 수 있는 자리가 하나 줄어든다.
//
// 바닥에 놓인 것만 싣는다. 안에 든 것은 **수만** 적는다(`Contained`) — 전송의 단위가 바닥에 놓인
// 것이기 때문이고(StashTransferPolicy), 내용물까지 실으면 응답이 수천 줄이 된다.
public record StmStashPairRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
}

public record StmStashEntryWire
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>쌓인 개수(없으면 1).</summary>
    [JsonPropertyName("stack")] public int Stack { get; set; } = 1;

    /// <summary>이 아이템 **안에** 든 것의 수. 같이 건너간다.</summary>
    [JsonPropertyName("contained")] public int Contained { get; set; }
}

public record StmStashSideWire
{
    [JsonPropertyName("profileId")] public string ProfileId { get; set; } = string.Empty;
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = string.Empty;
    [JsonPropertyName("stashRootId")] public string StashRootId { get; set; } = string.Empty;

    /// <summary>바닥에 놓인 것의 요약(이름·쌓인 수·안에 든 것의 수). 목록으로 그릴 때 쓴다.</summary>
    [JsonPropertyName("items")] public List<StmStashEntryWire> Items { get; set; } = [];

    /// <summary>
    /// **스태시 트리 전체**(루트 포함). 화면이 인게임 스태시와 같은 격자를 세우려면 이것이 필요하다 —
    /// `ItemFactory.FlatItemsToTree` 가 받는 그 모양이고, 장비 편집기가 팀원 스태시를 세울 때 쓰는
    /// 것과 같은 자료다(`/stm/members/gear-stash`). 요약(`Items`)과 **같은 스태시를 두 모양으로**
    /// 싣는 것이라 둘이 어긋날 일은 없다 — 같은 한 번의 읽기에서 만든다.
    /// </summary>
    [JsonPropertyName("tree")] public List<Item> Tree { get; set; } = [];
}

public record StmStashPairResponse
{
    [JsonPropertyName("player")] public StmStashSideWire Player { get; set; } = new();
    [JsonPropertyName("teammate")] public StmStashSideWire Teammate { get; set; } = new();
}
