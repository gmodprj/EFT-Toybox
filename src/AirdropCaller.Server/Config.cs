using System.Text.Json.Serialization;

namespace AirdropCaller.Server;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 모든 요청을 바닐라에 그대로 넘긴다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>요청마다 어느 상자였고 무엇을 담았는지 적는다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; } = true;

    /// <summary>
    /// 상자가 착지한 뒤 다음 예약을 받기까지 기다리는 시간(초).
    /// 연타로 비행기를 여러 대 띄우는 것을 막는다. 0 이면 끈다.
    /// </summary>
    [JsonPropertyName("CooldownSeconds")]
    public int CooldownSeconds { get; set; } = 30;

    /// <summary>
    /// 창이 열려 있을 때 플러그인이 상태를 보고하는 주기(초).
    /// 이 주기가 곧 "떨어뜨리기를 눌렀을 때 상자가 뜨기까지" 의 최대 지연이다.
    /// </summary>
    [JsonPropertyName("FastHeartbeatSeconds")]
    public double FastHeartbeatSeconds { get; set; } = 2;

    /// <summary>
    /// 창이 꺼져 있을 때의 주기(초). 아무도 안 읽는 보고라 천천히 받는다.
    /// 창을 켜면 늦어도 이만큼 뒤에 다시 빨라진다.
    ///
    /// **<see cref="RaidState.StaleAfter"/>(30초)보다 넉넉히 짧아야 한다.** 비슷하면
    /// 게임이 돌아가는데도 창이 "게임 안 켜짐" 으로 깜빡인다.
    /// </summary>
    [JsonPropertyName("IdleHeartbeatSeconds")]
    public double IdleHeartbeatSeconds { get; set; } = 10;

    /// <summary>
    /// 큰 상자의 격자. 바닐라 상자 넷은 전부 10×12=120칸이고, 이것은 그것을 베껴
    /// 키운 **따로 만든 상자**다(<see cref="BigCrate"/>). 바닐라 상자는 안 건드린다.
    /// </summary>
    [JsonPropertyName("BigCrateWidth")]
    public int BigCrateWidth { get; set; } = 30;

    /// <inheritdoc cref="BigCrateWidth"/>
    [JsonPropertyName("BigCrateHeight")]
    public int BigCrateHeight { get; set; } = 30;

    /// <summary>
    /// 상자 id → 그 상자에 담을 것.
    ///
    /// 이 id 는 게임이 `InitAirdrop(lootTemplateId, ...)` 에 넘긴 값이 그대로 돌아온 것이다
    /// (`POST /client/airdrop/loot` 의 `containerId`). 여기에 없는 id 는 건드리지 않고
    /// 바닐라가 만든 답을 그대로 흘려보낸다.
    ///
    /// 2단계에서는 손으로 적어 둔 것만 쓴다. 3단계에서 외부 창이 이 자리를 채운다.
    /// </summary>
    [JsonPropertyName("Drops")]
    public Dictionary<string, DropSpec> Drops { get; set; } = [];
}

/// <summary>상자 하나에 담을 것.</summary>
public class DropSpec
{
    /// <summary>
    /// 상자 종류. `Common` · `Supply` · `Medical` · `Weapon` 중 하나.
    /// 겉모습과 상자 안 격자 크기가 이걸로 정해진다.
    /// </summary>
    [JsonPropertyName("Icon")]
    public string Icon { get; set; } = "Common";

    /// <summary>담을 품목. 격자에 안 들어가는 것은 넣지 않고 기록에 남긴다.</summary>
    [JsonPropertyName("Items")]
    public List<ItemSpec> Items { get; set; } = [];

    /// <summary>
    /// 큰 상자로 보낼지. 겉모습과 창 제목은 <see cref="Icon"/> 이 그대로 정하고,
    /// 안 격자만 커진다(기본 30×30=900칸).
    /// </summary>
    [JsonPropertyName("BigCrate")]
    public bool BigCrate { get; set; }
}

/// <summary>품목 하나.</summary>
public class ItemSpec
{
    /// <summary>아이템 템플릿 id (24자리 hex).</summary>
    [JsonPropertyName("Tpl")]
    public string Tpl { get; set; } = string.Empty;

    /// <summary>몇 개를 따로 넣을지. 격자 칸을 그만큼 쓴다.</summary>
    [JsonPropertyName("Count")]
    public int Count { get; set; } = 1;

    /// <summary>
    /// 한 칸에 몇 개로 묶을지(탄약·돈 등). 비우면 1.
    /// 아이템이 허용하는 최대를 넘으면 게임 쪽에서 어떻게 되는지는 아직 안 봤다.
    /// </summary>
    [JsonPropertyName("StackSize")]
    public int? StackSize { get; set; }

    /// <summary>
    /// **이 품목 안에** 넣을 것. 이 품목이 격자를 가진 컨테이너여야 한다
    /// (가방·케이스·파우치 등). 아니면 무시하고 기록에 남긴다.
    ///
    /// 두 가지를 한 구조로 푼다 — 보급 상자가 10×12=120칸으로 고정이라 더 담으려면
    /// 안에 컨테이너를 넣어야 하고, 무기를 케이스에 담을지 말지도 이걸로 고른다.
    /// </summary>
    [JsonPropertyName("Items")]
    public List<ItemSpec>? Items { get; set; }

    /// <summary>
    /// 이 품목에 딸린 칸을 어디까지 채울지. 비우면 <see cref="SlotFill.Auto"/>.
    ///
    /// `Auto` 는 방어구(헤드기어·바디아머·택티컬 리그)에만 손대어 삽입물과 방어판을
    /// 채운다 — 삽입물이 빠진 방어구는 보호력이 0 이라 적어 낸 사람이 바란 물건이
    /// 아니다. `All` 은 헬멧의 야시경·마운트까지 모두 붙이고, `None` 은 빈 몸으로 둔다.
    /// </summary>
    [JsonPropertyName("Fill")]
    [JsonConverter(typeof(JsonStringEnumConverter<SlotFill>))]
    public SlotFill? Fill { get; set; }

    /// <summary>
    /// 칸마다 직접 고른 것. 칸 경로 → tpl.
    ///
    /// <list type="bullet">
    /// <item>적은 칸은 그대로 쓴다. <see cref="Fill"/> 보다 세다.</item>
    /// <item>안 적은 칸은 <see cref="Fill"/> 이 하던 대로.</item>
    /// <item>빈 문자열은 **"비워 둬라"** — `Auto` 가 채우는 것을 막는 유일한 방법이다.</item>
    /// </list>
    ///
    /// 겹친 칸은 점으로 잇는다: `"mod_mount.mod_nvg"`. 사전 하나로 몇 겹이든 적을 수
    /// 있어 모양이 늘지 않는다.
    ///
    /// 칸에 못 다는 것을 적으면 거절하고 "고친 것" 에 적는다. 조용히 버리지 않는다.
    /// </summary>
    [JsonPropertyName("Slots")]
    public Dictionary<string, string>? Slots { get; set; }
}
