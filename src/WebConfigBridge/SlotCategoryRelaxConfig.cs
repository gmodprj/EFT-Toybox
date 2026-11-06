using System.Text.Json.Serialization;

namespace WebConfigBridge;

/// <summary>
/// 서버 모드 SlotCategoryRelax 의 <c>config.jsonc</c> 를 그대로 옮긴 것.
/// 고를 수 있는 분류 목록은 설정이 아니라 그 모드가 내놓는
/// <c>discovered-categories.json</c> 에서 온다.
/// </summary>
public class SlotCategoryRelaxConfig : TargetConfig
{
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>분류 이름 → 켤지. 여기에 없는 분류는 꺼진 것으로 본다.</summary>
    [JsonPropertyName("Categories")]
    public Dictionary<string, bool> Categories { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("ExcludedSlotNames")]
    public List<string> ExcludedSlotNames { get; set; } = [];

    [JsonPropertyName("IncludeAttachmentSlots")]
    public bool IncludeAttachmentSlots { get; set; } = true;

    /// <summary>약실(Chambers)이 받는 탄을 넓힌다. 탄창은 건드리지 않는다.</summary>
    [JsonPropertyName("RelaxChambers")]
    public bool RelaxChambers { get; set; } = true;

    /// <summary>약실을 넓히지 않을 총기 아이템 id.</summary>
    [JsonPropertyName("ExcludedChamberWeaponIds")]
    public List<string> ExcludedChamberWeaponIds { get; set; } = [];

    /// <summary>약실을 넓히지 않을 구경. 기본값은 유탄발사기 셋과 신호권총이다.</summary>
    [JsonPropertyName("ExcludedChamberCalibers")]
    public List<string> ExcludedChamberCalibers { get; set; } =
        ["Caliber40x46", "Caliber40mmRU", "Caliber30x29", "Caliber26x75"];
}

/// <summary>SlotCategoryRelax 가 내놓는 분류 목록의 한 줄.</summary>
public class DiscoveredCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>그 분류에 속한 아이템 수. 고를 때 참고가 된다.</summary>
    [JsonPropertyName("items")]
    public int Items { get; set; }
}
