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
    private ItemRow? _parent;

    public string Tpl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>안에 다른 품목을 담을 수 있는가. 서버가 알려 준 값이다.</summary>
    public bool CanContain { get; set; }

    /// <summary>분류 아이콘 파일 이름. 아이템 그림을 못 구했을 때 이걸 쓴다.</summary>
    public string CategoryIcon { get; set; } = string.Empty;

    private System.Windows.Media.ImageSource? _itemIcon;

    /// <summary>아이템 그림. 받아 오는 데 시간이 걸려 뒤늦게 채워진다.</summary>
    public System.Windows.Media.ImageSource? ItemIcon
    {
        get => _itemIcon;
        set { _itemIcon = value; Changed(nameof(ItemIcon)); }
    }

    /// <summary>
    /// 이 품목을 담을 곳. null 이면 보급 상자에 직접 넣는다.
    ///
    /// 보급 상자는 10×12=120칸으로 고정이라, 더 담으려면 가방·케이스를 넣고
    /// 그 안에 담아야 한다. 무기를 케이스에 넣을지 말지도 이걸로 고른다.
    /// </summary>
    public ItemRow? Parent
    {
        get => _parent;
        set { _parent = value; Changed(nameof(Parent)); }
    }

    /// <summary>표에서 고르는 이름. 담을 수 있는 물건만 다른 줄의 선택지가 된다.</summary>
    public string Label => Name.Length > 24 ? Name[..24] + "…" : Name;

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

/// <summary>
/// 담을 곳 고르기 칸의 한 줄. `null` 인 줄이 "상자(직접)" 다.
/// </summary>
public class ContainerChoice(ItemRow? row)
{
    public ItemRow? Row { get; } = row;

    public string Label => Row is null ? "상자" : "↳ " + Row.Label;
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

    /// <summary>"담을 곳" 칸이 보여 주는 선택지. 맨 앞이 "상자(직접)" 다.</summary>
    public ObservableCollection<ContainerChoice> ContainerChoices { get; } = [];

    private IconService? _icons;
    private readonly FileToIconConverter _categoryIcons = new();

    /// <summary>
    /// 목록에 그림을 채운다. 받아 오는 데 시간이 걸리므로 보여 준 **뒤에** 따로 돈다 —
    /// 그림 때문에 목록이 늦게 뜨면 안 된다. 못 구한 것은 분류 아이콘으로 넘어간다.
    /// </summary>
    private Task FillIconsAsync(IReadOnlyList<FoundItem> items) =>
        FillAsync(items, i => i.Tpl, i => i.CategoryIcon, (i, icon) => i.ItemIcon = icon);

    private Task FillRowIconsAsync(IReadOnlyList<ItemRow> rows) =>
        FillAsync(rows, r => r.Tpl, r => r.CategoryIcon, (r, icon) => r.ItemIcon = icon);

    /// <summary>
    /// 그림을 **한꺼번에** 받는다. 하나씩 기다리면 400줄짜리 목록이 몇 분 걸린다
    /// (한 번 재 봤더니 20초에 하나였다). 동시 수 제한은 <see cref="IconService"/> 가 쥔다.
    /// </summary>
    private async Task FillAsync<T>(
        IReadOnlyList<T> rows,
        Func<T, string> tpl,
        Func<T, string> categoryIcon,
        Action<T, System.Windows.Media.ImageSource?> set)
    {
        if (_icons is null)
        {
            return;
        }

        await Task.WhenAll(rows.Select(async row =>
        {
            var icon = await _icons.GetAsync(tpl(row), CancellationToken.None);

            // CDN 에 없는 아이템이 있다(새로 들어온 방어판 등). 분류 아이콘으로 넘어간다.
            icon ??= _categoryIcons.Convert(categoryIcon(row), typeof(object), null,
                         System.Globalization.CultureInfo.CurrentCulture) as System.Windows.Media.ImageSource;

            set(row, icon);
        }));
    }

