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

    // 끌어다 **놓은 자리**. 그 한 개(`AnchorItemId`)는 이 칸에 그대로 놓고, 나머지는 빈 자리를
    // 찾는다. 없거나 그 자리가 막혀 있으면 전부 빈 자리 찾기로 간다(예전 동작).
    //
    // 왜 필요한가 — 서버가 늘 빈 자리를 찾으면 사용자가 놓은 곳과 다른 데로 가고, 그것이
    // "자동 정렬" 처럼 보인다(2026-11-07 지적). 좌표는 클라이언트가 EFT 자신의 드롭 계산
    // (`GridView.CalculateItemLocation`)으로 읽어 보낸다 — 우리가 다시 셈하지 않는다.
    [JsonPropertyName("anchorItemId")] public string? AnchorItemId { get; set; }
    [JsonPropertyName("anchorGrid")] public string? AnchorGrid { get; set; }
    [JsonPropertyName("anchorX")] public int AnchorX { get; set; } = -1;
    [JsonPropertyName("anchorY")] public int AnchorY { get; set; } = -1;
    [JsonPropertyName("anchorRotation")] public int AnchorRotation { get; set; }
}

public record StmStashTransferResponse
{
    /// <summary>고른 것의 수(안에 든 것은 빼고).</summary>
    [JsonPropertyName("picked")] public int Picked { get; set; }

    /// <summary>실제로 옮긴 것의 수 — 안에 든 것까지. 양쪽에서 같은 수다(총량 보존).</summary>
    [JsonPropertyName("moved")] public int Moved { get; set; }

    [JsonPropertyName("toTeammate")] public bool ToTeammate { get; set; }

    // --- 돌고 있는 게임에 알려 주기 위한 것 (2026-11-13) ---
    //
    // 전송은 **서버 쪽 프로필**을 고친다. 그런데 게임은 자기 메모리에 스태시 사본을 들고 있어,
    // 알려 주지 않으면 옮긴 뒤에도 **보내는 쪽 가방에 그대로 보인다**(2026-11-13 제보 —
    // 디스크는 맞았고 화면만 묵은 것이었다. fixes/2026-11-13-stash-transfer-silent-drops.md).
    // 그래서 창이 `ProfileUpdatesHandler` 로 플레이어 쪽 변경을 밀어 넣을 수 있게 재료를 싣는다.

    /// <summary>옮긴 것 전부(안에 든 것까지)의 id. 플레이어가 **잃는** 쪽일 때 지울 목록이다.</summary>
    [JsonPropertyName("movedIds")] public List<string> MovedIds { get; set; } = [];

    /// <summary>
    /// 플레이어가 **받는** 쪽일 때, 옮겨진 아이템들이 플레이어 스태시에서 갖게 된 최종 모양
    /// (부모·칸·좌표까지). 플레이어가 잃는 쪽이면 비어 있다.
    /// </summary>
    [JsonPropertyName("playerItems")] public List<SPTarkov.Server.Core.Models.Eft.Common.Tables.Item> PlayerItems { get; set; } = [];
}
