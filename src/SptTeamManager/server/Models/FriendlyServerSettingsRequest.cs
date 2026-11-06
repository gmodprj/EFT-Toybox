using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

public record FriendlyServerSettingsRequest : IRequestData
{
    public const string DefaultLoadoutManagementMode = "Immersive";

    [JsonPropertyName("gameplayMode")]
    public string GameplayMode { get; set; } = "GunsForHire";

    [JsonIgnore]
    public bool IsAllegiance => GameplayMode == "Allegiance";

    [JsonPropertyName("pmcArmbands")]
    public bool PmcArmbands { get; set; } = true;

    public const int DefaultAutoJoinMaxMembers = 4;

    private int autoJoinMaxMembers = DefaultAutoJoinMaxMembers;

    // 출격할 때 자동으로 따라붙는 인원의 상한. **0 은 무제한**(2026-11-03 전까지의 동작)이고,
    // 기본값 4 는 EFT 의 파티 인원이다. 음수가 들어오면 0 으로 본다 - 설정 파일을 손으로 고쳐도
    // 터지지 않아야 하고, "음수 명이 따라온다" 는 뜻이 없다.
    [JsonPropertyName("autoJoinMaxMembers")]
    public int AutoJoinMaxMembers
    {
        get => autoJoinMaxMembers;
        set => autoJoinMaxMembers = value < 0 ? 0 : value;
    }

    private string loadoutManagementMode = DefaultLoadoutManagementMode;

    [JsonPropertyName("loadoutManagementMode")]
    public string LoadoutManagementMode
    {
        get => loadoutManagementMode;
        set => loadoutManagementMode = NormalizeLoadoutManagementMode(value);
    }

    public static string NormalizeLoadoutManagementMode(string? mode)
    {
        return mode?.Trim().ToLowerInvariant() switch
        {
            "restricted" or "simple" => "Restricted",
            "immersive" => "Immersive",
            "extreme" or "realistic" => "Extreme",
            _ => DefaultLoadoutManagementMode,
        };
    }

}