    /// <summary>
    /// 담을 수 있는 줄이 바뀔 때마다 선택지를 다시 만든다.
    /// 자기 자신은 뺀다 — 자기 안에 자기를 담을 수는 없다.
    /// </summary>
    private void RebuildContainerChoices()
    {
        ContainerChoices.Clear();
        ContainerChoices.Add(new ContainerChoice(null));
        foreach (var row in _items.Where(r => r.CanContain))
        {
            ContainerChoices.Add(new ContainerChoice(row));
        }

        // 지워진 줄을 가리키고 있던 것은 상자 직접으로 되돌린다.
        foreach (var row in _items.Where(r => r.Parent is not null && !_items.Contains(r.Parent!)))
        {
            row.Parent = null;
        }
    }

    /// <summary>
    /// 평평한 줄 목록을 중첩 트리로 바꾼다. 고리(A 안에 B, B 안에 A)가 생기면
    /// 그 줄을 상자 직접으로 되돌린다 — 서버에 보내서 터뜨리느니 여기서 푼다.
    /// </summary>
    private List<ItemSpec> BuildSpecTree()
    {
        var specs = _items.ToDictionary(r => r, r => r.ToSpec());
        var roots = new List<ItemSpec>();

        foreach (var row in _items)
        {
            var parent = row.Parent;
            if (parent is not null && (parent == row || !_items.Contains(parent) || MakesCycle(row)))
            {
                parent = null;
            }

            if (parent is null)
            {
                roots.Add(specs[row]);
                continue;
            }

            (specs[parent].Items ??= []).Add(specs[row]);
        }

        return roots;
    }

    private bool MakesCycle(ItemRow row)
    {
        var seen = new HashSet<ItemRow>();
        for (var at = row.Parent; at is not null; at = at.Parent)
        {
            if (at == row || !seen.Add(at))
            {
                return true;
            }
        }

        return false;
    }

    public MainWindow()
    {
        InitializeComponent();

        _config = AppConfig.LoadOrCreate(out _startupError);

        // 분류 아이콘은 설치본의 SPT_Data\images\handbook 에서 직접 읽는다.
        FileToIconConverter.SptDir = _config.SptDir;
        _icons = new IconService(_config.ResolveIconCacheFolder(), _config.DownloadItemIcons);

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
                // 즐겨찾기가 있으면 거기부터. 없으면 **가장 큰 분류**를 연다 —
                // 가나다 순 첫 줄은 "방어판·삽입물" 인데, 하필 그 품목들은 아이콘
                // 제공처에도 없어서 처음 여는 사람에게 가장 안 좋은 첫인상이었다.
                var biggest = all
                    .Select((c, i) => (c, i))
                    .Where(x => x.c.Id.Length > 0 && x.c.Id != FavoriteCategoryId)
                    .OrderByDescending(x => x.c.Count)
                    .Select(x => x.i)
                    .FirstOrDefault();

                CategoryBox.SelectedIndex = _config.Favorites.Count > 0 ? 0 : biggest;
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


    private async void OnPresetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (PresetList.SelectedItem is Preset preset)
        {
            await LoadPresetIntoEditorAsync(preset, preset.Name);
        }
    }

    /// <summary>
    /// 프리셋을 편집 칸으로 옮긴다.
    ///
    /// 프리셋에는 tpl 만 들어 있다. 이름·크기·담을 수 있는지는 서버에 **한 번에**
    /// 물어 채운다 — 예전에는 품목 칸에 24자리 tpl 이 그대로 보였다.
    /// </summary>
    private async Task LoadPresetIntoEditorAsync(Preset preset, string name)
    {
        PresetNameBox.Text = name;
        IconBox.SelectedIndex = preset.Icon switch { "Supply" => 1, "Medical" => 2, "Weapon" => 3, _ => 0 };

        _items.Clear();

        var tpls = new List<string>();
        CollectTpls(preset.Items, tpls);

        var known = new Dictionary<string, FoundItem>();
        if (tpls.Count > 0 && _client is not null)
        {
            try
            {
                var found = await _client.LookupAsync(tpls, CancellationToken.None);
                foreach (var item in found?.Items ?? [])
                {
                    known[item.Tpl] = item;
                }
            }
            catch (Exception e)
            {
                MessageLine.Text = "품목 이름을 못 받았다(목록은 그대로 쓸 수 있다): " + e.Message;
            }
        }

        AddRows(preset.Items, null, known);
        _ = FillRowIconsAsync(_items.ToList());
        RebuildContainerChoices();
        UpdateFitHint();
    }

