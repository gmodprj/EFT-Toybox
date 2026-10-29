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
}
