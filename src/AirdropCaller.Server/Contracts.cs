using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// `/airdrop-caller/*` 가 주고받는 모양.
///
/// 이 라우트들은 **우리 플러그인과 외부 창만** 쓴다. 그래서 `/client/*` 가 쓰는
/// `{err, errmsg, data}` 포장을 하지 않고 평범한 JSON 을 그대로 주고받는다 —
/// 외부 창이 읽기 쉬운 쪽을 골랐다.
/// </summary>
public record QueueRequest : IRequestData
{
    /// <summary>
    /// 저장해 둔 프리셋으로 부를 때 그 id. 주면 <see cref="Icon"/>·<see cref="Items"/> 는 무시한다.
    /// </summary>
    [JsonPropertyName("presetId")]
    public string? PresetId { get; set; }

    /// <summary>상자 종류. 비우면 `Common`.</summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>담을 품목. 비어 있으면 거절한다 — 빈 상자를 띄울 이유가 없다.</summary>
    [JsonPropertyName("items")]
    public List<ItemSpec>? Items { get; set; }
}

public record QueueResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>받아들였을 때만 있다. 플러그인이 이 값을 `InitAirdrop` 에 넘긴다.</summary>
    [JsonPropertyName("dropId")]
    public string? DropId { get; set; }

    /// <summary>상자에 실제로 담긴 개수.</summary>
    [JsonPropertyName("placed")]
    public int Placed { get; set; }

    /// <summary>격자에 안 들어가 못 담은 것. **조용히 버리지 않는다.**</summary>
    [JsonPropertyName("skipped")]
    public List<string> Skipped { get; set; } = [];

    /// <summary>거절했을 때 그 이유를 사람이 읽는 문장으로.</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public record StateResponse
{
    /// <summary>게임이 붙어 있나. false 면 플러그인 보고가 끊긴 것이다.</summary>
    [JsonPropertyName("gameConnected")]
    public bool GameConnected { get; set; }

    [JsonPropertyName("inRaid")]
    public bool InRaid { get; set; }

    [JsonPropertyName("map")]
    public string? Map { get; set; }

    /// <summary>이 맵에서 보급을 부를 수 있나.</summary>
    [JsonPropertyName("canSummon")]
    public bool CanSummon { get; set; }

    [JsonPropertyName("crateInFlight")]
    public bool CrateInFlight { get; set; }

    [JsonPropertyName("availablePoints")]
    public int AvailablePoints { get; set; }

    [JsonPropertyName("hasPending")]
    public bool HasPending { get; set; }

    [JsonPropertyName("cooldownSeconds")]
    public int CooldownSeconds { get; set; }

    /// <summary>지금 드롭 버튼을 눌러도 되나. 외부 창은 이 값만 보면 된다.</summary>
    [JsonPropertyName("canQueue")]
    public bool CanQueue { get; set; }

    /// <summary>못 누르는 이유. 누를 수 있으면 null.</summary>
    [JsonPropertyName("blockedBy")]
    public string? BlockedBy { get; set; }
}

public record PendingResponse
{
    /// <summary>꺼내 간 예약. 없으면 null.</summary>
    [JsonPropertyName("dropId")]
    public string? DropId { get; set; }
}

public record SimpleResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
