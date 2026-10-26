using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

namespace WebConfigBridge;

/// <summary>
/// 서버가 모드를 알아보는 데 쓰는 정보.
/// IModMetadata 의 속성은 모두 init 전용이라 set 으로 두면 구현으로 인정되지 않는다.
///
/// <see cref="IModBlazorMetadata"/> 를 함께 구현하면 서버 웹 페이지 첫 화면의
/// "모드 페이지" 구역에 항목이 생긴다(같은 방식을 쓰는 설치된 전례: JeroBackpack.ModMetadata).
/// </summary>
public record WebConfigBridgeModMetadata : IModMetadata, IModBlazorMetadata
{
    /// <summary>정적 자원은 쓰지 않는다. 화면은 DLL 안의 Razor 컴포넌트가 그린다.</summary>
    public string? WWWRootUrl { get; init; } = string.Empty;

    /// <summary>첫 화면의 "모드 페이지" 카드가 가리키는 경로. Home.razor 의 @@page 와 같아야 한다.</summary>
    public string? HomePage { get; init; } = "/webconfigbridge";

    public string? HomePageDescription { get; init; } =
        "All In Weapon: Unslotted 의 무기·부품·탄약 호환 설정을 고친다";

    public string ModGuid { get; init; } = "spt.webconfigbridge";
    public string Name { get; init; } = "Web Config Bridge";
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
