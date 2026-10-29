using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AirdropCaller.App;

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
