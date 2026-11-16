using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Launcher;

namespace ModCardKorean;

/// <summary>
/// 서버 웹 첫 화면(SIC)의 "모드 페이지" 카드 글귀를 한국어로 바꾼다.
///
/// <para>
/// 카드 글귀는 <c>locales/web/ko.json</c> 을 거치지 않는다.
/// <c>SPTWeb.InitializeSptBlazor</c> 가 각 모드의 <c>IModBlazorMetadata</c> 에서 읽어
/// <c>IReadOnlyList&lt;ModPage&gt;</c> 싱글턴을 만들고, <c>LandingPage</c> 는 그 값을
/// 로케일 조회 없이 그대로 찍는다. 즉 글귀는 남의 DLL 안에 리터럴로 박혀 있다.
/// </para>
///
/// <para>
/// 다만 <see cref="ModPage"/> 는 record 가 아니라 setter 가 있는 보통 클래스이고,
/// 그 목록이 DI 싱글턴으로 올라가 있다. 서버 기동 순서가
/// <c>builder.Build()</c> → <c>RunPreSptLoadCallbacks</c> 이므로, 이 시점에는 목록이 이미 있다.
/// 그래서 <b>남의 DLL 을 건드리지 않고</b> 제자리에서 글귀만 바꿀 수 있다.
/// </para>
///
/// <para>
/// <c>TypePriority</c> 를 반드시 적는다. 기본값은 <c>int.MaxValue</c> 인데
/// <c>RunPreSptLoadCallbacks</c> 는 <c>0 이상 200000 미만</c>만 부른다. 비워 두면 조용히 안 돈다.
/// </para>
/// </summary>
[Injectable(InjectionType.Singleton, 10000)]
public class ModCardLocalizer(
    IReadOnlyList<ModPage> modPages,
    ModHelper modHelper,
    ISptLogger<ModCardLocalizer> logger
) : IOnLoad
{
    private const string DictionaryFileName = "mod-cards.ko.json";

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>사전 한 줄. 둘 다 없으면 그 카드는 원문 그대로 둔다.</summary>
    public record CardText
    {
        /// <summary>카드 제목. 모드 이름은 고유명이라 보통 비워 두고 원문을 쓴다.</summary>
        public string? Name { get; init; }

        /// <summary>카드 설명.</summary>
        public string? Description { get; init; }
    }

    public Task OnLoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Apply();
        }
        catch (Exception ex)
        {
            // 한글화가 실패해도 서버는 떠야 한다. 영어 카드로 남을 뿐이다.
            logger.Error($"[ModCardKorean] 카드 한글화에 실패했다. 원문 그대로 둔다: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private void Apply()
    {
        var dictionary = LoadDictionary();
        if (dictionary is null)
        {
            return;
        }

        var applied = 0;
        var missing = new List<string>();

        foreach (var page in modPages)
        {
            if (!dictionary.TryGetValue(page.HomePage, out var text))
            {
                // 사전에 없는 모드는 건드리지 않는다. 새 모드가 들어와도 깨지지 않는다.
                missing.Add($"{page.Name} ({page.HomePage})");
                continue;
            }

            if (!string.IsNullOrWhiteSpace(text.Name))
            {
                page.Name = text.Name;
            }

            if (!string.IsNullOrWhiteSpace(text.Description))
            {
                page.Description = text.Description;
            }

            applied++;
        }

        logger.Info($"[ModCardKorean] 모드 카드 {applied}/{modPages.Count} 개를 한국어로 바꿨다.");

        if (missing.Count > 0)
        {
            logger.Warning(
                $"[ModCardKorean] 사전에 없어 원문으로 남은 카드 {missing.Count} 개: {string.Join(", ", missing)}"
            );
        }
    }

    private Dictionary<string, CardText>? LoadDictionary()
    {
        var folder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var path = Path.Combine(folder, DictionaryFileName);

        if (!File.Exists(path))
        {
            logger.Error($"[ModCardKorean] 사전 파일이 없다: {path}");
            return null;
        }

        // '_' 로 시작하는 키는 주석 줄이다. 모양이 달라도 터지지 않게 먼저 걸러 낸다.
        var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            File.ReadAllText(path),
            ReadOptions
        );

        var parsed = raw
            ?.Where(entry => !entry.Key.StartsWith('_') && entry.Value.ValueKind == JsonValueKind.Object)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Deserialize<CardText>(ReadOptions) ?? new CardText()
            );

        if (parsed is null || parsed.Count == 0)
        {
            logger.Error($"[ModCardKorean] 사전 파일이 비어 있다: {path}");
            return null;
        }

        return parsed;
    }
}
