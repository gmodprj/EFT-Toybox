using System.Text.Json.Serialization;

namespace PrestigeStashRelax;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 표를 읽지도 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>
    /// **기본값은 false — 재기만 하고 고치지 않는다.**
    ///
    /// 프레스티지는 되돌릴 수 없는 일이고, 넘어간 물건은 우편으로 온다. 먼저 현재 값이 무엇인지
    /// 보고(<c>Report</c>), 그 숫자를 납득한 뒤에 켜는 순서가 맞다.
    /// </summary>
    [JsonPropertyName("Apply")]
    public bool Apply { get; set; }

    /// <summary>
    /// 단계별 현재 격자·필터를 DLL 옆 <c>prestige-stash-report.json</c> 으로 내놓는다.
    /// 로그를 긁지 않고 숫자를 보려는 자리다. 적용 전/후를 모두 담는다.
    /// </summary>
    [JsonPropertyName("Report")]
    public bool Report { get; set; } = true;

    // ── 크기 ──────────────────────────────────────────────────────────────

    /// <summary>
    /// 가로 칸 수. <c>0</c> 이면 원본 그대로(바닐라는 여섯 단계 모두 8 이다).
    /// 화면이 8 보다 넓은 격자를 그릴 수 있는지는 **아직 안 재 봤다**(설계 §6-2).
    /// </summary>
    [JsonPropertyName("XCellCount")]
    public int XCellCount { get; set; }

    /// <summary>
    /// 원본 세로에 **더할** 칸 수. 단계차(P1 3 … P6 8)를 그대로 지키면서 전체를 늘린다.
    /// </summary>
    [JsonPropertyName("YCellAdd")]
    public int YCellAdd { get; set; }

    /// <summary>
    /// 세로 칸 수를 못 박는다. <c>0</c> 이면 안 쓴다. <c>YCellAdd</c> 보다 **뒤에** 적용되므로
    /// 둘을 함께 적으면 이쪽이 이긴다.
    /// </summary>
    [JsonPropertyName("YCellCount")]
    public int YCellCount { get; set; }

    // ── 품목 ──────────────────────────────────────────────────────────────

    /// <summary>배낭과 가방·케이스를 허용한다. **안에 든 것이 함께 넘어간다.**</summary>
    [JsonPropertyName("AllowBags")]
    public bool AllowBags { get; set; }

    /// <summary>전술 조끼를 허용한다. 조끼도 안에 든 것이 함께 넘어간다.</summary>
    [JsonPropertyName("AllowVest")]
    public bool AllowVest { get; set; }

    /// <summary>보안 컨테이너를 허용한다.</summary>
    [JsonPropertyName("AllowSecureContainer")]
    public bool AllowSecureContainer { get; set; }

    /// <summary>돈을 허용한다.</summary>
    [JsonPropertyName("AllowMoney")]
    public bool AllowMoney { get; set; }

    /// <summary>탄·탄박스·탄창을 허용한다.</summary>
    [JsonPropertyName("AllowAmmo")]
    public bool AllowAmmo { get; set; }

    /// <summary>열쇠·카드를 허용한다.</summary>
    [JsonPropertyName("AllowKeys")]
    public bool AllowKeys { get; set; }

    /// <summary>무기·부품을 **P1·P2 에도** 허용한다. P3 이상에는 이미 들어 있다.</summary>
    [JsonPropertyName("AllowWeaponsEverywhere")]
    public bool AllowWeaponsEverywhere { get; set; }

    /// <summary>방어 장비를 **P1·P2 에도** 허용한다. P3 이상에는 이미 들어 있다.</summary>
    [JsonPropertyName("AllowArmorEverywhere")]
    public bool AllowArmorEverywhere { get; set; }

    /// <summary>
    /// 분류 사슬의 뿌리(<c>Item</c>) 하나를 넣는다. 클라이언트가 부모를 거슬러 맞춘다면
    /// 이것으로 전부 열린다 — **통하는지는 측정 대상이다**(설계 §6-1).
    /// </summary>
    [JsonPropertyName("AllowEverything")]
    public bool AllowEverything { get; set; }

    /// <summary>직접 적는 분류·아이템 id. 위 스위치로 모자랄 때.</summary>
    [JsonPropertyName("ExtraIds")]
    public List<string> ExtraIds { get; set; } = [];

    /// <summary>
    /// **측정용.** 원본 필터를 버리고 위에서 켠 것만 남긴다.
    ///
    /// 이렇게 해야 "배낭만 넣었을 때 배낭이 실제로 끌리는가"를 가릴 수 있다. 원본 186개가
    /// 함께 있으면 무엇 덕분에 끌렸는지 알 수 없다. 평소에는 <c>false</c> 로 둔다.
    /// </summary>
    [JsonPropertyName("ReplaceFilter")]
    public bool ReplaceFilter { get; set; }

    /// <summary>
    /// 우리가 더한 **분류 id 를 그 아래 아이템 id 전부로 펼쳐서도** 적는다.
    ///
    /// 2026-11-15 실측에서 `Backpack` 분류만으로는 배낭이 거부됐다
    /// (`UI/Trader/TransferLocked`). 원인이 클라이언트의 분류 판정인지 모드가 넣은 배낭의
    /// 사슬인지는 아직 못 가렸고, **펼치기는 둘 다를 덮는다** — 서버의 살아 있는 아이템 표에는
    /// 모드 아이템까지 들어 있다. 분류 id 자체도 함께 남긴다(손해가 없다).
    /// </summary>
    [JsonPropertyName("ExpandCategories")]
    public bool ExpandCategories { get; set; } = true;

    /// <summary>
    /// **확인용.** 여기 적은 아이템 id 가 적용 뒤 각 단계의 허용 목록에 들어갔는지 보고서에 적는다.
    ///
    /// 왜 — 화면만으로는 못 가린다. 전송 화면은 필터에 걸린 것과 "내용물이 든 가방"에 걸린 것을
    /// 둘 다 빨간색으로 보여 주고, 말풍선을 봐야 구별된다. 게임을 띄우지 않고 **필터만** 재려는
    /// 자리다.
    /// </summary>
    [JsonPropertyName("ProbeIds")]
    public List<string> ProbeIds { get; set; } = [];

    /// <summary>단계마다 한 줄씩 로그에 적는다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }
}
