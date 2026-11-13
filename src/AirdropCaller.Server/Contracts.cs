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

    /// <summary>큰 상자로 보낼지. <see cref="PresetId"/> 를 주면 프리셋에 적힌 값을 쓴다.</summary>
    [JsonPropertyName("bigCrate")]
    public bool BigCrate { get; set; }

    /// <summary>
    /// **마른 조립** — 조립해서 결과만 돌려주고 예약은 하지 않는다. 쿨다운도 쓰지 않는다.
    /// 그래서 게임이 꺼져 있어도, 레이드 밖에서도 담기는지 미리 볼 수 있다.
    /// 경위는 설계 §20.4(시험 삼아 쏜 예약이 18초 뒤 실물이 되어 떨어졌다).
    /// </summary>
    [JsonPropertyName("dryRun")]
    public bool DryRun { get; set; }
}

public record QueueResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>받아들였을 때만 있다. 플러그인이 이 값을 `InitAirdrop` 에 넘긴다.</summary>
    [JsonPropertyName("dropId")]
    public string? DropId { get; set; }

    /// <summary>상자에 실제로 담긴 개수. 품목에 붙은 부속은 세지 않는다.</summary>
    [JsonPropertyName("placed")]
    public int Placed { get; set; }

    /// <summary>
    /// 품목에 **붙은** 것의 수 — 방어판·삽입물·헬멧 부속 따위.
    /// 격자 칸을 쓰지 않으므로 <see cref="Placed"/> 와 따로 센다.
    /// </summary>
    [JsonPropertyName("attached")]
    public int Attached { get; set; }

    /// <summary>격자에 안 들어가 못 담은 것. **조용히 버리지 않는다.**</summary>
    /// <summary>못 담은 것. 상자에 들어가지 않았다.</summary>
    [JsonPropertyName("skipped")]
    public List<string> Skipped { get; set; } = [];

    /// <summary>
    /// 담기는 했는데 적어 낸 대로는 아닌 것(묶음을 깎았다 등).
    /// <see cref="Skipped"/> 와 섞으면 "못 담았다" 고 말해 놓고 상자에는 들어 있게 된다.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public List<string> Adjusted { get; set; } = [];

    /// <summary>거절했을 때 그 이유를 사람이 읽는 문장으로.</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// 상자 **자기 격자**에서 찬 칸 수. 안에 넣은 그릇의 속칸은 세지 않는다 —
    /// 사람이 보고 싶은 것은 "이 상자에 더 들어가나" 다.
    /// </summary>
    [JsonPropertyName("cellsUsed")]
    public int CellsUsed { get; set; }

    /// <summary>상자 자기 격자의 칸 수 전부. 0 이면 칸 수를 모른다는 뜻이다.</summary>
    [JsonPropertyName("cellsTotal")]
    public int CellsTotal { get; set; }

    /// <summary>
    /// 이 답이 **마른 조립**의 결과임. 참이면 <see cref="DropId"/> 는 없고 **아무것도 떨어지지 않는다** —
    /// 창이 "예약했다" 로 잘못 말하지 않게 답에도 적어 돌려준다.
    /// </summary>
    [JsonPropertyName("dryRun")]
    public bool DryRun { get; set; }
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

/// <summary>
/// 상태 보고에 대한 답.
///
/// 예약을 **여기에 실어 보낸다.** 전에는 플러그인이 1초마다 `/pending` 을 따로
/// 물어봤는데, 보고를 이미 주고받는 자리가 있는데 길을 하나 더 쓸 이유가 없었다.
/// </summary>
public record HeartbeatResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; } = true;

    /// <summary>꺼내 간 예약. 없으면 null.</summary>
    [JsonPropertyName("dropId")]
    public string? DropId { get; set; }

    /// <summary>
    /// 다음 보고까지 쉴 시간. **서버가 정한다** — 창이 꺼져 있으면 아무도 안 읽는
    /// 보고라 천천히 받는다. 창이 다시 열리면 바로 빨라진다.
    /// </summary>
    [JsonPropertyName("nextSeconds")]
    public double NextSeconds { get; set; }
}

public record SimpleResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
