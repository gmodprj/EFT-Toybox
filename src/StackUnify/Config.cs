using System.Text.Json.Serialization;

namespace StackUnify;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 데이터베이스를 건드리지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>바꾼 항목을 한 줄씩 기록한다. 수백 줄이 될 수 있다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// 바꾸지 않고 "바꿀 것"만 기록한다. 다른 모드가 지금 어떤 값을 넣어 뒀는지
    /// 먼저 보고 싶을 때 쓴다.
    /// </summary>
    [JsonPropertyName("ReportOnly")]
    public bool ReportOnly { get; set; }

    /// <summary>
    /// 내가 정한 값과 이미 들어 있던 값이 다른 항목이 몇 개인지 요약에 적는다.
    /// 그 수가 곧 "다른 모드가 건드려 둔 항목 수"다.
    /// </summary>
    [JsonPropertyName("ReportOverridden")]
    public bool ReportOverridden { get; set; } = true;

    /// <summary>
    /// 구경 이름 → 최대 스택. 탄약(`Caliber` 가 있는 아이템)에 적용한다.
    /// 이름은 게임 값 그대로다 — `Caliber9x19PARA`, `Caliber556x45NATO`, `Caliber12g` …
    /// **비어 있으면 아무것도 바꾸지 않는다.**
    /// </summary>
    [JsonPropertyName("StackByCaliber")]
    public Dictionary<string, int> StackByCaliber { get; set; } = [];

    /// <summary>
    /// 아이템 id → 최대 스택. 구경 규칙보다 우선한다. 탄약이 아닌 것에도 쓸 수 있다.
    /// </summary>
    [JsonPropertyName("StackByItemId")]
    public Dictionary<string, int> StackByItemId { get; set; } = [];

    /// <summary>건드리지 않을 아이템 id. 구경 규칙에 걸려도 넘어간다.</summary>
    [JsonPropertyName("ExcludedItemIds")]
    public List<string> ExcludedItemIds { get; set; } = [];
}
