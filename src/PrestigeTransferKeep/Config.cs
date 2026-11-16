using System.Text.Json.Serialization;

namespace PrestigeTransferKeep;

/// <summary>DLL 옆 config.jsonc 를 그대로 옮긴 것.</summary>
public class Config
{
    /// <summary>false 면 아무 패치도 걸지 않는다 — 3332 가 하던 대로 둔다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>
    /// 넘기기로 고른 물건을 **그대로 받는다**(바닐라 전송 우편). 3332 가 비워 버린 목록을 도로 끼운다.
    /// </summary>
    [JsonPropertyName("KeepTransferItems")]
    public bool KeepTransferItems { get; set; } = true;

    /// <summary>
    /// 전송분으로 주던 **프레스티지 코인 우편을 막는다**. 물건을 그대로 받으면서 코인까지 받으면
    /// 두 번 받는 셈이라 기본값은 막는 쪽이다.
    ///
    /// 꺼도 보스·금고에서 나오는 코인(`bossCoins`·`safeCoins`)은 그대로다 — 막는 것은
    /// **전송분 환산**뿐이고, 퍽을 살 길이 사라지지 않는다.
    /// </summary>
    [JsonPropertyName("SuppressCoinMail")]
    public bool SuppressCoinMail { get; set; } = true;

    /// <summary>끼어들 때마다 로그에 한 줄씩.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; } = true;
}
