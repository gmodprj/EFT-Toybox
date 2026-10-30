using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace AirdropCaller.App;

/// <summary>드롭다운 한 줄.</summary>
public class SlotChoice
{
    /// <summary>null = 자동에 맡긴다. 빈 문자열 = 비워 둔다. 그 밖 = 그 tpl 을 단다.</summary>
    public string? Tpl { get; init; }

    public string Label { get; init; } = string.Empty;

    /// <summary>이미 단 것과 같이 못 달면 false. 드롭다운에서 못 고르게 막는다.</summary>
    public bool Selectable { get; init; } = true;

    public string? Hint { get; init; }
}

/// <summary>칸 한 줄.</summary>
public class SlotRow : INotifyPropertyChanged
{
    private SlotChoice? _selected;

    public required string Path { get; init; }
    public required string Name { get; init; }

    /// <summary>
    /// 겹친 칸을 들여쓰는 여백. 헬멧은 마운트 안에 또 칸이 있다.
    ///
    /// 칸(column)을 따로 두지 않고 여백으로 민다 — 따로 두면 그 폭만큼 드롭다운이
    /// 줄마다 다른 자리에서 시작한다.
    /// </summary>
    public required Thickness Indent { get; init; }

    public required string KindLabel { get; init; }
    public required Brush KindBrush { get; init; }
    public required ObservableCollection<SlotChoice> Choices { get; init; }

    /// <summary>줄이 바뀌면 창이 서버에 다시 물어본다.</summary>
    public Action<SlotRow>? Picked { get; set; }

