using SPTarkov.Server.Core.Models.Spt.Mod;

namespace ZombieMapInfection;

/// <summary>서버가 모드를 알아보는 데 쓰는 정보. IModMetadata 의 속성은 init 전용이어야 한다.</summary>
public record ZombieMapInfectionModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "spt.zombiemapinfection";
    public string Name { get; init; } = "Zombie Map Infection";
    public string Author { get; init; } = "Qahnaarin";
    public List<string>? Contributors { get; init; } = [];
    public SemanticVersioning.Version Version { get; init; } = new("0.1.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.6");
    public bool HasPrepatcher { get; init; }
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = "https://github.com/Qahnaarin/Spt_Mods";
    public string License { get; init; } = "MIT";
}
