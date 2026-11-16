using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

namespace ModCardKorean;

/// <summary>
/// 서버가 모드를 알아보는 데 쓰는 정보.
/// IModMetadata 의 속성은 모두 init 전용이라 set 으로 두면 구현으로 인정되지 않는다.
///
/// <see cref="IModBlazorMetadata"/> 를 다는 이유는 카드를 만들기 위해서가 <b>아니다</b> —
/// 서버는 이 인터페이스를 단 모드의 어셈블리만 <c>AddApplicationPart</c> 로 받는다
/// (<c>SPTWeb.InitializeSptBlazor</c>). 컨트롤러를 등록하려면 이 길뿐이다.
/// <c>HomePage</c> 를 비워 두면 첫 화면의 "모드 페이지" 구역에 <b>카드가 생기지 않는다</b> —
/// 그 목록이 <c>HomePage</c> 가 빈 모드를 걸러 내기 때문이다.
/// </summary>
public record ModCardKoreanModMetadata : IModMetadata, IModBlazorMetadata
{
    /// <summary>정적 자원은 쓰지 않는다.</summary>
    public string? WWWRootUrl { get; init; } = string.Empty;

    /// <summary>비워 둔다. 카드를 만들지 않으려는 것이다.</summary>
    public string? HomePage { get; init; }

    /// <summary>카드가 없으므로 쓰이지 않는다.</summary>
    public string? HomePageDescription { get; init; }

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
