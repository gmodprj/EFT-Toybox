using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AirdropCaller.App;

/// <summary>품목 한 줄. 표에서 고칠 수 있게 알림을 단다.</summary>
public class ItemRow : INotifyPropertyChanged
{
    private int _count = 1;
    private int? _stackSize;

    public string Tpl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int Count
    {
        get => _count;
        set { _count = Math.Max(1, value); Changed(nameof(Count)); }
    }

    public int? StackSize
    {
        get => _stackSize;
        set { _stackSize = value is > 0 ? value : null; Changed(nameof(StackSize)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Changed(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ItemSpec ToSpec() => new() { Tpl = Tpl, Count = Count, StackSize = StackSize };
}

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private readonly ServerClient? _client;
    private readonly DispatcherTimer _timer = new();
    private readonly ObservableCollection<ItemRow> _items = [];

    private string? _startupError;
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();

        _config = AppConfig.LoadOrCreate(out _startupError);
        Topmost = _config.AlwaysOnTop;
        ItemGrid.ItemsSource = _items;
        IconBox.SelectedIndex = 0;

        var sessionId = _config.ResolveSessionId(out var how);
        if (sessionId is null)
        {
            // 세션을 못 얻으면 아무것도 못 한다. 창은 띄우되 이유를 분명히 말한다.
            _startupError = $"세션 id 를 찾지 못했다 ({how}). "
                            + $"airdrop-caller.config.json 의 sptDir 를 맞추거나 sessionId 를 직접 적는다.";
            SetState("서버에 붙지 못했다", _startupError, false);
            return;
        }

        _client = new ServerClient(_config.SptUrl, sessionId);
        DetailLine.Text = $"세션 {sessionId} ({how})";

        _timer.Interval = TimeSpan.FromSeconds(Math.Clamp(_config.StatePollSeconds, 0.5, 10));
        _timer.Tick += async (_, _) => await RefreshStateAsync();
        _timer.Start();

        Loaded += async (_, _) =>
        {
            await LoadPresetsAsync();
            await LoadCategoriesAsync();
        };
    }

    // ── 상태 ───────────────────────────────────────────────────────────────

    private async Task RefreshStateAsync()
    {
        if (_client is null || _busy)
        {
            return;
        }

        try
        {
            var state = await _client.GetStateAsync(CancellationToken.None);
            if (state is null)
            {
                SetState("서버가 답하지 않는다", null, false);
                return;
            }

            var head = state switch
            {
                { GameConnected: false } => "게임이 안 켜져 있다",
                { InRaid: false } => "레이드 중이 아니다",
                _ => $"레이드 중 — {state.Map}",
            };

            var detail = state.InRaid
                ? $"보급 지점 {state.AvailablePoints}개"
                  + (state.CrateInFlight ? " · 상자가 하늘에 있다" : string.Empty)
                  + (state.CooldownSeconds > 0 ? $" · 쿨다운 {state.CooldownSeconds}초" : string.Empty)
                : state.BlockedBy;

            SetState(head, detail, state.CanQueue && _items.Count > 0);
            CancelButton.IsEnabled = state.HasPending;
        }
        catch (Exception e)
        {
            SetState("서버에 붙지 못했다", e.Message, false);
        }
    }

    private void SetState(string head, string? detail, bool canDrop)
    {
        StateLine.Text = head;
        if (detail is not null)
        {
            DetailLine.Text = detail;
        }

        DropButton.IsEnabled = canDrop && _client is not null;
    }

    // ── 프리셋 ─────────────────────────────────────────────────────────────

    private async Task LoadPresetsAsync()
    {
        if (_client is null)
        {
            return;
        }

        try
        {
            var list = await _client.ListPresetsAsync(CancellationToken.None);
            var selectedId = (PresetList.SelectedItem as Preset)?.Id;
            PresetList.ItemsSource = list?.Presets ?? [];
            if (selectedId is not null)
            {
                PresetList.SelectedItem = (list?.Presets ?? []).FirstOrDefault(p => p.Id == selectedId);
            }
        }
        catch (Exception e)
        {
            MessageLine.Text = "프리셋을 못 읽었다: " + e.Message;
        }
    }

    private void OnPresetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (PresetList.SelectedItem is not Preset preset)
        {
            return;
        }

        PresetNameBox.Text = preset.Name;
        IconBox.SelectedIndex = preset.Icon switch
        {
            "Supply" => 1, "Medical" => 2, "Weapon" => 3, _ => 0,
        };

        _items.Clear();
        foreach (var item in preset.Items)
        {
            _items.Add(new ItemRow { Tpl = item.Tpl, Name = item.Tpl, Count = item.Count, StackSize = item.StackSize });
        }

        UpdateFitHint();
    }

    private void OnNewPreset(object sender, RoutedEventArgs e)
    {
        PresetList.SelectedItem = null;
        PresetNameBox.Text = "새 프리셋";
        IconBox.SelectedIndex = 0;
        _items.Clear();
        UpdateFitHint();
    }

    private async void OnSavePreset(object sender, RoutedEventArgs e)
    {
        if (_client is null)
        {
            return;
        }

        var name = PresetNameBox.Text.Trim();
        if (name.Length == 0)
        {
            MessageLine.Text = "프리셋 이름이 비었다.";
            return;
        }

        var preset = new Preset
        {
            Id = (PresetList.SelectedItem as Preset)?.Id ?? MakeId(),
            Name = name,
            Icon = SelectedIcon(),
            Items = _items.Select(i => i.ToSpec()).ToList(),
        };

        await Guard(async () =>
        {
            var result = await _client.SavePresetAsync(preset, CancellationToken.None);
            MessageLine.Text = result?.Ok == true ? $"'{name}' 저장했다." : result?.Reason ?? "저장하지 못했다.";
            await LoadPresetsAsync();
        });
    }

    private async void OnDeletePreset(object sender, RoutedEventArgs e)
    {
        if (_client is null || PresetList.SelectedItem is not Preset preset)
        {
            return;
        }

        if (MessageBox.Show($"'{preset.Name}' 을 지울까?", "보급 호출기",
                MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
        {
            return;
        }

        await Guard(async () =>
        {
            var result = await _client.DeletePresetAsync(preset.Id, CancellationToken.None);
            MessageLine.Text = result?.Ok == true ? $"'{preset.Name}' 지웠다." : result?.Reason ?? "지우지 못했다.";
            OnNewPreset(sender, e);
            await LoadPresetsAsync();
        });
    }

    /// <summary>파일 이름이 되는 값이라 영숫자만 쓴다. 서버도 같은 규칙으로 막는다.</summary>
    private static string MakeId() => "p" + DateTime.Now.ToString("yyyyMMddHHmmss");

    private string SelectedIcon() => (IconBox.SelectedItem as ComboBoxItem)?.Content as string ?? "Common";

    // ── 검색 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// 분류 목록을 받아 콤보에 채운다. **레이드와 무관하다** — 레이드 밖에서 미리
    /// 프리셋을 짜 두는 것이 이 창의 본래 쓰임이다.
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        if (_client is null)
        {
            return;
        }

        try
        {
            var list = await _client.ListCategoriesAsync(CancellationToken.None);
            var all = new List<CategoryInfo> { new() { Id = string.Empty, Name = "(분류 고르기 — 또는 아래에서 이름으로 찾기)" } };
            all.AddRange(list?.Categories ?? []);
            CategoryBox.ItemsSource = all;
            CategoryBox.SelectedIndex = 0;
        }
        catch (Exception e)
        {
            MessageLine.Text = "분류를 못 읽었다: " + e.Message;
        }
    }

    private async void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryBox.SelectedItem is CategoryInfo { Id.Length: > 0 })
        {
            await RunSearchAsync();
        }
    }

