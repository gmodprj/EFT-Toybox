using System.Text.Json.Serialization;

namespace ScavRaidWaves;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 설정을 건드리지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>맵마다 무엇을 어떻게 바꿨는지 한 줄씩 기록한다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// 바꾸지 않고 "바꿀 것"만 기록한다. 다른 모드가 지금 어떤 값을 넣어 뒀는지
    /// 먼저 보고 싶을 때 쓴다.
    /// </summary>
    [JsonPropertyName("ReportOnly")]
    public bool ReportOnly { get; set; }

    /// <summary>
    /// 모든 맵에 적용할 기본값. 맵별 설정이 있으면 그쪽이 이긴다.
    /// 둘 다 null 이면 그 항목은 건드리지 않는다.
    /// </summary>
    [JsonPropertyName("Default")]
    public MapSettings Default { get; set; } = new();

    /// <summary>
    /// 맵 id → 설정. id 는 SPT 가 쓰는 이름 그대로다 —
    /// `bigmap`(커스텀), `interchange`, `woods`, `shoreline`, `rezervbase`,
    /// `lighthouse`, `tarkovstreets`, `laboratory`, `sandbox`, `sandbox_high`,
    /// `factory4_day`, `factory4_night`, `labyrinth`.
    /// </summary>
    [JsonPropertyName("Maps")]
    public Dictionary<string, MapSettings> Maps { get; set; } = [];
}

/// <summary>
/// `location.json` 의 `scavRaidTimeSettings.maps.&lt;맵&gt;` 중 PMC 스폰에 걸리는 두 값.
/// null 은 "건드리지 않는다"는 뜻이다.
/// </summary>
public class MapSettings
{
    /// <summary>
    /// 스캐브 레이드의 시간을 깎을 확률(%). 바닐라는 75~100 이다.
    /// <b>0 으로 두면 시간을 깎지 않으므로 PMC 웨이브가 전부 산다.</b>
    /// 대신 스캐브 레이드가 정규 레이드와 같은 길이가 된다.
    /// </summary>
    [JsonPropertyName("ReducedChancePercent")]
    public double? ReducedChancePercent { get; set; }

    /// <summary>
    /// 시간을 깎을 때 봇 웨이브도 같이 조정할지. 바닐라는 true 다.
    /// false 로 두면 레이드는 짧아지되 웨이브 시각표는 원래대로 남는다 —
    /// 짧아진 길이 안에 드는 웨이브만 나오므로 PMC 가 일부 돌아온다.
    /// </summary>
    [JsonPropertyName("AdjustWaves")]
    public bool? AdjustWaves { get; set; }
}
