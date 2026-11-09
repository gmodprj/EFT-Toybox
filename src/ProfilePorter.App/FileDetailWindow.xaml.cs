using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfilePorter.App;

/// <summary>
/// 내보낸 파일 하나를 **되짚어 보는** 창(설계 §2.2).
///
/// 왼쪽은 나무, 오른쪽은 그 자리의 모습이다. 스태시 파일은 바닥 격자, 착용장비 파일은 인게임
/// 장비 판으로 시작하고, 상자를 고르면 **그 안**이 같은 격자 방식으로 펼쳐진다.
///
/// **읽기 전용이다.** 고르기·보내기·지우기를 두지 않는다. 격자는 본 창과 **같은**
/// <see cref="StashGridView"/> 를 <see cref="StashGridView.ReadOnly"/> 로 쓴다 — 베껴 만들면
/// 나중에 한쪽만 고쳐진다.
/// </summary>
public partial class FileDetailWindow : Window
{
    /// <summary>인게임 장비 화면의 자리(설계 §4.3). 줄마다 왼쪽에서 오른쪽으로 놓는다.</summary>
    private static readonly string[][] LoadoutRows =
    [
        ["Earpiece", "Headwear", "FaceCover"],
        ["ArmBand", "ArmorVest", "Eyewear"],
        ["Dogtag"],
        ["FirstPrimaryWeapon", "Holster"],
        ["SecondPrimaryWeapon", "Scabbard"],
        ["Backpack", "TacticalVest", "Pockets"],
    ];

    private readonly ServerClient _client;
    private readonly IconService? _icons;
    private readonly string _fileName;

    public FileDetailWindow(ServerClient client, IconService? icons, string fileName, string? startAt = null)
    {
        InitializeComponent();

        _client = client;
        _icons = icons;
        _fileName = fileName;

        HeaderText.Text = fileName;
        Title = $"내보낸 파일 보기 — {fileName}";

        // `startAt` 은 개발용 스크린샷(`--shot`)이 상자 안을 바로 찍을 때만 쓴다.
        _ = LoadAsync(startAt);
    }

    /// <summary>그 자리를 서버에 묻고 그린다. <paramref name="containerId"/> 가 null 이면 뿌리.</summary>
    private async Task LoadAsync(string? containerId)
    {
        try
        {
            StatusText.Text = "읽는 중…";
            var detail = await _client.DetailAsync(_fileName, containerId, CancellationToken.None);

            if (detail is null || !detail.Ok)
            {
                StatusText.Text = detail?.Reason ?? "세부 내용을 읽지 못했습니다.";
                StatusText.Foreground = Brushes.IndianRed;
                return;
            }

            StatusText.Foreground = Brushes.White;
            ShowNotices(detail.Notices);
            FillTree(detail.Tree, containerId);
            DrawView(detail);
        }
        catch (Exception e)
        {
            StatusText.Text = $"세부 내용을 읽지 못했습니다 — {e.Message}";
            StatusText.Foreground = Brushes.IndianRed;
        }
    }

    private void ShowNotices(IReadOnlyList<string> notices)
    {
        if (notices.Count == 0)
        {
            NoticeBox.Visibility = Visibility.Collapsed;
            return;
        }

        NoticeText.Text = string.Join("\n", notices);
        NoticeBox.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 나무를 한 겹 채운다. 상자 줄에는 **가짜 자식**을 하나 넣어 펼침 표시가 뜨게 하고,
    /// 실제로 펼칠 때 그 자리를 서버에 묻는다 — 1,361개짜리를 한 번에 올리지 않기 위해서다.
    /// </summary>
    private void FillTree(IReadOnlyList<DetailNode> nodes, string? containerId)
    {
        if (containerId is not null)
        {
            // 안으로 들어간 것은 오른쪽만 바꾼다. 나무는 그대로 둔다.
            return;
        }

        Tree.Items.Clear();
        foreach (var node in nodes)
        {
            Tree.Items.Add(MakeNode(node));
        }
    }

    private TreeViewItem MakeNode(DetailNode node)
    {
        var item = new TreeViewItem
        {
            Header = node.Display,
            Tag = node,
            Foreground = Brushes.White,
        };

        if (node.IsContainer)
        {
            item.Items.Add("…");
            item.Expanded += async (_, e) =>
            {
                e.Handled = true;
                if (item.Items.Count != 1 || item.Items[0] is not string) return;

                // **`async void` 는 던지면 앱을 죽인다.** 서버가 꺼져 있거나 500 이면
                // `EnsureSuccessStatusCode` 가 던지고, 그 예외는 WPF 디스패처로 올라가
                // 프로세스를 끝낸다. 여기서 받아 **창에만** 적는다.
                try
                {
                    var inside = await _client.DetailAsync(_fileName, node.Id, CancellationToken.None);
                    if (inside is null || !inside.Ok)
                    {
                        StatusText.Text = inside?.Reason ?? "그 안을 읽지 못했습니다.";
                        StatusText.Foreground = Brushes.IndianRed;
                        return;
                    }

                    item.Items.Clear();
                    foreach (var child in inside.Tree)
                    {
                        item.Items.Add(MakeNode(child));
                    }
                }
                catch (Exception ex)
                {
                    // 가짜 자식을 그대로 둔다 — 다시 펼쳐 볼 수 있어야 한다.
                    StatusText.Text = $"그 안을 읽지 못했습니다 — {ex.Message}";
                    StatusText.Foreground = Brushes.IndianRed;
                }
            };
        }

        return item;
    }

    private async void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not TreeViewItem { Tag: DetailNode node })
        {
            return;
        }

