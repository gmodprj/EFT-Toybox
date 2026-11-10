using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfilePorter.App;

public partial class App : Application
{
    /// <summary>
    /// **개발용 스위치.** `ProfilePorter.exe --shot <파일이름> <저장경로.png> [상자id]` 로 부르면
    /// 창을 띄우지 않고 세부 보기 화면을 PNG 로 찍고 끝낸다.
    ///
    /// 왜 필요한가 — 이 창은 Start 메뉴에 없어 화면을 사람 눈 말고는 볼 길이 없었다. 그래서
    /// 배치가 어긋나도 **아무 관문도 잡지 못했다**(2026-11-09 사용자 제보: "다 어긋났다").
    /// 이 스위치가 그 자리를 메운다 — 고친 뒤 같은 명령으로 다시 찍어 견주면 된다.
    ///
    /// 사람이 쓰는 길에는 영향이 없다. 인자가 없으면 평소대로 `MainWindow` 가 뜬다.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length < 3 || !string.Equals(e.Args[0], "--shot", StringComparison.Ordinal))
        {
            return;
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _ = ShotAsync(e.Args[1], e.Args[2], e.Args.Length > 3 ? e.Args[3] : null);
    }

    private async Task ShotAsync(string fileName, string outPath, string? containerId)
    {
        var config = AppConfig.Load(out _);
        var sessionId = config.ResolveSessionId(out var how);

        if (string.IsNullOrEmpty(sessionId))
        {
            Console.Error.WriteLine($"세션 id 를 찾지 못했다 — {how}");
            Shutdown(2);
            return;
        }

        var client = new ServerClient(config.SptUrl, sessionId);
        var icons = new IconService(config.ResolveIconCacheFolder(), config.DownloadItemIcons);
        var window = new FileDetailWindow(client, icons, fileName, containerId)
        {
            // 화면 밖에 띄운다 — 찍으려면 레이아웃이 실제로 돌아야 한다.
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = -4000,
            ShowInTaskbar = false,
        };

        window.Show();

        // 서버 왕복과 아이콘 내려받기가 끝나기를 기다린다. 그림이 덜 온 채로 찍으면
        // **없는 결함**을 보게 된다.
        await Task.Delay(TimeSpan.FromSeconds(6));
        await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);

        var width = (int)Math.Ceiling(window.ActualWidth);
        var height = (int)Math.Ceiling(window.ActualHeight);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using (var stream = File.Create(outPath))
        {
            encoder.Save(stream);
        }

        Console.Error.WriteLine($"찍었다: {outPath} ({width}x{height})");
        window.Close();
        Shutdown(0);
    }
}
