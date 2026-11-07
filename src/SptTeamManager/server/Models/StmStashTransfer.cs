using System.Collections.Generic;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// 계획 6 B2 의 전선 모양. 라우트 `/stm/members/stash-transfer`.
//
// 방향을 **요청이 적는다**(두 라우트로 가르지 않는다). 한 화면이 좌우로 주고받는 조작이라
// 같은 검사·같은 저널·같은 되돌리기를 타야 하고, 라우트가 둘이면 한쪽만 고쳐지는 자리가 생긴다.
//
// `ItemIds` 는 **스태시 바닥에 놓인 것**만 적는다. 안에 든 것은 서버가 따라 모은다
// (StashTransferPolicy) — 화면이 내용물까지 세어 보내면 둘의 셈이 어긋날 수 있다.
public record StmStashTransferRequest : IRequestData
{
    [JsonPropertyName("aid")] public string? Aid { get; set; }
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }

    /// <summary>참이면 플레이어 → 팀원, 거짓이면 팀원 → 플레이어.</summary>
    [JsonPropertyName("toTeammate")] public bool ToTeammate { get; set; }

    [JsonPropertyName("itemIds")] public List<string>? ItemIds { get; set; }
}

public record StmStashTransferResponse
{
    /// <summary>고른 것의 수(안에 든 것은 빼고).</summary>
    [JsonPropertyName("picked")] public int Picked { get; set; }

    /// <summary>실제로 옮긴 것의 수 — 안에 든 것까지. 양쪽에서 같은 수다(총량 보존).</summary>
    [JsonPropertyName("moved")] public int Moved { get; set; }

    [JsonPropertyName("toTeammate")] public bool ToTeammate { get; set; }
}