        if (!node.IsContainer)
        {
            // **아무것도 다시 읽지 않는다.** 뿌리를 다시 읽으면 나무가 통째로 다시 지어져
            // 펼쳐 둔 것이 전부 접힌다 — 안쪽 아이템을 한 번 누른 사람이 처음으로 되돌아간다.
            StatusText.Text = node.Display;
            StatusText.Foreground = Brushes.White;
            return;
        }

        await LoadAsync(node.Id);
    }

    private void DrawView(DetailResponse detail)
    {
        ViewHost.Children.Clear();
        var view = detail.View;
        if (view is null) return;

        switch (view.Kind)
        {
            case "stash":
                ViewHost.Children.Add(Label($"스태시 바닥 — {view.Rows.Count}개"));
                ViewHost.Children.Add(Grid(view.CellsH, view.CellsV, view.Rows));
                break;

            case "loadout":
                ViewHost.Children.Add(Label("입고 있던 한 벌"));
                ViewHost.Children.Add(LoadoutBoard(view.Slots));
                break;

            default:
                // **격자들을 가로로 흘린다.** 리그 하나에 격자가 열한 개다 — 세로로만 쌓으면
                // 한 주머니를 보려고 한참 굴려야 한다.
                var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
                var n = 0;

                foreach (var grid in view.Grids)
                {
                    n++;
                    var block = new StackPanel { Margin = new Thickness(0, 0, 10, 10) };
                    block.Children.Add(Label($"{GridTitle(grid, n)} — {grid.Rows.Count}개"));
                    block.Children.Add(Grid(grid.CellsH, grid.CellsV, grid.Rows));
                    wrap.Children.Add(block);
                }

                ViewHost.Children.Add(wrap);

                if (view.Slots.Count > 0)
                {
                    ViewHost.Children.Add(Label("꽂혀 있는 것"));
                    ViewHost.Children.Add(Rows(view.Slots, slotFirst: true));
                }

                break;
        }

        if (view.Loose.Count > 0)
        {
            ViewHost.Children.Add(Label($"자리를 잡지 못한 것 — {view.Loose.Count}개"));
            ViewHost.Children.Add(Rows(view.Loose, slotFirst: false));
        }

        StatusText.Text = detail.ContainerId is null
            ? "왼쪽 나무에서 상자를 고르면 그 안이 보입니다. 이 창은 읽기만 합니다."
            : "안쪽을 보고 있습니다. 왼쪽 나무로 다른 자리를 고를 수 있습니다.";
    }

    /// <summary>
    /// 격자 하나. **칸 수에 꼭 맞춘다** — 높이를 고정하면 2x3 짜리 주머니가 420px 짜리 빈 판을
    /// 차지하고 자기 스크롤바까지 달고 나온다(2026-11-09 실측 화면에서 그렇게 나왔다).
    /// 굴리는 것은 바깥 하나로 족하다.
    /// </summary>
    private UIElement Grid(int cellsH, int cellsV, List<StashRow> rows)
    {
        var grid = new StashGridView
        {
            ReadOnly = true,
            Icons = _icons,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 4),
        };

        if (cellsH > 0 && cellsV > 0)
        {
            grid.Width = cellsH * StashGridView.CellSize + 2;
            grid.Height = cellsV * StashGridView.CellSize + 2;
        }

        grid.Draw(cellsH, cellsV, rows);
        return grid;
    }

    /// <summary>
    /// 격자 이름. DB 가 주는 이름은 `GridView (1)` 같은 **인게임 표시 이름**이라 한국어 화면에서
    /// 겉돈다. 아는 이름(`main`)은 사전이 옮기고, 모르는 것은 **몇 번째 칸인지**로 적는다.
    /// </summary>
    private static string GridTitle(DetailGridRow grid, int ordinal)
    {
        if (!string.IsNullOrEmpty(grid.GridName) && grid.GridName != grid.Name)
        {
            return grid.GridName;
        }

        return $"칸 {ordinal}";
    }

    /// <summary>
    /// 인게임 장비 화면과 같은 자리에 칸을 놓는다(설계 §4.3).
    ///
    /// 세 칸짜리 줄을 쌓되, **무기 두 자루는 두 칸을 차지한다** — 게임에서도 그 자리가 넓다.
    /// </summary>
    private UIElement LoadoutBoard(List<DetailSlotRow> slots)
    {
        var board = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

        foreach (var row in LoadoutRows)
        {
            var line = new StackPanel { Orientation = Orientation.Horizontal };

            foreach (var slotId in row)
            {
                var found = slots.FirstOrDefault(s => s.SlotId == slotId);
                var wide = row.Length == 2 && slotId == row[0];
                line.Children.Add(SlotCard(slotId, found, wide));
            }

            board.Children.Add(line);
        }

        // 판에 자리가 없는 슬롯도 **버리지 않는다** — 모드가 더한 자리일 수 있다.
        var extra = slots.Where(s => !LoadoutRows.SelectMany(r => r).Contains(s.SlotId)).ToList();
        if (extra.Count > 0)
        {
            board.Children.Add(Label("그 밖의 자리"));
            board.Children.Add(Rows(extra, slotFirst: true));
        }

        return board;
    }

    private UIElement SlotCard(string slotId, DetailSlotRow? row, bool wide = false)
    {
        var text = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.White,
            FontSize = 12,
        };

        text.Inlines.Add(new System.Windows.Documents.Run(SlotTitle(slotId))
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xA0, 0x50)),
        });

        text.Inlines.Add(new System.Windows.Documents.LineBreak());
        text.Inlines.Add(row is null ? "(비어 있음)" : row.Name);

        if (row is not null && !string.IsNullOrWhiteSpace(row.Summary))
        {
            text.Inlines.Add(new System.Windows.Documents.LineBreak());
            text.Inlines.Add(new System.Windows.Documents.Run(row.Summary)
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA8)),
            });
        }

        var hasInside = row is { Contained: > 0 };

        if (hasInside)
        {
            text.Inlines.Add(new System.Windows.Documents.LineBreak());
            text.Inlines.Add(new System.Windows.Documents.Run($"안에 {row!.Contained}개 — 눌러서 열기")
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xA0, 0x50)),
            });
        }

        // **그림을 넣는다.** 글자만 있으면 인게임 장비 화면과 전혀 달라 보인다(사용자 제보).
        var body = new DockPanel { LastChildFill = true };
        var picture = new Image
        {
            Width = 64,
            Height = 64,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };

        DockPanel.SetDock(picture, Dock.Left);
        body.Children.Add(picture);
        body.Children.Add(text);

        if (row is not null && _icons is not null)
        {
            // 그린 뒤에 채운다 — 받는 동안 판이 멈춰 있으면 안 된다.
            _ = Task.Run(async () =>
            {
                var icon = await _icons.GetAsync(row.Tpl, CancellationToken.None);
                if (icon is not null)
                {
                    await Dispatcher.InvokeAsync(() => picture.Source = icon);
                }
            });
        }

        var card = new Border
        {
            Width = wide ? 398 : 196,
            MinHeight = 84,
            Margin = new Thickness(0, 0, 6, 6),
            Padding = new Thickness(6),
            Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)),
            BorderBrush = new SolidColorBrush(hasInside
                ? Color.FromRgb(0xC8, 0xA0, 0x50)
                : Color.FromRgb(0x50, 0x50, 0x55)),
            BorderThickness = new Thickness(1),
            Child = body,
        };

        // **안이 있는 칸은 눌린다**(설계 §4.3). 나무로도 갈 수 있지만, 장비 화면에서 리그를
        // 눌러 안을 보는 것이 사람이 먼저 하는 동작이다.
        if (hasInside)
        {
            card.Cursor = System.Windows.Input.Cursors.Hand;
            card.MouseLeftButtonUp += async (_, _) => await LoadAsync(row!.Id);
        }

        return card;
    }

    private static string SlotTitle(string slotId) => slotId switch
    {
        "Earpiece" => "헤드셋",
        "Headwear" => "모자",
        "FaceCover" => "얼굴",
        "ArmBand" => "완장",
        "ArmorVest" => "방탄복",
        "Eyewear" => "안경",
        "Dogtag" => "인식표",
        "TacticalVest" => "리그",
        "Backpack" => "가방",
        "FirstPrimaryWeapon" => "주무기 1",
        "SecondPrimaryWeapon" => "주무기 2",
        "Holster" => "권총집",
        "Scabbard" => "칼집",
        "Pockets" => "주머니",
        _ => slotId,
    };

    private static UIElement Rows(List<DetailSlotRow> rows, bool slotFirst)
    {
        var panel = new StackPanel { Margin = new Thickness(2, 0, 0, 10) };

        foreach (var row in rows)
        {
            panel.Children.Add(new TextBlock
            {
                Text = slotFirst && !string.IsNullOrEmpty(row.SlotName)
                    ? $"{row.SlotName} — {row.Display}"
                    : row.Display,
                Foreground = Brushes.White,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 2),
            });
        }

        return panel;
    }

    private static UIElement Label(string text) => new TextBlock
    {
        Text = text,
        FontWeight = FontWeights.Bold,
        Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xA0, 0x50)),
        Margin = new Thickness(0, 4, 0, 4),
    };
}