    private static void CollectTpls(IEnumerable<ItemSpec>? specs, List<string> into)
    {
        foreach (var spec in specs ?? [])
        {
            into.Add(spec.Tpl);
            CollectTpls(spec.Items, into);
        }
    }

    /// <summary>중첩된 프리셋을 평평한 줄 목록으로 편다. 담을 곳은 Parent 로 남는다.</summary>
    private void AddRows(IEnumerable<ItemSpec>? specs, ItemRow? parent, IReadOnlyDictionary<string, FoundItem> known)
    {
        foreach (var spec in specs ?? [])
        {
            known.TryGetValue(spec.Tpl, out var info);
            var row = new ItemRow
            {
                Tpl = spec.Tpl,
                Name = info?.Name ?? spec.Tpl,
                CanContain = info?.CanContain ?? false,
                CategoryIcon = info?.CategoryIcon ?? string.Empty,
                Count = spec.Count,
                StackSize = spec.StackSize,
                Parent = parent,
            };

            _items.Add(row);
            AddRows(spec.Items, row, known);
        }
    }

    /// <summary>
    /// 고른 프리셋을 그대로 베껴 새것으로 만든다. 저장을 눌러야 서버에 올라간다 —
    /// 베끼자마자 이름을 고치는 것이 보통이라 한 번 더 묻지 않는다.
    /// </summary>
    private async void OnCopyPreset(object sender, RoutedEventArgs e)
    {
        if (PresetList.SelectedItem is not Preset preset)
        {
            MessageLine.Text = "베낄 프리셋을 먼저 고른다.";
            return;
        }

        // 고른 것을 풀어 두고 선택을 지운다. 저장하면 새 id 로 올라간다.
        var copy = preset;
        PresetList.SelectedItem = null;
        await LoadPresetIntoEditorAsync(copy, copy.Name + " 사본");
        MessageLine.Text = "베꼈다. 이름을 고치고 저장을 누른다.";
    }

    private void OnNewPreset(object sender, RoutedEventArgs e)
    {
        PresetList.SelectedItem = null;
        PresetNameBox.Text = "새 프리셋";
        IconBox.SelectedIndex = 0;
        _items.Clear();
        RebuildContainerChoices();
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
            Items = BuildSpecTree(),
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

        // **이름을 적었으면 분류에 가두지 않는다.** 분류는 훑어보기용이다. 둘을 겹쳐
        // 걸었더니, 분류가 탄약에 맞춰진 채로 "iotv" 를 적으면 아이템이 멀쩡히 있는데도
        // "없다" 가 떴다. 적은 사람은 분류를 좁히려던 것이 아니라 그 이름을 찾던 것이다.
        var scope = query.Length > 0 ? string.Empty : category;

        await Guard(async () =>
        {
            var found = await _client.SearchAsync(query, scope, 400, CancellationToken.None);
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

        // 갈아 끼우기 **전에** 선택을 비운다. 목록이 바뀌면 전에 고른 것은 더 이상
        // 고른 것이 아니다 — 남겨 두면 그것이 `SelectedItem` 으로 돌아온다.
        SearchList.SelectedIndex = -1;

        var view = new CollectionViewSource { Source = items };
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FoundItem.Category)));
        SearchList.ItemsSource = view.View;

        SearchHint.Text = items.Count == 0
            ? "  없다"
            : truncated ? $"  {items.Count}개 — 더 있다. 좁혀 본다" : $"  {items.Count}개";

        // 그림은 뒤따라 온다. 목록이 그림 때문에 늦게 뜨면 안 된다.
        _ = FillIconsAsync(items);
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
            CanContain = found.CanContain,
            CategoryIcon = found.CategoryIcon,
            Count = 1,
            // 묶이는 물건이면 가득 채운 한 칸이 기본값으로 쓸 만하다.
            StackSize = found.MaxStack > 1 ? found.MaxStack : null,
        });

        RebuildContainerChoices();
        UpdateFitHint();
    }

    private void OnRemoveItem(object sender, RoutedEventArgs e)
    {
        if (ItemGrid.SelectedItem is ItemRow row)
        {
            _items.Remove(row);
            RebuildContainerChoices();
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
                null, SelectedIcon(), BuildSpecTree(), CancellationToken.None);

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
