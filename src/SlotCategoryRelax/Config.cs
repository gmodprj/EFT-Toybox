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

    /// <summary>
    /// 맨 끝(<c>int.MaxValue</c>)에서 칸 넓히기를 **한 번 더** 한다.
    ///
    /// 왜 필요한가 — 2026-11-07 실측. ECOTI(COTI)의 <c>ThermalRiserCompatibility</c> 는
    /// <c>PostLoad</c> **뒤**에 돌면서 열화상 라이저의 조준경 목록을 자기 것으로 **갈아 끼운다**.
    /// 넓힌 것을 더하는 게 아니라 통째로 다시 써서, 우리가 넣은 분류가 사라진다:
    ///
    ///   라이저 원본 65개(Collimator 21 · AssaultScope 8 · SpecialScope 3 …)
    ///     → PostLoad 에서 우리가 넓힘
    ///     → COTI 가 89개로 다시 씀(CompactCollimator 23 · OpticScope 50 · Mount 16, **Collimator 0**)
    ///
    /// 그래서 FALKE LE 같은 <c>Collimator</c> 조준경이 "아이템이 호환되지 않습니다" 로 막혔다.
    /// 맨 끝에서 한 번 더 돌면 COTI 가 다시 쓴 뒤의 목록을 기준으로 넓힌다.
    ///
    /// 주의 — 이 자리는 **COTI 가 고른 목록 밖의 조준경까지 올라가게 한다.** 끼워지기는 해도
    /// 열화상이 안 나올 수 있다(COTI 가 들고 있는 메시가 10개다). 모양만 올라간 도트가 된다.
    /// </summary>
    [JsonPropertyName("LateSweep")]
    public bool LateSweep { get; set; } = true;

    /// <summary>
    /// 아이템별 **씨앗 분류**. 그 아이템의 칸에 이 분류들을 "이미 받고 있는 것" 으로 쳐 준다.
    ///
    /// 왜 필요한가 — 2026-11-07 실측. 이 모드의 규칙은 "그 칸이 **이미 받고 있는** 분류만 넓힌다"
    /// 다(없던 분류를 지어 넣지 않는다). 그런데 ECOTI 의 <c>ThermalRiserCompatibility</c> 가
    /// 우리보다 뒤에서 열화상 라이저의 목록을 **갈아 끼우면서** <c>Collimator</c> 를 0개로
    /// 만든다. 씨앗이 없으니 늦은 쓸기를 더해도 그 칸만은 넓힐 수 없었다 — 순서가 아니라
    /// 규칙이 막는 자리다.
    ///
    /// 그래서 "이 아이템의 칸에는 이 분류도 받는 것으로 친다" 를 사람이 적는다. 적은 분류가
    /// <see cref="Categories"/> 에서 꺼져 있으면 아무 일도 하지 않는다(스위치가 상위다).
    ///
    /// 보기 — 열화상 라이저 2색에 반사식 조준경·돌격형 조준경·특수 조준경을 되돌린다:
    /// <code>
    /// "SeedCategories": {
    ///   "6ad3016bab020957abb6ca3b": ["Collimator", "AssaultScope", "SpecialScope"],
    ///   "6ad408a96bf1f761d970a4ff": ["Collimator", "AssaultScope", "SpecialScope"]
    /// }
    /// </code>
    /// </summary>
    [JsonPropertyName("SeedCategories")]
    public Dictionary<string, List<string>> SeedCategories { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

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
