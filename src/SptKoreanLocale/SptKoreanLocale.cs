using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptKoreanLocale;

/// <summary>
/// 게임 DB locale(아이템·퀘스트·목표·봇 역할 이름 …)의 한국어를 우리 사전으로 덮어쓴다.
///
/// F12 설정 창을 <c>SptKoreanConfig</c> 하나로 처리한 것과 같은 꼴이다 — 사전은 이 저장소에 두고,
/// 모드는 그것을 읽어 덮어쓰기만 한다. **다른 모드의 파일은 건드리지 않는다**(업데이트에 날아간다).
///
/// <para>
/// 어떻게 되는가 (SPT 4.1.6 역컴파일 + 서버 실측):
/// </para>
/// <list type="number">
/// <item><c>LocaleTable.Global</c> 은 언어마다 <c>LazyLoad&lt;GlobalLocaleDictionary&gt;</c> 를 들고 있다.</item>
/// <item><b>그 사전에 직접 쓰면 사라진다.</b> <c>LazyLoad.AddTransformer</c> 는 <c>Clear()</c> 를 불러
/// 캐시를 버리고, 다음 읽기에서 파일을 다시 읽어 등록된 변환기만 다시 입힌다. 즉 우리보다 늦게
/// 변환기를 등록하는 모드가 하나라도 있으면 우리가 사전에 쓴 값은 통째로 날아간다.
/// 실측: <c>PostLoad</c> 에서 22항목을 쓰고 로그에 22를 적었는데, 그 뒤 사전이 60,398 → 61,125 항목으로
/// 다시 세워져 <c>/client/locale/kr</c> 에는 22항목 전부가 영문으로 돌아와 있었다.</item>
/// <item>그래서 값을 쓰지 않고 <b>변환기를 등록한다.</b> 변환기는 사전이 다시 세워질 때마다
/// 등록 순서대로 돌므로, 늦게 등록한 우리 것이 마지막에 입혀진다.</item>
/// <item><c>DataCallbacks.GetLocalesGlobal</c> 은 요청마다 <c>LocaleService.GetLocaleDb(url 의 언어)</c> →
/// <c>LazyLoad.Value</c> 를 내려보내므로, 변환기를 거친 사전이 그대로 클라이언트에 간다.</item>
/// </list>
///
/// <para>
/// 순서는 <c>PostLoad</c> 여야 한다. 모드가 만드는 아이템·퀘스트의 locale 열쇠는 그 모드가 돈 뒤에
/// 생기므로, 이르게 돌면 <c>onlyExistingKeys</c> 검사가 그 열쇠를 "없다" 고 잡는다.
/// </para>
///
/// <para>
/// <c>LocaleTable.Global</c> 에는 "DO NOT USE THIS PROPERTY DIRECTLY" 주석이 붙어 있다. 그 주석이 막는
/// 것은 <b>값을 직접 고치는 것</b>이고(바로 위에서 날아간 그 방식이다), 변환기를 등록하는 길은
/// <c>LazyLoad</c> 에만 있어 이 속성을 거치는 수밖에 없다. 값은 건드리지 않는다.
/// </para>
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]
public class SptKoreanLocale(
    ISptLogger<SptKoreanLocale> logger,
    LocaleTable localeTable,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>사전 폴더 이름.</summary>
    public const string LangFolderName = "lang";

    /// <summary>적용 결과를 내놓는 파일 이름. 검사 도구가 읽는다.</summary>
    public const string ReportFileName = "applied-report.json";

    private int _runs;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<Config>(modFolder, "config.jsonc");

        var dicts = ReadDictionaries(modFolder);
        var entryCount = dicts.Sum(d => d.File.Entries.Count);
        var keptCount = dicts.Sum(d => d.File.KeptAsIs.Count);

        if (!config.ModEnabled)
        {
            logger.Info(
                $"[SptKoreanLocale] 꺼져 있어 아무것도 바꾸지 않았다. 사전 {dicts.Count}개 / 항목 {entryCount}개"
                + $" / 일부러 영문으로 둔 것 {keptCount}개.");
            return Task.CompletedTask;
        }

        if (entryCount == 0)
        {
            logger.Warning($"[SptKoreanLocale] 사전이 비어 있다({modFolder}\\{LangFolderName}). 건너뛴다.");
            return Task.CompletedTask;
        }

        // 안전장치는 닫힌 쪽으로 넘어뜨린다 —
        // LocaleService.GetLocaleDb 는 그 언어가 없으면 조용히 'en' 을 돌려준다. 여기서는 표를 직접 보고
        // 그 언어가 정말 있는지 확인하므로, 없으면 영어 사전을 건드릴 일 자체가 없다.
        if (!localeTable.Global.TryGetValue(config.Locale, out var lazy) || lazy is null)
        {
            logger.Error(
                $"[SptKoreanLocale] locale '{config.Locale}' 이 이 설치본에 없다."
                + " 다른 언어를 건드리지 않으려고 아무것도 하지 않는다."
                + " locales/global 에 그 언어 파일이 있는지 본다.");
            return Task.CompletedTask;
        }

        // 값을 쓰지 않고 변환기를 등록한다. 사전이 다시 세워질 때마다 등록 순서대로 돌고,
        // 우리는 PostLoad 라 늦게 등록되므로 마지막에 입혀진다.
        lazy.AddTransformer(dict => Apply(dict, config, dicts, modFolder, entryCount, keptCount));

        logger.Info(
            $"[SptKoreanLocale] locale '{config.Locale}' 에 한국어 변환기를 걸었다 —"
            + $" 사전 {dicts.Count}개 / 항목 {entryCount}개, 일부러 영문으로 둔 것 {keptCount}개."
            + " 실제 적용 셈은 사전이 처음 읽힐 때 적는다.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// 사전이 세워질 때마다 불린다. 여기서 값을 바꾼다 — 받은 사전을 그대로 고쳐 돌려준다.
    /// </summary>
    private GlobalLocaleDictionary? Apply(
        GlobalLocaleDictionary? target,
        Config config,
        List<(string Name, LangFile File)> dicts,
        string modFolder,
        int entryCount,
        int keptCount)
    {
        if (target is null)
        {
            return target;
        }

        var run = Interlocked.Increment(ref _runs);
        var applied = 0;
        var missing = new List<string>();
        var sameAlready = 0;
        var overwroteKorean = 0;
        var skippedKorean = 0;

        foreach (var (name, file) in dicts)
        {
            foreach (var (key, korean) in file.Entries)
            {
                if (string.IsNullOrWhiteSpace(korean))
                {
                    continue;
                }

                var exists = target.TryGetValue(key, out var current);
                if (!exists && config.OnlyExistingKeys)
                {
                    missing.Add($"{name}:{key}");
                    continue;
                }

                if (exists)
                {
                    if (string.Equals(current, korean, StringComparison.Ordinal))
                    {
                        sameAlready++;
                        continue;
                    }

                    if (HasKorean(current))
                    {
                        if (config.SkipAlreadyKorean)
                        {
                            skippedKorean++;
                            continue;
                        }

                        overwroteKorean++;
                    }
                }

                target[key] = korean;
                applied++;

                if (config.Debug)
                {
                    logger.Info($"[SptKoreanLocale] {key}: \"{current}\" → \"{korean}\"");
                }
            }
        }

        WriteReport(modFolder, config, dicts, run, target.Count, applied, missing, sameAlready, overwroteKorean, skippedKorean);

        // 사전은 요청마다 다시 세워질 수 있다. 첫 번째만 Info 로 적고 그 뒤는 Debug 로 내린다 —
        // 로그를 매 요청 채우지 않으면서, 한 번은 실측값이 남게 한다.
        var summary =
            $"[SptKoreanLocale] locale '{config.Locale}' 항목 {target.Count}개 가운데 {applied}개를 한국어로 바꿨다"
            + $" (사전 {dicts.Count}개 / 항목 {entryCount}개, 이미 같던 것 {sameAlready}개"
            + $", 일부러 영문으로 둔 것 {keptCount}개, 적용 {run}번째).";

        if (run == 1)
        {
            logger.Info(summary);
        }
        else if (config.Debug)
        {
            logger.Debug(summary);
        }

        if (run == 1 && overwroteKorean > 0)
        {
            logger.Info($"[SptKoreanLocale] 그중 {overwroteKorean}개는 이미 한글이던 값을 덮어쓴 것이다.");
        }

        if (run == 1 && skippedKorean > 0)
        {
            logger.Info($"[SptKoreanLocale] 이미 한글이어서 건너뛴 항목 {skippedKorean}개 (skipAlreadyKorean).");
        }

        if (run == 1 && missing.Count > 0)
        {
            logger.Warning(
                $"[SptKoreanLocale] 이 설치본의 locale 에 없는 열쇠 {missing.Count}개 — 그 모드를 뺐거나 사전에 오타다: "
                + string.Join(", ", missing.Take(12))
                + (missing.Count > 12 ? $" … (+{missing.Count - 12})" : string.Empty));
        }

        return target;
    }

    /// <summary><c>lang/*.json</c> 을 전부 읽는다. 밑줄로 시작하는 파일은 건너뛴다(메모·보류용).</summary>
    private List<(string Name, LangFile File)> ReadDictionaries(string modFolder)
    {
        var result = new List<(string, LangFile)>();
        var folder = System.IO.Path.Combine(modFolder, LangFolderName);

        if (!Directory.Exists(folder))
        {
            logger.Warning($"[SptKoreanLocale] 사전 폴더가 없다: {folder}");
            return result;
        }

        foreach (var path in Directory.GetFiles(folder, "*.json").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith('_'))
            {
                continue;
            }

            try
            {
                var parsed = modHelper.GetJsonDataFromFile<LangFile>(folder, System.IO.Path.GetFileName(path));
                result.Add((name, parsed));
            }
            catch (Exception ex)
            {
                logger.Error($"[SptKoreanLocale] 사전을 읽지 못했다 {name}.json — {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 무엇을 했는지 파일로 내놓는다. 서버 로그는 기동마다 덮이고, 검사 도구가 읽을 자리가 필요하다.
    /// </summary>
    private void WriteReport(
        string modFolder,
        Config config,
        List<(string Name, LangFile File)> dicts,
        int run,
        int localeCount,
        int applied,
        List<string> missing,
        int sameAlready,
        int overwroteKorean,
        int skippedKorean)
    {
        try
        {
            var payload = new
            {
                at = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ssK"),
                locale = config.Locale,
                run,
                localeEntries = localeCount,
                dictionaries = dicts.Select(d => new
                {
                    name = d.Name,
                    entries = d.File.Entries.Count,
                    keptAsIs = d.File.KeptAsIs.Count,
                }),
                applied,
                sameAlready,
                overwroteKorean,
                skippedKorean,
                missingKeys = missing,
            };

            File.WriteAllText(
                System.IO.Path.Combine(modFolder, ReportFileName),
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            logger.Warning($"[SptKoreanLocale] 결과 파일을 내놓지 못했다 — {ex.Message}");
        }
    }

    private static bool HasKorean(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var c in value)
        {
            // 한글 음절 + 자모. 호환 자모(ㄱ-ㅣ)까지 본다.
            if ((c >= '가' && c <= '힣')
                || (c >= 'ᄀ' && c <= 'ᇿ')
                || (c >= '㄰' && c <= '㆏'))
            {
                return true;
            }
        }

        return false;
    }
}