    public SlotChoice? Selected
    {
        get => _selected;
        set
        {
            if (ReferenceEquals(_selected, value))
            {
                return;
            }

            _selected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            Picked?.Invoke(this);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// 품목의 칸마다 무엇을 달지 직접 고르는 창.
///
/// **판단은 하나도 하지 않는다.** 어떤 칸이 있고 자동이 뭘 고르는지는 전부 서버에
/// 물어본다(`/airdrop-caller/slots`). 창이 "자동: A" 라고 적어 놓고 상자에 B 가
/// 들어 있는 것이 제일 나쁘다.
///
/// 한 칸을 바꿀 때마다 **다시 물어본다.** 겹친 칸은 윗칸에 무엇을 달았느냐에 따라
/// 아랫칸이 통째로 달라진다 — 헬멧의 마운트를 바꾸면 그 밑 야시경 칸도 바뀐다.
/// </summary>
public partial class SlotWindow : Window
{
    private readonly ServerClient _client;
    private readonly ItemRow _row;
    private readonly ObservableCollection<SlotRow> _rows = [];
    private readonly Dictionary<string, string> _picks;

    /// <summary>목록을 다시 그리는 동안에는 바뀜을 무시한다. 안 그러면 끝없이 돈다.</summary>
    private bool _filling;

    public SlotWindow(ServerClient client, ItemRow row)
    {
        InitializeComponent();
        _client = client;
        _row = row;
        _picks = new Dictionary<string, string>(row.Slots);

        SlotList.ItemsSource = _rows;
        HeadLine.Text = row.Name;
        SubLine.Text = "불러오는 중…";

        Loaded += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        SlotListResponse? answer;
        try
        {
            answer = await _client.SlotsAsync(_row.Tpl, FillCode(), _picks, CancellationToken.None);
        }
        catch (Exception e)
        {
            SubLine.Text = "서버에 물어보지 못했다: " + e.Message;
            return;
        }

        if (answer is null || answer.Reason is not null)
        {
            SubLine.Text = answer?.Reason ?? "서버가 답하지 않았다.";
            return;
        }

        _filling = true;
        try
        {
            _rows.Clear();
            foreach (var node in answer.Slots)
            {
                _rows.Add(Build(node));
            }
        }
        finally
        {
            _filling = false;
        }

        HeadLine.Text = answer.Name;
        SubLine.Text = _rows.Count == 0
            ? "이 품목에는 칸이 없다."
            : $"칸 {_rows.Count}개 — 직접 고른 것 {_picks.Count}개. "
              + $"채우기가 '{_row.FillLabel}' 이라, 안 고른 칸은 그 규칙대로 간다."
              + (_row.Count > 1 ? $" 개수가 {_row.Count}개라 {_row.Count}개 모두 같아진다." : string.Empty);
    }

    private string FillCode() => _row.FillLabel switch
    {
        ItemRow.FillAll => "All",
        ItemRow.FillNone => "None",
        _ => "Auto",
    };

    private SlotRow Build(SlotNode node)
    {
        var choices = new ObservableCollection<SlotChoice>
        {
            // **`auto` 가 아니라 `chosen` 을 적는다.**
            //
            // `auto` 는 "이 칸을 채운다면 무엇을 고를까" 고, `chosen` 은 "실제로 무엇이
            // 들어가는가" 다. 둘은 자주 다르다 — `자동` 은 방어구의 삽입물·방어판과
            // 필수 칸만 채우므로, 헬멧의 야시경·마운트 칸은 **비워 둔다.**
            //
            // 그런데 `auto` 를 적는 바람에 창이 "자동 — Peltor ComTac VI 헤드셋" 이라고
            // 말해 놓고 상자에는 방탄판만 들어 있었다. 또 같은 거짓말을 했다(2026-10-30).
            new()
            {
                Tpl = null,
                Label = node.Chosen.Length > 0
                    ? "자동 — " + NameOf(node, node.Chosen)
                    : "자동 — 안 채운다",
                Hint = node.Chosen.Length > 0
                    ? null
                    : node.Auto.Length > 0
                        ? $"'자동' 은 이 칸을 비워 둔다. 채우려면 아래에서 고른다 "
                          + $"(가장 비싼 것은 {NameOf(node, node.Auto)})."
                        : "이 칸에 달 수 있는 것이 없다.",
            },
            new() { Tpl = string.Empty, Label = "비움" },
        };

        foreach (var candidate in node.Candidates)
        {
            choices.Add(new SlotChoice
            {
                Tpl = candidate.Tpl,
                Label = Label(candidate),
                Selectable = !candidate.Blocked,
                Hint = candidate.Blocked ? "이미 단 것과 같이 달 수 없다." : null,
            });
        }

        var row = new SlotRow
        {
            Path = node.Path,
            Name = node.Name,
            Indent = new Thickness(node.Depth * 18.0, 0, 0, 0),
            KindLabel = node.Kind switch
            {
                "plate" => "방탄판",
                "soft" => "삽입물",
                _ => node.Required ? "필수" : "장비",
            },
            KindBrush = node.Kind switch
            {
                "plate" => Brush("Accent"),
                "soft" => Brush("Dim"),
                _ => Brush("Fg"),
            },
            Choices = choices,
        };

        // 지금 상태에 맞는 줄을 고른다. 직접 고른 칸이 아니면 늘 "자동" 이다.
        row.Selected = !node.Picked
            ? choices[0]
            : choices.FirstOrDefault(c => c.Tpl == (node.Chosen.Length == 0 ? string.Empty : node.Chosen))
              ?? choices[1];

        row.Picked = OnRowPicked;
        return row;
    }

    private static string Label(SlotCandidate candidate)
    {
        var bits = candidate.Name;
        if (candidate.ArmorClass > 0)
        {
            bits += $"  [{candidate.ArmorClass}등급]";
        }

        if (candidate.Price > 0)
        {
            bits += $"  {candidate.Price:N0}원";
        }

        return candidate.Blocked ? bits + "  (같이 못 단다)" : bits;
    }

    private static string NameOf(SlotNode node, string tpl) =>
        node.Candidates.FirstOrDefault(c => c.Tpl == tpl)?.Name ?? tpl;

    private Brush Brush(string key) => (Brush)FindResource(key);

    private async void OnRowPicked(SlotRow row)
    {
        if (_filling)
        {
            return;
        }

        switch (row.Selected?.Tpl)
        {
            case null:
                // 자동에 돌려준다 — 적지 않은 것과 같은 뜻이다.
                _picks.Remove(row.Path);
                break;

            case "":
                // **빈 문자열이 "비워 둬라" 다.** 자동이 채우는 것을 막는 유일한 방법이라
                // 지워서는 안 된다.
                _picks[row.Path] = string.Empty;
                break;

            case { } tpl:
                _picks[row.Path] = tpl;
                break;
        }

        // 윗칸을 바꾸면 아랫칸이 통째로 달라진다. 서버가 다시 훑게 한다.
        await ReloadAsync();
    }

    private async void OnReset(object sender, RoutedEventArgs e)
    {
        _picks.Clear();
        await ReloadAsync();
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        _row.Slots = new Dictionary<string, string>(_picks);
        _row.SlotsChanged();
        DialogResult = true;
    }
}
