using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace ModCardKorean;

/// <summary>
/// 화면에 나오는 아이템·스킬 이름을 영문 → 한글로 바꿀 사전을 내놓는다.
///
/// <para>
/// 왜 라우트인가: <c>LocaleTable.Global</c> 의 <c>LazyLoad</c> 는 값을 읽는 순간 그때까지 걸린
/// 변환기를 모두 돌린다. <c>SptKoreanLocale</c> 은 PostLoad 패스(<c>TypePriority &gt;= 200000</c>)에서
/// 변환기를 거는데, 같은 패스 안의 순서는 보장되지 않는다. 기동 중에 읽으면 한글화 전 사전을
/// 뽑을 수 있다. 그래서 <b>브라우저가 물어볼 때</b> 읽는다 — 그때는 다 걸려 있다.
/// </para>
///
/// <para>
/// 디스크의 <c>locales/global/*.json</c> 을 쓰지 않는 이유는 따로 있다. 서버는 런타임에
/// 모드가 만든 아이템까지 합쳐 들고 있어서, 디스크 파일에는 그 이름이 아예 없다.
/// </para>
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("modcardkorean")]
public class ItemNamesController(LocaleTable localeTable, ISptLogger<ItemNamesController> logger)
    : ControllerBase
{
    /// <summary>아이템 이름 열쇠. 설명은 화면에 통째로 나오지 않고 분량만 키운다.</summary>
    private static readonly string[] Suffixes = [" Name", " ShortName"];

    private static readonly Lock Gate = new();
    private static Dictionary<string, string>? _cache;

    /// <summary>번역기와 UI 사전은 이 모드 폴더에 둔다. SPT 파일은 건드리지 않는다.</summary>
    [HttpGet("ko-ui.js")]
    public IActionResult GetScript() => OwnFile("ko-ui.js", "text/javascript; charset=utf-8");

    [HttpGet("ko-ui.json")]
    public IActionResult GetUiDictionary() => OwnFile("ko-ui.json", "application/json; charset=utf-8");

    private IActionResult OwnFile(string name, string contentType)
    {
        var folder = Path.GetDirectoryName(typeof(ItemNamesController).Assembly.Location);
        var path = folder is null ? null : Path.Combine(folder, name);

        if (path is null || !System.IO.File.Exists(path))
        {
            logger.Error($"[ModCardKorean] 파일이 없다: {name}");
            return NotFound();
        }

        return File(System.IO.File.ReadAllBytes(path), contentType);
    }

    [HttpGet("item-names.json")]
    public ActionResult<Dictionary<string, string>> Get()
    {
        try
        {
            return Ok(Build());
        }
        catch (Exception ex)
        {
            // 사전을 못 만들어도 화면은 떠야 한다. UI 번역은 따로 돈다.
            logger.Error($"[ModCardKorean] 아이템 이름 사전을 만들지 못했다: {ex.Message}");
            return Ok(new Dictionary<string, string>());
        }
    }

    private Dictionary<string, string> Build()
    {
        lock (Gate)
        {
            if (_cache is not null)
            {
                return _cache;
            }

            if (!localeTable.Global.TryGetValue("en", out var enLazy) || enLazy is null
                || !localeTable.Global.TryGetValue("kr", out var krLazy) || krLazy is null)
            {
                logger.Error("[ModCardKorean] 글로벌 로케일에 'en' 또는 'kr' 이 없다. 빈 사전을 돌려준다.");
                return _cache = [];
            }

            // 값을 읽는 이 순간 변환기가 모두 돈다. SptKoreanLocale 의 결과가 여기 들어온다.
            var en = enLazy.Value as Dictionary<string, string>;
            var kr = krLazy.Value as Dictionary<string, string>;

            if (en is null || kr is null)
            {
                logger.Error("[ModCardKorean] 로케일 사전을 읽지 못했다. 빈 사전을 돌려준다.");
                return _cache = [];
            }

            // 같은 영문이 서로 다른 한글로 갈리면 버린다. 엉뚱한 것으로 바뀌느니 영문이 낫다.
            var byEnglish = new Dictionary<string, HashSet<string>>();

            foreach (var (key, enValue) in en)
            {
                if (!Suffixes.Any(s => key.EndsWith(s, StringComparison.Ordinal)))
                {
                    continue;
                }

                if (!kr.TryGetValue(key, out var krValue))
                {
                    continue;
                }

                var e = Clean(enValue);
                var k = Clean(krValue);

                if (e.Length == 0 || k.Length == 0 || e == k)
                {
                    continue;
                }

                if (!e.Any(char.IsAsciiLetter))
                {
                    continue; // 영문이 없으면 바꿀 것도 없다
                }

                if (!k.Any(IsHangul))
                {
                    continue; // 한글이 아니면 번역이 아니다
                }

                // 짧은 한 낱말은 버린다. "Case"·"Buck"·"Core" 처럼 UI 라벨이나 사람 이름과
                // 부딪쳐 엉뚱한 자리를 바꿔 놓는다. 아이템 하나를 놓치는 편이 낫다.
                if (e.Length <= 6 && !e.Any(c => c is ' ' or '('))
                {
                    continue;
                }

                if (!byEnglish.TryGetValue(e, out var set))
                {
                    byEnglish[e] = set = [];
                }

                set.Add(k);
            }

            var dict = new Dictionary<string, string>(byEnglish.Count, StringComparer.Ordinal);
            var dropped = 0;

            foreach (var (e, set) in byEnglish)
            {
                if (set.Count > 1)
                {
                    dropped++;
                    continue;
                }

                dict[e] = set.First();
            }

            logger.Info(
                $"[ModCardKorean] 아이템 이름 사전 {dict.Count} 줄을 만들었다"
                + $" (영문 {byEnglish.Count} 종 / 갈려서 버린 것 {dropped} 종)."
            );

            return _cache = dict;
        }
    }

    private static bool IsHangul(char c) => c >= '가' && c <= '힣';

    /// <summary>
    /// 로케일 값에서 서식 태그를 떼고 공백을 고른다.
    ///
    /// <para>
    /// 다른 모드가 이름에 Unity 리치 텍스트를 넣어 둔다
    /// (보기: <c>&lt;b&gt;&lt;color=#FFFFFF&gt;Colt M4A1 … ●&lt;/color&gt;&lt;/b&gt;</c>).
    /// 게임 안에서는 색이 입혀 보이지만, 웹 화면에 넣으면 <b>태그가 글자 그대로 찍힌다.</b>
    /// 우리는 텍스트 노드에 넣으므로 실행되지는 않지만 보기 흉하다.
    /// </para>
    /// </summary>
    private static string Clean(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder(raw.Length);
        var depth = 0;

        foreach (var c in raw)
        {
            if (c == '<')
            {
                depth++;
            }
            else if (c == '>')
            {
                if (depth > 0)
                {
                    depth--;
                }
            }
            else if (depth == 0)
            {
                sb.Append(c);
            }
        }

        // 태그를 떼면 공백이 겹쳐 남는다.
        return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }
}
