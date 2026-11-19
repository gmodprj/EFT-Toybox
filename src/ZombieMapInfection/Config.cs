using System.Text.Json.Serialization;

namespace ZombieMapInfection;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 아무것도 바꾸지 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>
    /// 맵 열쇠 → 감염률(%). 열쇠는 <c>seasonalevents.json</c> 의 할로윈
    /// <c>zombieSettings.mapInfectionAmount</c> 에 쓰인 이름이다(대소문자는 가리지 않는다) —
    /// bigmap(세관), Interchange, Woods, Shoreline, RezervBase, Lighthouse, TarkovStreets,
    /// laboratory, Sandbox(그라운드 제로), factory4(팩토리 낮·밤).
    /// 0 이면 그 맵에는 좀비가 나오지 않는다. 적지 않은 맵은 건드리지 않는다.
    /// </summary>
    [JsonPropertyName("Infection")]
    public Dictionary<string, double> Infection { get; set; } = [];
}
