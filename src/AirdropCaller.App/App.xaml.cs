using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AirdropCaller.App;

public partial class App : Application
{
    /// <summary>
    /// **개발용 스위치.** `AirdropCaller.exe --shot <저장경로.png> [기다릴초]` 로 부르면
    /// 사람에게 창을 보여 주지 않고 화면을 PNG 로 찍고 끝낸다.
    ///
    /// 왜 필요한가 — 이 창은 사람 눈 말고는 볼 길이 없어서, 배치가 어긋나도 **아무 관문도
    /// 잡지 못한다.** ProfilePorter 에서 같은 스위치로 화면 여섯 군데를 고쳤다(2026-11-09).
    /// 서버가 꺼져 있어도 찍힌다 — 상태줄에 "서버에 붙지 못했다" 가 뜬 모습 그대로다.
    ///
    /// 사람이 쓰는 길에는 영향이 없다. 인자가 없으면 평소대로 <c>MainWindow</c> 가 뜬다.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length < 2 || !string.Equals(e.Args[0], "--shot", StringComparison.Ordinal))
        {
            return;
        }

        // StartupUri 가 이미 띄운 창을 쓰지 않고 우리가 따로 띄운다 — 사람 화면에 번쩍이지 않게.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var seconds = e.Args.Length > 2 && double.TryParse(e.Args[2], out var s) ? s : 4;
        _ = ShotAsync(e.Args[1], seconds);
    }

    private async Task ShotAsync(string outPath, double seconds)
    {
        var window = new MainWindow
        {
            // 화면 밖에 띄운다 — 찍으려면 레이아웃이 실제로 돌아야 한다.
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = -4000,
            ShowInTaskbar = false,
            Topmost = false,
        };

        window.Show();

        // `StartupUri` 가 띄운 창은 사람 화면에 보인다 — 찍는 길에서는 치운다.
        foreach (Window other in Windows)
        {
            if (!ReferenceEquals(other, window))
            {
                other.Hide();
            }
        }

        // 서버 왕복과 아이콘 내려받기가 끝나기를 기다린다. 그림이 덜 온 채로 찍으면
        // **없는 결함**을 보게 된다.
        await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, seconds)));
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
