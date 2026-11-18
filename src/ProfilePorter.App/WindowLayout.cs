using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ProfilePorter.App;

/// <summary>
/// 창 위치·크기와 칸 너비(<c>GridSplitter</c> 로 끈 것)를 exe 옆
/// <c>profile-porter.layout.json</c> 에 남기고, 다음에 열 때 되살린다.
///
/// **설정 파일과 따로 둔다.** 창을 닫을 때마다 쓰는 파일이라, 사람이 고쳐 둔
/// <c>profile-porter.config.json</c> 을 건드리지 않는다. 이 파일은 지워도 된다 — 지우면 기본 배치로 연다.
///
/// 칸 너비는 **픽셀을 별(*) 비율로** 되돌린다. 창 크기가 달라져도 나눈 비율이 그대로 간다.
/// </summary>
public static class WindowLayout
{
    public sealed class Entry
    {
        [JsonPropertyName("left")] public double Left { get; set; }
        [JsonPropertyName("top")] public double Top { get; set; }
        [JsonPropertyName("width")] public double Width { get; set; }
        [JsonPropertyName("height")] public double Height { get; set; }
        [JsonPropertyName("maximized")] public bool Maximized { get; set; }

        /// <summary>칸 너비(픽셀). 끄는 띠 칸은 빼고 별 칸만 차례대로 적는다.</summary>
        [JsonPropertyName("columns")] public double[] Columns { get; set; } = [];
    }

    private const string FileName = "profile-porter.layout.json";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string Path => System.IO.Path.Combine(AppContext.BaseDirectory, FileName);

    /// <summary>
    /// 남겨 둔 배치를 입히고, 창을 닫을 때 다시 적도록 건다. <b>창을 보이기 전에</b> 부른다.
    /// </summary>
    /// <param name="key">창 종류(<c>main</c>, <c>detail</c>).</param>
    /// <param name="columns">칸을 나눈 격자. 그 안의 별(*) 칸이 끌 수 있는 칸이다.</param>
    public static void Attach(Window window, string key, Grid? columns)
    {
        var saved = Load().GetValueOrDefault(key);
        if (saved is not null)
        {
            Apply(window, saved, columns);
        }

        window.Closing += (_, _) => Save(key, Capture(window, columns));
    }

    private static void Apply(Window window, Entry e, Grid? columns)
    {
        // 화면 구성이 바뀌어(모니터를 뺐다 등) 창이 화면 밖으로 가면 못 찾는다.
        // 제목 줄 언저리가 지금 화면 안에 있을 때만 자리를 되살린다. 크기는 어디서든 되살린다.
        if (e.Width >= window.MinWidth && e.Height >= window.MinHeight && e.Width > 100 && e.Height > 100)
        {
            window.Width = e.Width;
            window.Height = e.Height;
        }

        var vl = SystemParameters.VirtualScreenLeft;
        var vt = SystemParameters.VirtualScreenTop;
        var vr = vl + SystemParameters.VirtualScreenWidth;
        var vb = vt + SystemParameters.VirtualScreenHeight;
        if (e.Left + 100 > vl && e.Left + 100 < vr && e.Top >= vt && e.Top + 30 < vb)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = e.Left;
            window.Top = e.Top;
        }

        if (e.Maximized)
        {
            window.WindowState = WindowState.Maximized;
        }

        if (columns is not null)
        {
            var stars = StarColumns(columns);
            if (stars.Count == e.Columns.Length && e.Columns.All(w => w > 0 && double.IsFinite(w)))
            {
                for (var i = 0; i < stars.Count; i++)
                {
                    stars[i].Width = new GridLength(e.Columns[i], GridUnitType.Star);
                }
            }
        }
    }

    private static Entry Capture(Window window, Grid? columns)
    {
        // 최대화 중이면 Left/Width 는 최대화된 값이다. 되돌렸을 때의 자리(RestoreBounds)를 적는다.
        var bounds = window.WindowState == WindowState.Normal
            ? new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight)
            : window.RestoreBounds;

        return new Entry
        {
            Left = bounds.Left,
            Top = bounds.Top,
            Width = bounds.Width,
            Height = bounds.Height,
            Maximized = window.WindowState == WindowState.Maximized,
            Columns = columns is null ? [] : StarColumns(columns).Select(c => c.ActualWidth).ToArray(),
        };
    }

    /// <summary>끄는 띠(고정 너비) 칸을 뺀, 별(*) 칸들.</summary>
    private static List<ColumnDefinition> StarColumns(Grid grid) =>
        grid.ColumnDefinitions.Where(c => c.Width.IsStar).ToList();

    private static Dictionary<string, Entry> Load()
    {
        try
        {
            if (File.Exists(Path))
            {
                return JsonSerializer.Deserialize<Dictionary<string, Entry>>(File.ReadAllText(Path), Options) ?? [];
            }
        }
        catch
        {
            // 깨진 배치 파일은 기본 배치로 연다 — 창을 못 띄울 이유가 아니다. 닫을 때 새로 적힌다.
        }

        return [];
    }

    private static void Save(string key, Entry entry)
    {
        try
        {
            var all = Load();
            all[key] = entry;
            File.WriteAllText(Path, JsonSerializer.Serialize(all, Options));
        }
        catch
        {
            // 못 적으면 다음에 기본 배치로 열 뿐이다. 창 닫기를 막지 않는다.
        }
    }
}