    private async void OnSearch(object sender, RoutedEventArgs e) => await RunSearchAsync();

    private async void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await RunSearchAsync();
        }
    }

    private async Task RunSearchAsync()
    {
        if (_client is null)
        {
            return;
        }

        var category = (CategoryBox.SelectedItem as CategoryInfo)?.Id ?? string.Empty;
        var query = SearchBox.Text.Trim();

        if (query.Length == 0 && category.Length == 0)
        {
            SearchHint.Text = "  분류를 고르거나 이름을 적는다";
            return;
        }

        await Guard(async () =>
        {
            var found = await _client.SearchAsync(query, category, 400, CancellationToken.None);
            var items = found?.Items ?? [];

            // 분류별로 묶어 보여 준다. 2만 개를 한 줄씩 훑게 둘 수는 없다.
            var view = new CollectionViewSource { Source = items };
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FoundItem.Category)));
            SearchList.ItemsSource = view.View;

            SearchHint.Text = items.Count == 0
                ? "  없다"
                : found!.Truncated ? $"  {items.Count}개 — 더 있다. 좁혀 본다" : $"  {items.Count}개";
        });
    }

    private void OnAddItem(object sender, RoutedEventArgs e)
    {
        if (SearchList.SelectedItem is not FoundItem found)
        {
            return;
        }

        _items.Add(new ItemRow
        {
            Tpl = found.Tpl,
            Name = found.Name,
            Count = 1,
            // 묶이는 물건이면 가득 채운 한 칸이 기본값으로 쓸 만하다.
            StackSize = found.MaxStack > 1 ? found.MaxStack : null,
        });

        UpdateFitHint();
    }

    private void OnRemoveItem(object sender, RoutedEventArgs e)
    {
        if (ItemGrid.SelectedItem is ItemRow row)
        {
            _items.Remove(row);
            UpdateFitHint();
        }
    }

    private void UpdateFitHint()
    {
        var total = _items.Sum(i => i.Count);
        FitHint.Text = total == 0
            ? "  품목을 넣는다"
            : $"  품목 {_items.Count}종 {total}칸 — 상자에 안 들어가는 것은 서버가 알려 준다";
    }

    // ── 드롭 ───────────────────────────────────────────────────────────────

    private async void OnDrop(object sender, RoutedEventArgs e)
    {
        if (_client is null || _items.Count == 0)
        {
            return;
        }

        await Guard(async () =>
        {
            var result = await _client.QueueAsync(
                null, SelectedIcon(), _items.Select(i => i.ToSpec()).ToList(), CancellationToken.None);

            if (result?.Ok != true)
            {
                MessageLine.Text = result?.Reason ?? "예약하지 못했다.";
                return;
            }

            var text = $"예약했다 — {result.Placed}개를 담았다.";
            if (result.Skipped.Count > 0)
            {
                // 조용히 버리지 않는다. 무엇이 안 들어갔는지 그대로 말한다.
                text += $" 못 담은 것 {result.Skipped.Count}개: {string.Join(", ", result.Skipped)}";
            }

            MessageLine.Text = text;
        });
    }

    private async void OnCancel(object sender, RoutedEventArgs e)
    {
        if (_client is null)
        {
            return;
        }

        await Guard(async () =>
        {
            var result = await _client.CancelAsync(CancellationToken.None);
            MessageLine.Text = result?.Ok == true ? "예약을 취소했다." : result?.Reason ?? "취소하지 못했다.";
        });
    }

    /// <summary>
    /// 버튼을 두 번 눌러 겹치는 것을 막고, 터져도 창이 죽지 않게 한다.
    /// </summary>
    private async Task Guard(Func<Task> work)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        try
        {
            await work();
        }
        catch (Exception e)
        {
            MessageLine.Text = "서버 호출이 실패했다: " + e.Message;
        }
        finally
        {
            _busy = false;
        }
    }
}
