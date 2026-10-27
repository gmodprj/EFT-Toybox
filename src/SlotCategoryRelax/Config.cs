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
}
