using System.Text.Json.Serialization;

namespace WebConfigBridge;

/// <summary>
/// 서버 모드 HelmetSlotUnion 의 <c>config.jsonc</c> 를 그대로 옮긴 것.
/// 키 이름은 그 모드가 읽는 이름이어야 하므로 원문을 그대로 쓴다.
///
/// 그 모드도 설정을 서버 시작 때 한 번만 읽는다. 저장은 즉시, 적용은 다음 서버 시작부터.
/// </summary>
public class HelmetSlotUnionConfig : TargetConfig
{
    /// <summary>false 면 데이터베이스를 건드리지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>칸마다 바뀐 내용을 한 줄씩 기록한다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// mod_equipment 와 mod_equipment_000·001·002… 를 한 묶음으로 본다.
    /// 칸 번호의 뜻이 헬멧마다 달라 기본은 꺼짐이다.
    /// </summary>
    [JsonPropertyName("MergeEquipmentSlots")]
    public bool MergeEquipmentSlots { get; set; }

    /// <summary>헬멧·헬멧 파츠끼리 서로를 막는 ConflictingItems 항목을 지운다.</summary>
    [JsonPropertyName("RemoveAttachmentConflicts")]
    public bool RemoveAttachmentConflicts { get; set; } = true;

    /// <summary>헬멧·헬멧 파츠의 ConflictingItems 를 통째로 비운다. 범위 밖 항목까지 사라진다.</summary>
    [JsonPropertyName("RemoveAllConflictsOnAttachments")]
    public bool RemoveAllConflictsOnAttachments { get; set; }

    /// <summary>건드리지 않을 칸 이름. 대소문자는 가리지 않는다.</summary>
    [JsonPropertyName("ExcludedSlotNames")]
    public List<string> ExcludedSlotNames { get; set; } = [];

    /// <summary>건드리지 않을 헬멧 아이템 id.</summary>
    [JsonPropertyName("ExcludedHelmetIds")]
    public List<string> ExcludedHelmetIds { get; set; } = [];
}
