using SPTarkov.Server.Core.Models.Spt.Mod;

namespace ModCardKorean;

/// <summary>
/// 서버가 모드를 알아보는 데 쓰는 정보.
/// IModMetadata 의 속성은 모두 init 전용이라 set 으로 두면 구현으로 인정되지 않는다.
///
/// 이 모드는 <see cref="SPTarkov.Server.Web.IModBlazorMetadata"/> 를 <b>구현하지 않는다.</b>
/// 자기 페이지가 없으므로 첫 화면의 "모드 페이지" 구역에 카드를 하나 더 만들 이유가 없다.
/// </summary>
public record ModCardKoreanModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "spt.modcardkorean";
    public string Name { get; init; } = "Mod Card Korean";
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
