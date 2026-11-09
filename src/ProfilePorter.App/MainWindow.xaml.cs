using System.Windows;
using System.Windows.Controls;

namespace ProfilePorter.App;

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private ServerClient? _client;
    private IconService? _icons;
    private string? _folder;

    public MainWindow()
    {
        InitializeComponent();
        _config = AppConfig.Load(out var configError);

        var sessionId = _config.ResolveSessionId(out var how);
        if (string.IsNullOrEmpty(sessionId))
        {
            // 세션을 못 찾으면 **아무것도 하지 않는다.** 창은 열어 두고 무엇을 고쳐야 하는지 말한다.
            Status($"세션 id 를 찾지 못했습니다 — {how}. {AppConfig.Path} 의 sptDir 나 sessionId 를 채우세요.", warn: true);
            return;
        }

        StashGrid.SelectionChanged += StashGrid_SelectionChanged;
        _icons = new IconService(_config.ResolveIconCacheFolder(), _config.DownloadItemIcons);
        StashGrid.Icons = _icons;
        _client = new ServerClient(_config.SptUrl, sessionId);
        Status($"서버에 묻는 중… (세션 {how})");
        _ = LoadAllAsync();

        if (configError is not null)
        {
            Status($"설정을 읽지 못해 기본값으로 엽니다 — {configError}", warn: true);
        }
    }

    private async Task LoadAllAsync()
    {
        if (_client is null) return;

        try
        {
            var profiles = await _client.ProfilesAsync(CancellationToken.None);
            if (profiles is null || !profiles.Ok)
            {
                Status("서버가 프로필을 주지 않았습니다 — 모드가 꺼져 있는지 보세요(config.jsonc 의 ModEnabled).", warn: true);
                return;
            }

            SourceProfiles.ItemsSource = profiles.Profiles;
            TargetProfiles.ItemsSource = profiles.Profiles.ToList();

            await LoadFilesAsync();
            Status($"프로필 {profiles.Profiles.Count}개를 읽었습니다. 왼쪽에서 내보낼 프로필을 고르세요.");
        }
        catch (Exception e)
        {
            Status($"서버에 닿지 못했습니다 — {e.Message}  (SPT 서버가 켜져 있나요?)", warn: true);
        }
    }

    private async Task LoadFilesAsync()
    {
        if (_client is null) return;

        var files = await _client.FilesAsync(CancellationToken.None);
        if (files is null) return;

        _folder = files.Folder;
        FolderText.Text = "  " + (_folder ?? string.Empty);

        // 이름은 서버가 폴더에서 읽어 채워 준다. 창이 지어내지 않는다 —
        // 지어낸 이름으로 들여오기를 부르면 서버가 다른 파일을 열거나 못 찾는다.
        Files.ItemsSource = files.Files;
        UpdateImportButton();
    }

    private async void SourceProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_client is null || SourceProfiles.SelectedItem is not ProfileRow row) return;

        StashGrid.Clear();
        Status($"{row.Nickname} 의 스태시를 읽는 중…");

        try
        {
            var stash = await _client.StashAsync(row.ProfileId, CancellationToken.None);
            if (stash is null || !stash.Ok)
            {
                Status(stash?.Reason ?? "스태시를 읽지 못했습니다.", warn: true);
                return;
            }

            StashGrid.Draw(stash.Grid?.CellsH ?? 0, stash.Grid?.CellsV ?? 0, stash.Items);
            Status($"{row.Nickname} 의 스태시 바닥 {stash.Items.Count}개. "
                   + "칸을 눌러 고르세요 — Ctrl 을 누른 채 누르면 여러 개가 됩니다.");
        }
        catch (Exception ex)
        {
            Status($"스태시를 읽지 못했습니다 — {ex.Message}", warn: true);
        }
    }

    private void StashGrid_SelectionChanged()
    {
        var picked = StashGrid.SelectedRows;
        if (picked.Count == 0)
        {
            Status("고른 것이 없습니다. 칸을 눌러 고르세요.");
            return;
        }

        // **안에 든 것까지 센 수**를 보여 준다 — 실제로 옮겨지는 양이 그것이다(설계 §9.1).
        var total = picked.Sum(r => 1 + r.Contained);
        Status($"{picked.Count}개를 골랐습니다 — 안에 든 것까지 {total}개가 나갑니다.");
    }

    private void TargetProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateImportButton();

    private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateImportButton();
        UpdateFileButtons();
    }

    /// <summary>
    /// 못 읽는 파일은 **지우기만** 된다 — 세부 보기도 보내기도 열 수 없다(설계 §2.1).
    /// </summary>
    private void UpdateFileButtons()
    {
        var row = Files.SelectedItem as PorterFileRow;
        DeleteButton.IsEnabled = row is not null;
        DetailButton.IsEnabled = row is { Readable: true };
    }

    /// <summary>
    /// 세부 보기 창을 띄운다. **본 창을 막지 않는다**(`Show`) — 열어 둔 채로 다른 파일을
    /// 고를 수 있다. 아이콘은 **같은 인스턴스**를 넘겨 캐시를 두 번 받지 않는다.
    /// </summary>
    private void Detail_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null || Files.SelectedItem is not PorterFileRow row) return;

        if (!row.Readable)
        {
            Status("그 파일은 열지 못합니다 — 지우기만 됩니다.", warn: true);
            return;
        }

        new FileDetailWindow(_client, _icons, row.FileName) { Owner = this }.Show();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null || Files.SelectedItem is not PorterFileRow row) return;

        // 되돌릴 수 없는 일이다. **한 번 묻는다** — 무엇이 사라지고 무엇이 남는지 함께 적는다.
        var what = row.Readable
            ? $"아이템 {row.Items.Count}개 · {row.CreatedUtc} (UTC)"
            : "못 읽는 파일";

        var answer = MessageBox.Show(
            $"'{row.FileName}' 을 지웁니다.\n\n{what}\n\n"
            + "되돌릴 수 없습니다.\n이미 보낸 우편과 받은 물건은 그대로 남습니다.",
            "내보낸 파일 지우기",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (answer != MessageBoxResult.Yes) return;

        try
        {
            var result = await _client.DeleteFileAsync(row.FileName, CancellationToken.None);
            if (result is null || !result.Ok)
            {
                // 서버가 준 사유를 **그대로** 적는다. 우리가 지어내지 않는다.
                Status(result?.Reason ?? "지우지 못했습니다.", warn: true);
                return;
            }

            await LoadFilesAsync();
            Status($"'{row.FileName}' 을 지웠습니다.");
        }
        catch (Exception ex)
        {
            Status($"지우지 못했습니다 — {ex.Message}", warn: true);
        }
    }

    private void UpdateImportButton()
    {
        var hasFile = Files.SelectedItem is PorterFileRow;
        var hasTarget = TargetProfiles.SelectedItem is ProfileRow;
        ImportButton.IsEnabled = hasFile && hasTarget;

        if (!hasFile || !hasTarget)
        {
            ImportHint.Text = "파일과 받는 프로필을 고르면 보낼 수 있습니다.";
            return;
        }

        var file = (PorterFileRow)Files.SelectedItem;
        var target = (ProfileRow)TargetProfiles.SelectedItem;
        ImportHint.Text = $"{target.Nickname} 에게 **우편으로** {file.Items.Count}개를 보냅니다. "
                          + "스태시에 바로 꽂지 않습니다 — 게임 안 우편함에서 받으세요.";
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e) => StashGrid.SelectAll();

    private void SelectNone_Click(object sender, RoutedEventArgs e) => StashGrid.SelectNone();

    private async void ExportStash_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null) return;

        if (SourceProfiles.SelectedItem is not ProfileRow row)
        {
            Status("내보낼 프로필을 먼저 고르세요.", warn: true);
            return;
        }

        var ids = StashGrid.SelectedRows.Select(r => r.Id).ToList();
        if (ids.Count == 0)
        {
            Status("내보낼 것을 하나도 고르지 않았습니다.", warn: true);
            return;
        }

        await RunExportAsync((overwrite, rename) => _client!.ExportStashAsync(
            row.ProfileId, ids, TypedName(), null, overwrite, rename, CancellationToken.None));
    }

    private async void ExportLoadout_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null) return;

        if (SourceProfiles.SelectedItem is not ProfileRow row)
        {
            Status("내보낼 프로필을 먼저 고르세요.", warn: true);
            return;
        }

        await RunExportAsync((overwrite, rename) => _client!.ExportLoadoutAsync(
            row.ProfileId, TypedName(), null, overwrite, rename, CancellationToken.None));
    }

    private async Task RunExportAsync(Func<bool, bool, Task<PorterResultRow?>> call)
    {
        SetBusy(true);
        try
        {
            var result = await call(false, false);

            // 같은 이름이면 서버가 **덮지 않고 거절**한다. 그때만 사람에게 묻는다.
            if (result is not null && !result.Ok
                && string.Equals(result.Reason, ServerClient.NameTakenReason, StringComparison.Ordinal))
            {
                var answer = MessageBox.Show(
                    $"'{ExportName.Text?.Trim()}' 은(는) 이미 있습니다.\n\n"
                    + "[예] 덮어씁니다 — 먼저 있던 파일은 사라집니다.\n"
                    + "[아니요] 뒤에 번호를 붙여 새 파일로 저장합니다.\n"
                    + "[취소] 아무것도 하지 않습니다.",
                    "같은 이름이 있습니다", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (answer == MessageBoxResult.Cancel)
                {
                    Status("내보내지 않았습니다.");
                    return;
                }

                result = await call(answer == MessageBoxResult.Yes, answer == MessageBoxResult.No);
            }

            if (result is null || !result.Ok)
            {
                Status(result?.Reason ?? "내보내지 못했습니다.", warn: true);
                return;
            }

            await LoadFilesAsync();

            var skipped = result.Skipped.Count == 0
                ? string.Empty
                : "  (뺀 것: " + string.Join(" / ", result.Skipped) + ")";
            Status($"'{result.FileName}' 으로 {result.Count}개를 내보냈습니다 — 원본 프로필은 고치지 않았습니다.{skipped}");
        }
        catch (Exception e)
        {
            Status($"내보내지 못했습니다 — {e.Message}", warn: true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null) return;
        if (Files.SelectedItem is not PorterFileRow file || TargetProfiles.SelectedItem is not ProfileRow target) return;

        // **되돌릴 수 없는 쪽**이므로 한 번 묻는다. 창이 묻고, 서버가 또 백업한다.
        var answer = MessageBox.Show(
            $"{target.Nickname} 에게 우편으로 {file.Items.Count}개를 보냅니다.\n\n"
            + "이 도구는 물건을 늘립니다 — 보내는 쪽은 그대로 남습니다.\n"
            + "받는 프로필은 보내기 전에 자동으로 백업됩니다.",
            "보낼까요?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (answer != MessageBoxResult.OK)
        {
            Status("보내지 않았습니다.");
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _client.ImportAsync(target.ProfileId, file.FileName, CancellationToken.None);
            if (result is null || !result.Ok)
            {
                Status(result?.Reason ?? "보내지 못했습니다.", warn: true);
                return;
            }

            // 서버가 고친 것(잠금 풀기 등)은 **말해 준다.** 조용히 바뀌면 사람이 모른다.
            var said = result.Skipped.Count == 0
                ? string.Empty
                : "  " + string.Join(" / ", result.Skipped);

            Status($"{target.Nickname} 에게 {result.Count}개를 우편으로 보냈습니다 — "
                   + "게임 안 우편함에서 받으세요. 받기 전에는 그 프로필의 재산이 아닙니다." + said);
        }
        catch (Exception ex)
        {
            Status($"보내지 못했습니다 — {ex.Message}", warn: true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAllAsync();

    /// <summary>
    /// 사람이 적은 이름. 비어 있으면 **null 을 보낸다** — 기본 이름은 서버가 짓는다
    /// (`프로필명-종류-날짜_시각`). 창과 서버가 각자 짓게 두면 규칙이 둘로 갈린다.
    /// </summary>
    private string? TypedName()
    {
        var typed = ExportName.Text?.Trim();
        return string.IsNullOrWhiteSpace(typed) ? null : typed;
    }

    private void SetBusy(bool busy)
    {
        SelectAllButton.IsEnabled = !busy;
        SelectNoneButton.IsEnabled = !busy;
        ExportStashButton.IsEnabled = !busy;
        ExportLoadoutButton.IsEnabled = !busy;
        ImportButton.IsEnabled = !busy && Files.SelectedItem is not null && TargetProfiles.SelectedItem is not null;
    }

    private void Status(string text, bool warn = false)
    {
        StatusText.Text = text;
        StatusText.Foreground = warn
            ? (System.Windows.Media.Brush)FindResource("Warn")
            : (System.Windows.Media.Brush)FindResource("Fg");
    }
}
