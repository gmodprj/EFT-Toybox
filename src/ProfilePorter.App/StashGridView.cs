using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProfilePorter.App;

/// <summary>
/// 스태시를 **인게임과 같은 격자**로 그린다.
///
/// 목록으로 그리면 1칸짜리 탄약과 큰 리그가 똑같아 보여, 무엇이 얼마나 자리를 차지하는지 알 수
/// 없다. 게임 화면과 같은 모양이면 고르는 사람이 자기 스태시를 **그대로 알아본다.**
///
/// 고르는 법은 셋이다 — 그냥 누르면 **그것 하나**, `Ctrl`·`Shift` 를 누른 채면 **더하고 빼기**,
/// 빈 곳에서 **끌면 네모 안의 것 전부**.
/// </summary>
public sealed class StashGridView : ScrollViewer
{
    /// <summary>칸 하나의 픽셀 크기. 인게임 비율(정사각)과 같게 둔다.</summary>
    private const int Cell = 46;

    private readonly Canvas _canvas = new();
    private readonly List<(StashRow row, Border box)> _boxes = [];
    private readonly HashSet<string> _selected = new(StringComparer.Ordinal);

    private readonly Rectangle _lasso = new()
    {
        Stroke = new SolidColorBrush(Color.FromRgb(0xC8, 0xA0, 0x50)),
        StrokeThickness = 1,
        StrokeDashArray = [3, 2],
        Fill = new SolidColorBrush(Color.FromArgb(0x30, 0xC8, 0xA0, 0x50)),
        Visibility = Visibility.Collapsed,
    };

    private Point _dragFrom;
    private bool _dragging;

    public StashGridView()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30));
        BorderThickness = new Thickness(0);
        Content = _canvas;

        _canvas.Background = Brushes.Transparent; // 빈 곳에서도 마우스를 받으려면 칠이 있어야 한다
        _canvas.MouseLeftButtonDown += OnCanvasDown;
        _canvas.MouseMove += OnCanvasMove;
        _canvas.MouseLeftButtonUp += OnCanvasUp;
    }

    /// <summary>아이콘을 구해 오는 쪽. null 이면 이름만 그린다.</summary>
    public IconService? Icons { get; set; }

    /// <summary>고른 것이 바뀌면 알린다.</summary>
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
        // 무기에는 맞지만 삽입물로 커지는 방어구는 게임보다 한 칸 크게 나온다(설계 §16.2).
        foreach (var row in rows.OrderByDescending(r => Math.Max(1, r.W) * Math.Max(1, r.H)))
        {
            var box = MakeBox(row);
            Canvas.SetLeft(box, row.X * Cell + 1);
            Canvas.SetTop(box, row.Y * Cell + 1);
            _canvas.Children.Add(box);
            _boxes.Add((row, box));
        }

        _canvas.Children.Add(_lasso);
        _ = LoadIconsAsync();
    }

    /// <summary>아이콘은 **그린 뒤에** 채운다. 받는 동안 격자가 멈춰 있으면 안 된다.</summary>
    private async Task LoadIconsAsync()
    {
        if (Icons is null)
        {
            return;
        }

        foreach (var (row, box) in _boxes.ToList())
        {
            var icon = await Icons.GetAsync(row.Tpl, CancellationToken.None);
            if (icon is null || box.Child is not Grid grid)
            {
                continue;
            }

            grid.Children.Insert(0, new Image
            {
                Source = icon,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2),
                IsHitTestVisible = false,
            });
        }
    }

    private Border MakeBox(StashRow row)
    {
        // 아이콘이 깔리고 그 위에 글자가 얹힌다. 글자는 **아래쪽**에 둔다 — 아이콘의 생김새를
        // 가리지 않으면서 무엇인지 읽히게 하려는 것이다.
        var grid = new Grid();
        grid.Children.Add(new TextBlock
        {
            Text = row.GridLabel,
            Foreground = new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2)),
            FontSize = 10,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(2, 0, 2, 1),
            Background = new SolidColorBrush(Color.FromArgb(0xB0, 0x1E, 0x1E, 0x1E)),
            IsHitTestVisible = false,
        });

        var box = new Border
        {
            Width = Math.Max(1, row.W) * Cell - 1,
            Height = Math.Max(1, row.H) * Cell - 1,
            Background = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x42)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x5E)),
            BorderThickness = new Thickness(1),
            Child = grid,
            // 안에 든 것이 있으면 그 수를 마우스로 보여 준다 — 그것이 실제로 나가는 양이다.
            ToolTip = row.Contained > 0
                ? $"{row.Name}  (안에 {row.Contained}개 — 모두 따라갑니다)"
                : row.Name,
            Cursor = Cursors.Hand,
        };

        box.MouseLeftButtonUp += (_, e) =>
        {
            if (_dragging)
            {
                return; // 끌던 중이면 캔버스 쪽이 처리한다
            }

            Toggle(row.Id, Additive());
            e.Handled = true;
        };

        return box;
    }

    /// <summary>`Ctrl` 이나 `Shift` 를 누르고 있으면 **더하고 빼기**다.</summary>
    private static bool Additive() =>
        (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != ModifierKeys.None;

    private void OnCanvasDown(object sender, MouseButtonEventArgs e)
    {
        _dragFrom = e.GetPosition(_canvas);
        _dragging = true;
        _canvas.CaptureMouse();

        Canvas.SetLeft(_lasso, _dragFrom.X);
        Canvas.SetTop(_lasso, _dragFrom.Y);
        _lasso.Width = 0;
        _lasso.Height = 0;
        _lasso.Visibility = Visibility.Visible;
        Panel.SetZIndex(_lasso, 1000);
    }

    private void OnCanvasMove(object sender, MouseEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        var now = e.GetPosition(_canvas);
        Canvas.SetLeft(_lasso, Math.Min(_dragFrom.X, now.X));
        Canvas.SetTop(_lasso, Math.Min(_dragFrom.Y, now.Y));
        _lasso.Width = Math.Abs(now.X - _dragFrom.X);
        _lasso.Height = Math.Abs(now.Y - _dragFrom.Y);
    }

    private void OnCanvasUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        _canvas.ReleaseMouseCapture();
        _lasso.Visibility = Visibility.Collapsed;

        var now = e.GetPosition(_canvas);
        var box = new Rect(
            Math.Min(_dragFrom.X, now.X), Math.Min(_dragFrom.Y, now.Y),
            Math.Abs(now.X - _dragFrom.X), Math.Abs(now.Y - _dragFrom.Y));

        // 끈 거리가 거의 없으면 **빈 곳을 누른 것**이다 — 고른 것을 푼다.
        if (box.Width < 4 && box.Height < 4)
        {
            if (!Additive())
            {
                _selected.Clear();
                Repaint();
                SelectionChanged?.Invoke();
            }

            return;
        }

        if (!Additive())
        {
            _selected.Clear();
        }

        foreach (var (row, _) in _boxes)
        {
            var area = new Rect(
                row.X * Cell, row.Y * Cell,
                Math.Max(1, row.W) * Cell, Math.Max(1, row.H) * Cell);

            // 네모에 **닿기만 해도** 고른다. 완전히 품어야 한다면 큰 물건을 고르기 어렵다.
            if (box.IntersectsWith(area))
            {
                _selected.Add(row.Id);
            }
        }

        Repaint();
        SelectionChanged?.Invoke();
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
