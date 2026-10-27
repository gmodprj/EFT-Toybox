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

    /// <summary>
    /// 헬멧·헬멧 파츠끼리 서로를 막는 ConflictingItems 항목을 지운다.
    /// 칸 필터를 넓혀도 이것이 남아 있으면 "A와 B는 동시에 장착할 수 없습니다" 가 뜬다.
    /// 범위 밖(헤드셋·안면 보호구 등)을 가리키는 항목은 그대로 둔다.
    /// </summary>
    [JsonPropertyName("RemoveAttachmentConflicts")]
    public bool RemoveAttachmentConflicts { get; set; } = true;

    /// <summary>
    /// 위보다 센 선택지. 헬멧·헬멧 파츠의 ConflictingItems 를 통째로 비운다.
    /// 범위 밖을 가리키는 항목까지 사라지므로 다른 모드와 부딪힐 수 있다.
    /// </summary>
    [JsonPropertyName("RemoveAllConflictsOnAttachments")]
    public bool RemoveAllConflictsOnAttachments { get; set; }

    /// <summary>건드리지 않을 칸 이름. 대소문자는 가리지 않는다.</summary>
    [JsonPropertyName("ExcludedSlotNames")]
    public List<string> ExcludedSlotNames { get; set; } = [];

    /// <summary>건드리지 않을 헬멧 아이템 id.</summary>
    [JsonPropertyName("ExcludedHelmetIds")]
    public List<string> ExcludedHelmetIds { get; set; } = [];
}
