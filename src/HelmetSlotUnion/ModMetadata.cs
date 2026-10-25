using SPTarkov.Server.Core.Models.Spt.Mod;

namespace HelmetSlotUnion;

/// <summary>서버가 모드를 알아보는 데 쓰는 정보.</summary>
public record HelmetSlotUnionModMetadata : IModMetadata
{
    public string ModGuid { get; set; } = "spt.helmetslotunion";
    public string Name { get; set; } = "Helmet Slot Union";
    public string Author { get; set; } = "Qahnaarin";
    public List<string>? Contributors { get; set; }
    public SemanticVersioning.Version Version { get; set; } = new("0.1.0");
    public SemanticVersioning.Range SptVersion { get; set; } = new("~4.1.6");
    public bool HasPrepatcher { get; set; }
    public List<string>? Incompatibilities { get; set; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; set; }
    public string? Url { get; set; } = "https://github.com/Qahnaarin/Spt_Mods";
    public string? License { get; set; } = "MIT";
}
