using System.Text.Json.Serialization;

namespace HelmetSlotUnion;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 데이터베이스를 건드리지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>칸마다 바뀐 내용을 한 줄씩 기록한다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// mod_equipment 와 mod_equipment_000·001·002… 를 한 묶음으로 본다.
    /// 칸 번호가 헬멧마다 다른 뜻이라(FAST는 001이 헤드셋, 다른 헬멧은 측면 보호대) 기본은 꺼짐이다.
    /// </summary>
    [JsonPropertyName("MergeEquipmentSlots")]
    public bool MergeEquipmentSlots { get; set; }

    /// <summary>건드리지 않을 칸 이름. 대소문자는 가리지 않는다.</summary>
    [JsonPropertyName("ExcludedSlotNames")]
    public List<string> ExcludedSlotNames { get; set; } = [];

    /// <summary>건드리지 않을 헬멧 아이템 id.</summary>
    [JsonPropertyName("ExcludedHelmetIds")]
    public List<string> ExcludedHelmetIds { get; set; } = [];
}
