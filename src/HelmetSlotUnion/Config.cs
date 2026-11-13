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
    /// 합집합을 모을 때 **데이터베이스에 없는 id 는 담지 않는다**.
    ///
    /// 2026-11-12 실측: 어느 헬멧 하나에 적혀 있던 죽은 참조가 이 합집합을 타고 헬멧
    /// **174칸**으로 퍼져 있었다. 켜 두면 그런 복사가 생기지 않는다 — 넓히는 일 자체는 그대로다.
    /// 예전 동작으로 돌리려면 false.
    /// </summary>
    [JsonPropertyName("SkipDeadIds")]
    public bool SkipDeadIds { get; set; } = true;

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

    /// <summary>
    /// 총기·총기 파츠끼리 서로를 막는 ConflictingItems 항목을 지운다.
    /// 칸 필터는 건드리지 않는다 — 넓히는 것은 SlotCategoryRelax 같은 다른 모드의 몫이고,
    /// 넓힌 뒤에도 남는 "A와 B는 동시에 장착할 수 없습니다" 를 여기서 푼다.
    /// </summary>
    [JsonPropertyName("RemoveWeaponAttachmentConflicts")]
    public bool RemoveWeaponAttachmentConflicts { get; set; } = true;

    /// <summary>
    /// 위보다 센 선택지. 총기·총기 파츠의 ConflictingItems 를 통째로 비운다.
    /// 탄약처럼 범위 밖을 가리키는 항목까지 사라진다.
    /// </summary>
    [JsonPropertyName("RemoveAllConflictsOnWeaponAttachments")]
    public bool RemoveAllConflictsOnWeaponAttachments { get; set; }

    /// <summary>
    /// 총기 충돌 정리에서 건드리지 않을 분류(부모) id. 어느 쪽에 있든 그 항목은 남는다.
    /// 기본값은 탄창(5448bc234bdc2d3c308b4569)·장전 손잡이(55818a6f4bdc2db9688b456b) 다.
    /// 그 둘이 거는 제한은 모양이 아니라 구경 이야기라(.338 볼트에 7.62 총열) 지우면 안 된다.
    /// </summary>
    [JsonPropertyName("WeaponConflictExcludedCategoryIds")]
    public List<string> WeaponConflictExcludedCategoryIds { get; set; } =
        ["5448bc234bdc2d3c308b4569", "55818a6f4bdc2db9688b456b"];

    /// <summary>건드리지 않을 총기 아이템 id.</summary>
    [JsonPropertyName("ExcludedWeaponIds")]
    public List<string> ExcludedWeaponIds { get; set; } = [];

    /// <summary>
    /// 헬멧·총기 범위에서 걸러지지 않고 남은 ConflictingItems 를 전부 지운다.
    /// 안면 보호구↔헬멧, 헤드셋↔헬멧, 헬멧 쪽에서만 닿는 마운트↔기계식 조준기처럼
    /// 두 범위에 걸쳐 있는 것들이 여기서 풀린다.
    /// WeaponConflictExcludedCategoryIds(탄창·장전 손잡이)는 여기서도 그대로 남는다 — 구경 제한이다.
    /// </summary>
    [JsonPropertyName("RemoveRemainingConflicts")]
    public bool RemoveRemainingConflicts { get; set; } = true;

    /// <summary>건드리지 않을 칸 이름. 대소문자는 가리지 않는다.</summary>
    [JsonPropertyName("ExcludedSlotNames")]
    public List<string> ExcludedSlotNames { get; set; } = [];

    /// <summary>건드리지 않을 헬멧 아이템 id.</summary>
    [JsonPropertyName("ExcludedHelmetIds")]
    public List<string> ExcludedHelmetIds { get; set; } = [];
}
