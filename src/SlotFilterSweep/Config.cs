using System.Text.Json.Serialization;

namespace SlotFilterSweep;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 표를 훑지도 않는다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>
    /// **기본값은 false — 세기만 하고 지우지 않는다.**
    ///
    /// 죽은 참조를 지우는 것은 남의 모드가 적어 둔 목록을 고치는 일이다. 먼저 몇 개인지
    /// 로그와 <c>dead-refs.json</c> 으로 보고, 그 숫자를 납득한 뒤에 켜는 순서가 맞다.
    /// </summary>
    [JsonPropertyName("Apply")]
    public bool Apply { get; set; }

    /// <summary>칸(Slots)을 본다.</summary>
    [JsonPropertyName("IncludeSlots")]
    public bool IncludeSlots { get; set; } = true;

    /// <summary>약실(Chambers)을 본다.</summary>
    [JsonPropertyName("IncludeChambers")]
    public bool IncludeChambers { get; set; } = true;

    /// <summary>탄창 칸(Cartridges)을 본다.</summary>
    [JsonPropertyName("IncludeCartridges")]
    public bool IncludeCartridges { get; set; } = true;

    /// <summary>
    /// 지키는 id. 여기 적힌 것은 표에 없어도 **건드리지 않는다**.
    ///
    /// 우리는 맨 끝(<c>int.MaxValue</c>)에서 돌지만, 그보다 더 늦게 아이템을 만드는 모드가
    /// 생길 수 있다. 그때 "지금은 없는" 부품이 잘못 지워지는 것을 막는 자리다.
    /// </summary>
    [JsonPropertyName("KeepIds")]
    public List<string> KeepIds { get; set; } = [];

    /// <summary>로그에 보여 줄 "가장 많이 나온 죽은 id" 개수.</summary>
    [JsonPropertyName("TopIds")]
    public int TopIds { get; set; } = 10;

    /// <summary>죽은 참조를 찾은 칸마다 한 줄씩 기록한다. 조사할 때만 켠다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// 찾은 것을 DLL 옆 <c>dead-refs.json</c> 으로 내놓는다. 로그를 긁지 않고 숫자를 보려는 자리다.
    /// </summary>
    [JsonPropertyName("WriteReport")]
    public bool WriteReport { get; set; } = true;
}
