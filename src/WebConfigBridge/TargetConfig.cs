using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebConfigBridge;

/// <summary>
/// 남의 설정 파일을 비추는 POCO 의 바탕.
///
/// 우리가 모델로 만들지 않은 키를 여기에 담아 두었다가 저장할 때 그대로 돌려준다.
/// 이것이 없으면 모르는 키가 저장 때 사라진다 — 어떤 모드는 <c>"// 항목": "설명"</c> 형태의
/// 주석용 키를 쓰고(RealisticInsurance), 어떤 모드는 우리가 다루지 않는 깊은 트리를
/// 들고 있다(RepairSkillExtension). 둘 다 남의 데이터이므로 건드리지 않고 지나보낸다.
///
/// 다만 JSON 이 아닌 <c>//</c> 줄 주석은 이 방법으로도 살릴 수 없다. 그래서 처음 저장할 때
/// 원본을 <c>.orig</c> 로 남긴다.
/// </summary>
public abstract class TargetConfig
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; } = new();
}
