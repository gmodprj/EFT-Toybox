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
        ["Dogtag", "TacticalVest", "Backpack"],
        ["FirstPrimaryWeapon", "Holster"],
        ["SecondPrimaryWeapon", "Scabbard"],
        ["Pockets"],
    ];

    private readonly ServerClient _client;
    private readonly IconService? _icons;
    private readonly string _fileName;

    public FileDetailWindow(ServerClient client, IconService? icons, string fileName)
    {
        InitializeComponent();

        _client = client;
        _icons = icons;
        _fileName = fileName;

        HeaderText.Text = fileName;
        Title = $"내보낸 파일 보기 — {fileName}";

        _ = LoadAsync(null);
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

                var inside = await _client.DetailAsync(_fileName, node.Id, CancellationToken.None);
                item.Items.Clear();

                foreach (var child in inside?.Tree ?? [])
                {
                    item.Items.Add(MakeNode(child));
                }
            };
        }

        return item;
    }

    private async void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem { Tag: DetailNode node })
        {
            await LoadAsync(node.IsContainer ? node.Id : null);
        }
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
                foreach (var grid in view.Grids)
                {
                    ViewHost.Children.Add(Label($"{(string.IsNullOrEmpty(grid.GridName) ? grid.Name : grid.GridName)} — {grid.Rows.Count}개"));
                    ViewHost.Children.Add(Grid(grid.CellsH, grid.CellsV, grid.Rows));
                }

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

    private UIElement Grid(int cellsH, int cellsV, List<StashRow> rows)
    {
        var grid = new StashGridView
        {
            ReadOnly = true,
            Icons = _icons,
            Height = Math.Min(420, Math.Max(120, (cellsV + 1) * 46)),
            Margin = new Thickness(0, 0, 0, 10),
        };

        grid.Draw(cellsH, cellsV, rows);
        return grid;
    }

    /// <summary>인게임 장비 화면과 같은 자리에 칸을 놓는다(설계 §4.3).</summary>
    private UIElement LoadoutBoard(List<DetailSlotRow> slots)
    {
        var board = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

        foreach (var row in LoadoutRows)
        {
            var line = new StackPanel { Orientation = Orientation.Horizontal };

            foreach (var slotId in row)
            {
                var found = slots.FirstOrDefault(s => s.SlotId == slotId);
                line.Children.Add(SlotCard(slotId, found));
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

    private UIElement SlotCard(string slotId, DetailSlotRow? row)
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

        if (row is { Contained: > 0 })
        {
            text.Inlines.Add(new System.Windows.Documents.LineBreak());
            text.Inlines.Add(new System.Windows.Documents.Run($"안에 {row.Contained}개 — 왼쪽 나무에서 열어 보세요")
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA8)),
            });
        }

        return new Border
        {
            Width = 196,
            MinHeight = 64,
            Margin = new Thickness(0, 0, 6, 6),
            Padding = new Thickness(6),
            Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x55)),
            BorderThickness = new Thickness(1),
            Child = text,
        };
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
