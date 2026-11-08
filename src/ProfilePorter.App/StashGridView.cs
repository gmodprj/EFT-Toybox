using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProfilePorter.App;

/// <summary>
/// 스태시를 **인게임과 같은 격자**로 그린다.
///
/// 목록으로 그리면 "리그 ×1" 한 줄이 1칸짜리 탄약과 똑같아 보여, 무엇이 얼마나 자리를 차지하는지
/// 알 수 없다. 게임 화면과 같은 모양이면 고르는 사람이 자기 스태시를 **그대로 알아본다.**
///
/// 아이콘은 쓰지 않는다 — 받아 오려면 밖으로 나가는 통신이 필요하고(AirdropCaller 가 그 길을
/// 열어 두었지만 여기서는 범위 밖이다), 이름과 개수만으로도 칸은 알아볼 수 있다.
/// </summary>
public sealed class StashGridView : ScrollViewer
{
    /// <summary>칸 하나의 픽셀 크기. 인게임 비율(정사각)과 같게 둔다.</summary>
    private const int Cell = 46;

    private readonly Canvas _canvas = new();
    private readonly List<(StashRow row, Border box)> _boxes = [];
    private readonly HashSet<string> _selected = new(StringComparer.Ordinal);

    public StashGridView()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30));
        BorderThickness = new Thickness(0);
        Content = _canvas;
    }

    /// <summary>고른 것이 바뀌면 알린다. 창이 상태줄을 고치고 내보내기에 쓴다.</summary>
    public event Action? SelectionChanged;

    public IReadOnlyList<StashRow> SelectedRows =>
        _boxes.Where(b => _selected.Contains(b.row.Id)).Select(b => b.row).ToList();

    public void Clear()
    {
        _canvas.Children.Clear();
        _boxes.Clear();
        _selected.Clear();
        SelectionChanged?.Invoke();
    }

    /// <summary>격자와 아이템을 그린다. 이미 그려진 것은 지운다.</summary>
    public void Draw(int cellsH, int cellsV, IReadOnlyList<StashRow> rows)
    {
        Clear();

        if (cellsH <= 0 || cellsV <= 0)
        {
            // 칸 수를 모르면 **그리지 않는다.** 지어낸 크기로 그리면 자리가 거짓이 된다.
            _canvas.Children.Add(new TextBlock
            {
                Text = "스태시 격자 크기를 서버가 주지 않았습니다.",
                Foreground = Brushes.IndianRed,
                Margin = new Thickness(8),
            });
            return;
        }

        _canvas.Width = cellsH * Cell + 1;
        _canvas.Height = cellsV * Cell + 1;

        var line = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3C));
        for (var x = 0; x <= cellsH; x++)
        {
            _canvas.Children.Add(new Line
            {
                X1 = x * Cell, Y1 = 0, X2 = x * Cell, Y2 = cellsV * Cell,
                Stroke = line, StrokeThickness = 1,
            });
        }

        for (var y = 0; y <= cellsV; y++)
        {
            _canvas.Children.Add(new Line
            {
                X1 = 0, Y1 = y * Cell, X2 = cellsH * Cell, Y2 = y * Cell,
                Stroke = line, StrokeThickness = 1,
            });
        }

        // **큰 것부터 그린다** — 나중에 그린 것이 위로 온다. 칸이 겹치면 작은 것이 큰 것에
        // 가려져 **누를 수 없게** 되는데, 이 순서면 작은 것이 늘 위에 남는다.
        //
        // 겹침은 실제로 생긴다: 서버가 주는 크기(`GetItemSize`)는 부착물까지 센 값이라
        // 무기에는 맞지만 플레이트 캐리어처럼 **삽입물로 커지는** 것은 게임이 그리는 것보다
        // 한 칸 크게 나온다(2026-11-08 실측: Siege-R 기본 4x4 → 받은 4x5).
        foreach (var row in rows.OrderByDescending(r => Math.Max(1, r.W) * Math.Max(1, r.H)))
        {
            var box = MakeBox(row);
            Canvas.SetLeft(box, row.X * Cell + 1);
            Canvas.SetTop(box, row.Y * Cell + 1);
            _canvas.Children.Add(box);
            _boxes.Add((row, box));
        }
    }

    private Border MakeBox(StashRow row)
    {
        var label = new TextBlock
        {
            Text = row.GridLabel,
            Foreground = new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2)),
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(3, 2, 3, 2),
        };

        var box = new Border
        {
            Width = Math.Max(1, row.W) * Cell - 1,
            Height = Math.Max(1, row.H) * Cell - 1,
            Background = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x42)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x5E)),
            BorderThickness = new Thickness(1),
            Child = label,
            // 안에 든 것이 있으면 그 수를 마우스로 보여 준다 — 그것이 실제로 나가는 양이다.
            ToolTip = row.Contained > 0
                ? $"{row.Name}  (안에 {row.Contained}개 — 모두 따라갑니다)"
                : row.Name,
            Cursor = Cursors.Hand,
        };

        box.MouseLeftButtonUp += (_, e) =>
        {
            // 컨트롤을 누른 채 클릭하면 **더하고 뺀다**. 그냥 클릭하면 그것 하나만 고른다 —
            // 인게임 격자와 같은 손놀림이다.
            var additive = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            Toggle(row.Id, additive);
            e.Handled = true;
        };

        return box;
    }

    private void Toggle(string id, bool additive)
    {
        if (!additive)
        {
            var only = !_selected.Contains(id) || _selected.Count > 1;
            _selected.Clear();
            if (only)
            {
                _selected.Add(id);
            }
        }
        else if (!_selected.Add(id))
        {
            _selected.Remove(id);
        }

        Repaint();
        SelectionChanged?.Invoke();
    }

    private void Repaint()
    {
        foreach (var (row, box) in _boxes)
        {
            var on = _selected.Contains(row.Id);
            box.Background = new SolidColorBrush(on
                ? Color.FromRgb(0x5A, 0x4A, 0x2A)
                : Color.FromRgb(0x3A, 0x3A, 0x42));
            box.BorderBrush = new SolidColorBrush(on
                ? Color.FromRgb(0xC8, 0xA0, 0x50)
                : Color.FromRgb(0x55, 0x55, 0x5E));
            box.BorderThickness = new Thickness(on ? 2 : 1);
        }
    }
}
