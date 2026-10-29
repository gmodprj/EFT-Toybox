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

    /// <summary>
    /// 분류·프리셋을 한 번이라도 받아 왔나. **창을 먼저 띄우고 서버를 켜는 순서**도
    /// 흔하고, 서버는 뜨는 데 2분 넘게 걸린다. 처음 한 번만 받고 말면 그 경우 목록이
    /// 영영 비어 있다 — 받을 때까지 상태 확인 때마다 다시 시도한다.
    /// </summary>
    private bool _listsLoaded;

    /// <summary>글자를 칠 때마다 서버를 때리지 않도록 멈춘 뒤 한 번만 보낸다.</summary>
    private readonly DispatcherTimer _searchDebounce = new() { Interval = TimeSpan.FromMilliseconds(250) };

    public MainWindow()
    {
        InitializeComponent();

        _config = AppConfig.LoadOrCreate(out _startupError);
        Topmost = _config.AlwaysOnTop;
        TopmostBox.IsChecked = _config.AlwaysOnTop;
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

        _searchDebounce.Tick += async (_, _) =>
        {
            _searchDebounce.Stop();
            await RunSearchAsync();
        };

        Loaded += async (_, _) => await LoadListsAsync();
    }

    /// <summary>분류와 프리셋을 받아 온다. 성공해야 <see cref="_listsLoaded"/> 가 선다.</summary>
    private async Task LoadListsAsync()
    {
        if (_client is null)
        {
            return;
        }

        try
        {
            var categories = await _client.ListCategoriesAsync(CancellationToken.None);
            var presets = await _client.ListPresetsAsync(CancellationToken.None);
            if (categories is null)
            {
                return;
            }

            var all = new List<CategoryInfo>
            {
                new() { Id = FavoriteCategoryId, Name = "★ 즐겨찾기", Count = _config.Favorites.Count },
                new() { Id = string.Empty, Name = "(전체 — 이름으로 찾기)" },
            };
            all.AddRange(categories.Categories);
            CategoryBox.ItemsSource = all;

            var selectedId = (PresetList.SelectedItem as Preset)?.Id;
            PresetList.ItemsSource = presets?.Presets ?? [];
            if (selectedId is not null)
            {
                PresetList.SelectedItem = (presets?.Presets ?? []).FirstOrDefault(p => p.Id == selectedId);
            }

            _listsLoaded = true;

            // 열자마자 목록이 비어 있으면 무엇을 해야 하는지 알 수 없다.
            // 첫 분류를 골라 두어 바로 뭔가 보이게 한다.
            if (CategoryBox.SelectedIndex < 0)
            {
                // 즐겨찾기가 있으면 거기부터. 없으면 첫 진짜 분류.
                CategoryBox.SelectedIndex = _config.Favorites.Count > 0 ? 0 : Math.Min(2, all.Count - 1);
            }
        }
        catch (Exception e)
        {
            // 서버가 아직 안 떴을 수 있다. 다음 상태 확인 때 다시 온다.
            MessageLine.Text = "목록을 받지 못했다 — 서버가 뜨면 다시 받는다. " + e.Message;
        }
    }

    private async void OnReload(object sender, RoutedEventArgs e)
    {
        _listsLoaded = false;
        MessageLine.Text = "목록을 다시 받는다…";
        await LoadListsAsync();
        if (_listsLoaded)
        {
            MessageLine.Text = "목록을 다시 받았다.";
        }
    }

    private void OnTopmostChanged(object sender, RoutedEventArgs e)
    {
        Topmost = TopmostBox.IsChecked == true;
        _config.AlwaysOnTop = Topmost;
        _config.Save();
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

            // 여기까지 왔으면 서버는 붙었다. 목록을 아직 못 받았으면 지금 받는다.
            if (!_listsLoaded)
            {
                await LoadListsAsync();
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

    /// <summary>
    /// 고른 프리셋을 그대로 베껴 새것으로 만든다. 저장을 눌러야 서버에 올라간다 —
    /// 베끼자마자 이름을 고치는 것이 보통이라 한 번 더 묻지 않는다.
    /// </summary>
    private void OnCopyPreset(object sender, RoutedEventArgs e)
    {
        if (PresetList.SelectedItem is not Preset preset)
        {
            MessageLine.Text = "베낄 프리셋을 먼저 고른다.";
            return;
        }

        PresetList.SelectedItem = null;
        PresetNameBox.Text = preset.Name + " 사본";
        IconBox.SelectedIndex = preset.Icon switch { "Supply" => 1, "Medical" => 2, "Weapon" => 3, _ => 0 };

        _items.Clear();
        foreach (var item in preset.Items)
        {
            _items.Add(new ItemRow { Tpl = item.Tpl, Name = item.Tpl, Count = item.Count, StackSize = item.StackSize });
        }

        UpdateFitHint();
        MessageLine.Text = "베꼈다. 이름을 고치고 저장을 누른다.";
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
            await LoadListsAsync();
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
            await LoadListsAsync();
        });
    }

    /// <summary>파일 이름이 되는 값이라 영숫자만 쓴다. 서버도 같은 규칙으로 막는다.</summary>
    private static string MakeId() => "p" + DateTime.Now.ToString("yyyyMMddHHmmss");

    private string SelectedIcon() => (IconBox.SelectedItem as ComboBoxItem)?.Content as string ?? "Common";

    // ── 검색 ───────────────────────────────────────────────────────────────

    private async void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryBox.SelectedItem is CategoryInfo { Id.Length: > 0 })
        {
            await RunSearchAsync();
        }
    }

    /// <summary>
    /// 글자를 칠 때마다 서버를 때리면 과하다. 멈춘 뒤 잠깐 기다렸다 한 번만 보낸다.
    /// </summary>
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounce.Stop();
        _searchDebounce.Start();
    }

    /// <summary>
    /// 즐겨찾기 토글. 고른 것이 목록에 있으면 빼고 없으면 넣는다.
    /// ★ 분류를 보고 있었다면 바로 다시 그린다.
    /// </summary>
    private async void OnToggleFavorite(object sender, RoutedEventArgs e)
    {
        if (SearchList.SelectedItem is not FoundItem found)
        {
            MessageLine.Text = "즐겨찾기할 아이템을 먼저 고른다.";
            return;
        }

        if (!_config.Favorites.Remove(found.Tpl))
        {
            _config.Favorites.Add(found.Tpl);
        }

        _config.Save();
        await RunSearchAsync();
    }

    /// <summary>즐겨찾기 묶음을 가리키는 가짜 분류 id. 서버는 이 값을 모른다.</summary>
    private const string FavoriteCategoryId = "::favorites";

    private async Task RunSearchAsync()
    {
        if (_client is null)
        {
            return;
        }

        var category = (CategoryBox.SelectedItem as CategoryInfo)?.Id ?? string.Empty;
        var query = SearchBox.Text.Trim();

        // 즐겨찾기는 서버에 없다. 찜한 tpl 로 직접 찾아 모은다.
        if (category == FavoriteCategoryId)
        {
            await ShowFavoritesAsync(query);
            return;
        }

        if (query.Length == 0 && category.Length == 0)
        {
            SearchHint.Text = "  분류를 고르거나 이름을 적는다";
            return;
        }

        await Guard(async () =>
        {
            var found = await _client.SearchAsync(query, category, 400, CancellationToken.None);
            var items = found?.Items ?? [];

            Show(items, found!.Truncated);
        });
    }

    private async Task ShowFavoritesAsync(string query)
    {
        if (_config.Favorites.Count == 0)
        {
            SearchList.ItemsSource = null;
            SearchHint.Text = "  아직 없다 — 아이템을 고르고 '즐겨찾기 ☆'";
            return;
        }

        await Guard(async () =>
        {
            // tpl 로 하나씩 찾는다. 즐겨찾기는 많아야 수십 개라 이 정도면 된다.
            var items = new List<FoundItem>();
            foreach (var tpl in _config.Favorites)
            {
                var hit = await _client!.SearchAsync(tpl, null, 1, CancellationToken.None);
                var item = hit?.Items.FirstOrDefault();
                if (item is not null
                    && (query.Length == 0 || item.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                {
                    items.Add(item);
                }
            }

            Show(items, false);
        });
    }

    /// <summary>분류별로 묶어 보여 준다. 2만 개를 한 줄씩 훑게 둘 수는 없다.</summary>
    private void Show(List<FoundItem> items, bool truncated)
    {
        foreach (var item in items)
        {
            item.Star = _config.Favorites.Contains(item.Tpl) ? "★" : string.Empty;
        }

        var view = new CollectionViewSource { Source = items };
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FoundItem.Category)));
        SearchList.ItemsSource = view.View;

        SearchHint.Text = items.Count == 0
            ? "  없다"
            : truncated ? $"  {items.Count}개 — 더 있다. 좁혀 본다" : $"  {items.Count}개";
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
