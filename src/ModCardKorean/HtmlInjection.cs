using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace ModCardKorean;

/// <summary>
/// 서버가 내보내는 모든 HTML 페이지에 번역기 스크립트 한 줄을 끼워 넣는다.
///
/// <para>
/// 왜 미들웨어인가: 호스트 HTML 은 <c>SPTarkov.Server.Web.dll</c> 안에 리터럴로 박혀 있어
/// 모드가 <c>&lt;head&gt;</c> 에 들어갈 자리가 없다. 디스크의 <c>blazor.web.js</c> 를 고치는 길도
/// 있지만, 그러면 SPT 파일을 건드리게 되고 업데이트마다 다시 해야 한다.
/// </para>
///
/// <para>
/// 무엇보다 <b>그 길로는 닿지 않는 페이지가 있다.</b> <c>/isb</c> 와
/// <c>/ultimateflea/app.html</c> 은 그 모드가 직접 내놓는 독립 HTML 이라
/// Blazor 호스트의 스크립트를 부르지 않는다. ISB 는 HTML 이 DLL 안에 박혀 있어
/// 파일로 고칠 수도 없다. 응답에 끼우면 셋 다 한 번에 잡힌다.
/// </para>
///
/// <para>
/// <c>IStartupFilter</c> 는 Microsoft 의 인터페이스라 SPT 의 DI 가 그대로 등록해 준다
/// (System 으로 시작하는 네임스페이스만 건너뛴다). 등록은 <c>builder.Build()</c> 전에
/// 끝나므로 파이프라인이 세워질 때 반영된다.
/// </para>
/// </summary>
[Injectable(InjectionType.Singleton, 10000)]
public class KoreanScriptStartupFilter(ISptLogger<KoreanScriptStartupFilter> logger) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(KoreanScriptInjector.Create(logger));
            next(app);
        };
    }
}

/// <summary>응답이 HTML 이면 <c>&lt;/head&gt;</c> 앞에 스크립트 한 줄을 끼운다.</summary>
internal static class KoreanScriptInjector
{
    /// <summary>두 번 끼우지 않으려는 표식.</summary>
    private const string Marker = "data-ko-ui";

    private const string Tag =
        "<script " + Marker + " src=\"/modcardkorean/ko-ui.js\" defer></script>";

    public static Func<HttpContext, Func<Task>, Task> Create(ISptLogger<KoreanScriptStartupFilter> logger)
    {
        return async (context, next) =>
        {
            if (!LooksLikePage(context))
            {
                await next();
                return;
            }

            var original = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await next();
            }
            finally
            {
                context.Response.Body = original;
            }

            buffer.Position = 0;

            try
            {
                if (ShouldInject(context))
                {
                    var html = Encoding.UTF8.GetString(buffer.ToArray());
                    var at = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);

                    if (at >= 0 && !html.Contains(Marker, StringComparison.Ordinal))
                    {
                        var bytes = Encoding.UTF8.GetBytes(html.Insert(at, Tag));
                        context.Response.ContentLength = bytes.Length;
                        await original.WriteAsync(bytes);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // 끼우지 못해도 원본은 그대로 내보낸다. 화면이 영어로 남을 뿐 깨지지 않는다.
                logger.Error($"[ModCardKorean] 번역기 스크립트를 끼우지 못했다: {ex.Message}");
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(original);
        };
    }

    /// <summary>
    /// 버퍼에 담아도 되는 요청인지 본다. 모든 응답을 버퍼에 담으면 큰 JSON 과
    /// 웹소켓까지 메모리에 올라간다.
    /// </summary>
    private static bool LooksLikePage(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Blazor 의 회선과 프레임워크 자원은 건드리지 않는다.
        if (path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (context.WebSockets.IsWebSocketRequest)
        {
            return false;
        }

        if (!HttpMethods.IsGet(context.Request.Method))
        {
            return false;
        }

        // 브라우저가 페이지를 달라고 한 요청만 본다.
        var accept = context.Request.Headers.Accept.ToString();
        return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldInject(HttpContext context)
    {
        if (context.Response.StatusCode is < 200 or >= 300)
        {
            return false;
        }

        // 이미 압축돼 나가는 응답은 손대면 깨진다.
        if (!string.IsNullOrEmpty(context.Response.Headers.ContentEncoding.ToString()))
        {
            return false;
        }

        var type = context.Response.ContentType ?? string.Empty;
        return type.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }
}
