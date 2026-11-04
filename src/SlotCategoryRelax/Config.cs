using System.Text.Json.Serialization;

namespace SlotCategoryRelax;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 데이터베이스를 건드리지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>칸마다 몇 개가 늘었는지 한 줄씩 기록한다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// 분류별 on/off. 켠 분류만 칸 필터를 넓힌다.
    ///
    /// 키는 아이템 표의 부모 노드 이름이다(대소문자 가리지 않음). 여기에 없는 분류는 꺼진 것으로 본다.
    /// 어떤 분류가 있는지는 모드가 서버 시작 때 <c>discovered-categories.json</c> 로 내놓는다 —
    /// 설치본에 실제로 있는 것만 들어가므로 다른 모드가 분류를 더하면 그것도 나타난다.
    /// </summary>
    [JsonPropertyName("Categories")]
    public Dictionary<string, bool> Categories { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>건드리지 않을 칸 이름. 대소문자는 가리지 않는다.</summary>
    [JsonPropertyName("ExcludedSlotNames")]
    public List<string> ExcludedSlotNames { get; set; } = [];

    /// <summary>
    /// 부품에 달린 하위 칸까지 넓힐지. 끄면 무기 본체의 칸만 넓힌다.
    /// 마운트에 조준경을 다는 식의 조합은 이것이 켜져 있어야 열린다.
    /// </summary>
    [JsonPropertyName("IncludeAttachmentSlots")]
    public bool IncludeAttachmentSlots { get; set; } = true;

    /// <summary>
    /// 약실(Chambers)이 받는 탄을 넓힌다 — All In Weapon 의 FckChambers 자리다.
    /// 약실이 이미 받고 있는 분류의 탄을 전부 넣는다(거의 다 Ammo). 탄창은 건드리지 않는다.
    /// </summary>
    [JsonPropertyName("RelaxChambers")]
    public bool RelaxChambers { get; set; } = true;

    /// <summary>
    /// 약실을 넓히지 않을 총기 아이템 id. 유탄발사기·신호권총처럼 빼 두고 싶은 것을 적는다.
    /// </summary>
    [JsonPropertyName("ExcludedChamberWeaponIds")]
    public List<string> ExcludedChamberWeaponIds { get; set; } = [];

    /// <summary>
    /// 약실을 넓히지 않을 구경(<c>ammoCaliber</c>). 아이템 id 보다 오래 간다 — 모드가 더한 총도 걸린다.
    /// 기본값은 유탄발사기 셋(40x46·40mmRU·30x29)과 신호권총(26x75)이다. 소총탄이 들어갈 자리가 아니다.
    /// </summary>
    [JsonPropertyName("ExcludedChamberCalibers")]
    public List<string> ExcludedChamberCalibers { get; set; } =
        ["Caliber40x46", "Caliber40mmRU", "Caliber30x29", "Caliber26x75"];
}
