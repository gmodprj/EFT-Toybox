using System.Text.Json.Serialization;

namespace WebConfigBridge;

/// <summary>
/// 서버 모드 All In Weapon: Unslotted 의 <c>config/config.jsonc</c> 를 그대로 옮긴 것.
/// 키 이름은 그 모드가 읽는 이름이어야 하므로 원문을 그대로 쓴다.
///
/// 이 값들은 그 모드가 <c>OnLoadAsync</c> 에서 한 번만 읽는다. 웹 편집기에서 저장하면
/// 파일은 즉시 바뀌지만 적용은 **다음 서버 시작부터**다.
/// </summary>
public class AllinWeaponUnslottedConfig
{
    /// <summary>false 로 두면 그 모드가 데이터베이스를 건드리지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>무기 본체의 부착 칸에 아무 부품이나 넣을 수 있게 한다.</summary>
    [JsonPropertyName("FckWeapons")]
    public bool FckWeapons { get; set; } = true;

    /// <summary>부품의 하위 칸에 아무 부품이나 넣을 수 있게 한다.</summary>
    [JsonPropertyName("FckMods")]
    public bool FckMods { get; set; } = true;

    /// <summary>약실에 아무 탄약이나 올릴 수 있게 한다.</summary>
    [JsonPropertyName("FckChambers")]
    public bool FckChambers { get; set; } = true;

    /// <summary>무기에 아무 탄창이나 넣을 수 있게 한다.</summary>
    [JsonPropertyName("FckCalibers")]
    public bool FckCalibers { get; set; } = true;

    /// <summary>탄창에 아무 탄약이나 채울 수 있게 한다.</summary>
    [JsonPropertyName("FckMagazines")]
    public bool FckMagazines { get; set; } = true;

    /// <summary>
    /// 실험 로직. 맞는 분류만 넣는 대신 모든 분류를 칸 필터에 밀어넣는다.
    /// 원본 설명이 "본인 책임" 이라고 적어둔 항목이라 기본은 꺼짐이다.
    /// </summary>
    [JsonPropertyName("FckALL")]
    public bool FckALL { get; set; }

    /// <summary>부품의 ConflictingItems 필터를 지운다.</summary>
    [JsonPropertyName("RemoveConflictingItems")]
    public bool RemoveConflictingItems { get; set; } = true;

    /// <summary>
    /// 칸을 푸는 다른 방식. 게임이 로딩되지 않을 때 도움이 될 수 있으나
    /// 다른 모드와 이상하게 맞물릴 수 있다고 원본이 적어두었다.
    /// </summary>
    [JsonPropertyName("Experimental")]
    public bool Experimental { get; set; }
}
