using System.Text.Json.Serialization;

namespace SptKoreanLocale;

/// <summary>
/// <c>config.jsonc</c> 의 내용. 주석을 받는다(<c>ModHelper.GetJsonDataFromFile</c> 가 JSONC 를 읽는다).
/// </summary>
public class Config
{
    /// <summary>꺼 두면 아무것도 바꾸지 않고 사전만 읽어 셈을 남긴다.</summary>
    [JsonPropertyName("modEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>
    /// 덮어쓸 게임 locale 코드. SPT 의 한국어는 <c>kr</c> 이다(<c>locales/global/kr.json</c>).
    /// 웹 화면의 <c>ko</c> 와 다르다 — 그쪽은 <c>locales/web/</c> 소관이다.
    /// </summary>
    [JsonPropertyName("locale")]
    public string Locale { get; set; } = "kr";

    /// <summary>
    /// 참이면 **이미 있는 열쇠만** 덮어쓴다. 사전에 오타가 있거나 그 모드를 뺀 뒤라면
    /// 조용히 새 열쇠를 만드는 대신 경고로 알린다. 새 열쇠를 일부러 넣어야 할 때만 끈다.
    /// </summary>
    [JsonPropertyName("onlyExistingKeys")]
    public bool OnlyExistingKeys { get; set; } = true;

    /// <summary>
    /// 참이면 이미 한글이 들어 있는 값은 건드리지 않는다.
    /// 거짓(기본)이면 덮어쓰되 몇 개를 덮었는지 로그에 남긴다 — 다른 모드가 먼저 넣은 한글을
    /// 우리가 밀어낸 것이 보이게 하려는 것이다.
    /// </summary>
    [JsonPropertyName("skipAlreadyKorean")]
    public bool SkipAlreadyKorean { get; set; }

    /// <summary>
    /// 참(기본)이면 지금 값에 붙어 있는 <b>꾸밈을 그대로 두고 글자만</b> 바꾼다.
    /// <para>
    /// 아이템 이름·약칭에는 다른 모드가 색 태그(<c>&lt;b&gt;&lt;color=#RRGGBB&gt;…&lt;/color&gt;&lt;/b&gt;</c>)와
    /// ● 같은 표시를 덧댄다. 바닐라 한국어 이름에도 똑같이 붙는다 — 실측:
    /// <c>5447a9cd… Name</c> = <c>&lt;b&gt;&lt;color=#FFFFFF&gt;Colt M4A1 5.56x45 돌격소총  ●&lt;/color&gt;&lt;/b&gt;</c>.
    /// 우리 변환기는 <b>맨 끝에 돌기 때문에</b> 값을 통째로 쓰면 그 꾸밈이 사라져, 사전에 적은
    /// 아이템만 색과 ● 가 없어 인벤토리에서 따로 보인다(2026-11-09 실측).
    /// </para>
    /// <para>
    /// 그래서 겉의 태그와 양끝 표시를 떼어 두고 가운데 글자만 우리 값으로 갈아 끼운 뒤 다시 붙인다.
    /// 사전에 색을 박아 넣는 길도 있지만, 그러면 그 모드가 아이템마다 계산하는 <b>등급 색을 우리가
    /// 고정</b>해 버린다. 꾸밈이 없는 열쇠(퀘스트 이름·목표 등)는 이 설정과 무관하게 그대로 바뀐다.
    /// </para>
    /// </summary>
    [JsonPropertyName("preserveDecoration")]
    public bool PreserveDecoration { get; set; } = true;

    /// <summary>항목 하나하나를 로그에 적는다.</summary>
    [JsonPropertyName("debug")]
    public bool Debug { get; set; }
}

/// <summary>사전 파일 하나(<c>lang/*.json</c>).</summary>
public class LangFile
{
    /// <summary>사람이 읽는 메모. 코드는 쓰지 않는다.</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>locale 열쇠 → 한국어 문구.</summary>
    [JsonPropertyName("entries")]
    public Dictionary<string, string> Entries { get; set; } = [];

    /// <summary>
    /// 생성한 사전이 적어 두는 <b>그때의 영문 원본</b>(열쇠 → 영문). 모드는 쓰지 않는다.
    /// 퀘스트 설명처럼 다른 모드가 붙인 머리말까지 통째로 덮는 사전은 모드 구성이 바뀌면
    /// 값이 낡는다 — <c>tools/check-locale-dict.mjs</c> 가 이것을 지금의 <c>en</c> 과 맞춰
    /// "원본이 바뀜" 으로 알린다.
    /// </summary>
    [JsonPropertyName("sources")]
    public Dictionary<string, string> Sources { get; set; } = [];

    /// <summary>
    /// 일부러 영문으로 두는 열쇠와 그 이유. 코드는 건드리지 않고, 검사 도구가
    /// "빠뜨린 것" 과 "일부러 둔 것" 을 가르는 데 쓴다.
    /// </summary>
    [JsonPropertyName("keptAsIs")]
    public Dictionary<string, string> KeptAsIs { get; set; } = [];

    /// <summary>
    /// 이 사전이 책임지는 열쇠 묶음의 접두. 모드는 쓰지 않고 <c>tools/check-locale-dict.mjs</c> 가
    /// "이 묶음에서 빠뜨린 열쇠" 를 찾는 데 쓴다 — 보기: <c>ScavRole/</c>.
    /// </summary>
    [JsonPropertyName("coveragePrefixes")]
    public List<string> CoveragePrefixes { get; set; } = [];

    /// <summary>
    /// 접두로는 묶을 수 없는 열쇠 묶음을 위한 정규식. 퀘스트 열쇠는 24자리 16진수 id 로 시작해
    /// 접두가 없다 — 보기: <c>^[0-9a-f]{24} name$</c>. 모드는 쓰지 않고 검사 도구가 쓴다.
    /// </summary>
    [JsonPropertyName("coveragePatterns")]
    public List<string> CoveragePatterns { get; set; } = [];
}
