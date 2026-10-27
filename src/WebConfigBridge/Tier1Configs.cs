using System.Text.Json.Serialization;

namespace WebConfigBridge;

// 요청·이벤트마다 값을 쓰는 모드들의 설정. 키 이름은 각 모드가 읽는 이름 그대로다.
// 모델로 만들지 않은 키는 TargetConfig 의 확장 데이터가 보존한다.

/// <summary>Dynamic Maps — 레이드 중 지도에 표시할 마커.</summary>
public class DynamicMapsConfig : TargetConfig
{
    [JsonPropertyName("AllowShowFriendlyPlayerMarkersInRaid")]
    public bool AllowShowFriendlyPlayerMarkersInRaid { get; set; } = true;

    [JsonPropertyName("AllowShowEnemyPlayerMarkersInRaid")]
    public bool AllowShowEnemyPlayerMarkersInRaid { get; set; } = true;

    [JsonPropertyName("AllowShowBossMarkersInRaid")]
    public bool AllowShowBossMarkersInRaid { get; set; } = true;

    [JsonPropertyName("AllowShowScavMarkersInRaid")]
    public bool AllowShowScavMarkersInRaid { get; set; } = true;
}

/// <summary>Trade Marker — 상인 화면의 거래 표시와 플리 제한.</summary>
public class TradeMarkerConfig : TargetConfig
{
    [JsonPropertyName("marking")]
    public TradeMarkerSection Marking { get; set; } = new();

    [JsonPropertyName("ragfairRestriction")]
    public TradeMarkerSection RagfairRestriction { get; set; } = new();
}

public class TradeMarkerSection
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("disabledTraderIds")]
    public List<string> DisabledTraderIds { get; set; } = [];
}

/// <summary>Equipment Is Eternal — 사망 시 되돌려줄 것.</summary>
public class EquipmentIsEternalConfig : TargetConfig
{
    [JsonPropertyName("RestoreArmBand")]
    public bool RestoreArmBand { get; set; }

    [JsonPropertyName("RestoreSpecialSlots")]
    public bool RestoreSpecialSlots { get; set; }

    [JsonPropertyName("RestoreScabbard")]
    public bool RestoreScabbard { get; set; }

    [JsonPropertyName("RestoreSecuredContainer")]
    public bool RestoreSecuredContainer { get; set; }

    /// <summary>레이드 전 보험을 든 1단계 장비만 되돌려준다.</summary>
    [JsonPropertyName("OnlyRestoreInsured")]
    public bool OnlyRestoreInsured { get; set; }

    /// <summary>사망해도 레이드 중 주운 퀘스트 아이템을 지킨다.</summary>
    [JsonPropertyName("KeepQuestItems")]
    public bool KeepQuestItems { get; set; }

    /// <summary>복원 후 id 가 겹칠 때 중복을 버린다. 끄면 새 id 를 붙여 둘 다 남긴다.</summary>
    [JsonPropertyName("DeduplicateItems")]
    public bool DeduplicateItems { get; set; } = true;
}

/// <summary>Ultimate Flea — 기능 스위치(mod.json). 가격·경제 세부는 다른 파일에 있다.</summary>
public class UltimateFleaModConfig : TargetConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("enablePoolManager")]
    public bool EnablePoolManager { get; set; } = true;

    [JsonPropertyName("enablePricing")]
    public bool EnablePricing { get; set; } = true;

    [JsonPropertyName("enableEconomy")]
    public bool EnableEconomy { get; set; } = true;

    /// <summary>가격 원천. 원본이 쓰는 값은 "local" 등 문자열이다.</summary>
    [JsonPropertyName("priceSource")]
    public string PriceSource { get; set; } = "local";

    [JsonPropertyName("pvePrices")]
    public bool PvePrices { get; set; }

    [JsonPropertyName("preserveBasePrices")]
    public bool PreserveBasePrices { get; set; } = true;

    [JsonPropertyName("disableTraderPriceFloor")]
    public bool DisableTraderPriceFloor { get; set; }

    [JsonPropertyName("exactOfferPrices")]
    public bool ExactOfferPrices { get; set; }

    [JsonPropertyName("playerOffersOnly")]
    public bool PlayerOffersOnly { get; set; }

    [JsonPropertyName("debug")]
    public bool Debug { get; set; }
}

/// <summary>
/// Realistic Insurance — 되돌아올 확률의 바탕값만 다룬다.
/// 약탈자 능력치·탐욕·가치 가중 같은 묶음은 확장 데이터가 그대로 보존한다.
/// </summary>
public class RealisticInsuranceConfig : TargetConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("baseReturnChancePercent")]
    public BaseReturnChance BaseReturnChancePercent { get; set; } = new();

    /// <summary>능력치를 따지기 전, 죽인 쪽이 살아서 탈출했을 기본 확률.</summary>
    [JsonPropertyName("looterExtractedChancePercent")]
    public double LooterExtractedChancePercent { get; set; } = 65;

    /// <summary>죽인 쪽이 탈출하지 못했을 때 반환 확률에 더해지는 값.</summary>
    [JsonPropertyName("looterDiedBonusPercent")]
    public double LooterDiedBonusPercent { get; set; } = 25;

    [JsonPropertyName("logRolls")]
    public bool LogRolls { get; set; }
}

/// <summary>무엇에게 죽었는지에 따른 기본 반환 확률(%).</summary>
public class BaseReturnChance
{
    [JsonPropertyName("pmc")]
    public double Pmc { get; set; } = 55;

    /// <summary>스캐브로 들어온 실제 플레이어. 잘 털고 잘 빠져나간다.</summary>
    [JsonPropertyName("playerScav")]
    public double PlayerScav { get; set; } = 45;

    /// <summary>AI 스캐브. 조금만 집어가고 자주 죽는다.</summary>
    [JsonPropertyName("scav")]
    public double Scav { get; set; } = 75;

    [JsonPropertyName("boss")]
    public double Boss { get; set; } = 40;

    [JsonPropertyName("other")]
    public double Other { get; set; } = 90;
}

/// <summary>
/// Repair Skill Extension — 버프 확률 계산의 세 값만 다룬다.
/// 원본 파일이 "값만 고치라" 고 적어둔 깊은 트리(repairKit 등)는 확장 데이터가 보존한다.
/// </summary>
public class RepairSkillExtensionConfig : TargetConfig
{
    [JsonPropertyName("BuffSettings")]
    public RepairBuffSettings BuffSettings { get; set; } = new();
}

public class RepairBuffSettings
{
    /// <summary>globals.json 기본값 0.15</summary>
    [JsonPropertyName("CommonBuffMinChanceValue")]
    public double CommonBuffMinChanceValue { get; set; } = 0.25;

    /// <summary>globals.json 기본값 0.005</summary>
    [JsonPropertyName("CommonBuffChanceLevelBonus")]
    public double CommonBuffChanceLevelBonus { get; set; } = 0.015;

    /// <summary>globals.json 기본값 30</summary>
    [JsonPropertyName("ReceivedDurabilityMaxPercent")]
    public double ReceivedDurabilityMaxPercent { get; set; } = 10;
}

/// <summary>
/// Infinite Everything — 프로필 id 마다 토글이 붙는다.
/// 루트가 사전이라 <see cref="TargetConfig"/> 를 쓰지 않는다. 대신 사전이 모든 키를 그대로 들고 간다.
/// </summary>
public class InfiniteEverythingProfile
{
    [JsonPropertyName("money")]
    public bool Money { get; set; }

    [JsonPropertyName("hideout")]
    public bool Hideout { get; set; }
}
