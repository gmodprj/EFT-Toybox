using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AirdropCaller.App;

/// <summary>
/// 분류 아이콘 파일 이름을 그림으로 바꾼다. `SPT_Data\images\handbook` 에서 읽는다.
///
/// **아이템마다의 아이콘은 없다.** 게임 번들 안에 있어 서버도 못 준다. 여기 있는
/// 것은 분류 아이콘 83개뿐이라 같은 분류의 아이템은 같은 그림을 단다 — 없는 것보다
/// 훑기 쉽다.
/// </summary>
public class FileToIconConverter : IValueConverter
{
    /// <summary>`E:\Spt` 같은 설치 폴더. 창이 뜰 때 한 번 정한다.</summary>
    public static string SptDir { get; set; } = string.Empty;

    // 그림 받기를 한꺼번에 돌리면서 여기서도 불린다. 사전은 잠가야 한다.
    private readonly Dictionary<string, BitmapImage?> _cache = [];
    private readonly object _lock = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string file || file.Length == 0 || SptDir.Length == 0)
        {
            return null;
        }

        lock (_lock)
        {
            if (_cache.TryGetValue(file, out var cached))
            {
                return cached;
            }
        }

        BitmapImage? image = null;
        try
        {
            var path = Path.Combine(SptDir, "SPT_Runtime", "SPT_Data", "images", "handbook", file);
            if (File.Exists(path))
            {
                image = new BitmapImage();
                image.BeginInit();
                // 파일을 붙잡고 있지 않도록 통째로 읽어 둔다.
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path);
                image.DecodePixelHeight = 20;
                image.EndInit();
                image.Freeze();
            }
        }
        catch
        {
            // 그림 하나 없다고 목록을 못 보여 줄 이유는 없다.
            image = null;
        }

        lock (_lock)
        {
            _cache[file] = image;
        }

        return image;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// 서버가 준 `#RRGGBB` 를 붓으로 바꾼다. 없거나 읽을 수 없으면 기본 글자색.
///
/// 다른 모드가 아이템 이름에 색으로 등급을 표시한다. 태그는 글자에서 떼되
/// **그 정보는 색으로 되살린다** — 안 그러면 구분이 사라진다.
/// 너무 어두운 색은 어두운 바탕에서 안 보이므로 밝기를 끌어올린다.
/// </summary>
public class HexToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Default = new(Color.FromRgb(0xE6, 0xE6, 0xE6));
    private readonly Dictionary<string, SolidColorBrush> _cache = [];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || hex.Length < 7 || hex[0] != '#')
        {
            return Default;
        }

        if (_cache.TryGetValue(hex, out var cached))
        {
            return cached;
        }

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var brush = new SolidColorBrush(Brighten(color));
            brush.Freeze();
            _cache[hex] = brush;
            return brush;
        }
        catch
        {
            return Default;
        }
    }

    /// <summary>
    /// 어두운 바탕(#2D2D30)에 얹을 것이므로 너무 어두우면 읽을 수 없다.
    /// 색감은 두고 밝기만 올린다.
    /// </summary>
    private static Color Brighten(Color c)
    {
        var max = Math.Max(c.R, Math.Max(c.G, c.B));
        if (max >= 150)
        {
            return c;
        }

        var scale = max == 0 ? 1f : 180f / max;
        return Color.FromRgb(
            (byte)Math.Min(255, c.R * scale),
            (byte)Math.Min(255, c.G * scale),
            (byte)Math.Min(255, c.B * scale));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
