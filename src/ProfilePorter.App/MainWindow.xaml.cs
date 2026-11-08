using System.Windows;
using System.Windows.Controls;

namespace ProfilePorter.App;

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private ServerClient? _client;
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

        StashItems.ItemsSource = null;
        Status($"{row.Nickname} 의 스태시를 읽는 중…");

        try
        {
            var stash = await _client.StashAsync(row.ProfileId, CancellationToken.None);
            if (stash is null || !stash.Ok)
            {
                Status(stash?.Reason ?? "스태시를 읽지 못했습니다.", warn: true);
                return;
            }

            StashItems.ItemsSource = stash.Items;
            Status($"{row.Nickname} 의 스태시 바닥 {stash.Items.Count}개. 내보낼 것을 고르세요(여러 개 됩니다).");
        }
        catch (Exception ex)
        {
            Status($"스태시를 읽지 못했습니다 — {ex.Message}", warn: true);
        }
    }

    private void StashItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var picked = StashItems.SelectedItems.Count;
        if (picked == 0) return;

        // **안에 든 것까지 센 수**를 보여 준다 — 실제로 옮겨지는 양이 그것이다(설계 §9.1).
        var total = StashItems.SelectedItems.OfType<StashRow>().Sum(r => 1 + r.Contained);
        Status($"{picked}개를 골랐습니다 — 안에 든 것까지 {total}개가 나갑니다.");
    }

    private void TargetProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateImportButton();

    private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateImportButton();

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

    private async void ExportStash_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null) return;

        if (SourceProfiles.SelectedItem is not ProfileRow row)
        {
            Status("내보낼 프로필을 먼저 고르세요.", warn: true);
            return;
        }

        var ids = StashItems.SelectedItems.OfType<StashRow>().Select(r => r.Id).ToList();
        if (ids.Count == 0)
        {
            Status("내보낼 것을 하나도 고르지 않았습니다.", warn: true);
            return;
        }

        await RunExportAsync(() => _client.ExportStashAsync(
            row.ProfileId, ids, NameOrDefault(row.Nickname, "stash"), null, CancellationToken.None));
    }

    private async void ExportLoadout_Click(object sender, RoutedEventArgs e)
    {
        if (_client is null) return;

        if (SourceProfiles.SelectedItem is not ProfileRow row)
        {
            Status("내보낼 프로필을 먼저 고르세요.", warn: true);
            return;
        }

        await RunExportAsync(() => _client.ExportLoadoutAsync(
            row.ProfileId, NameOrDefault(row.Nickname, "loadout"), null, CancellationToken.None));
    }

    private async Task RunExportAsync(Func<Task<PorterResultRow?>> call)
    {
        SetBusy(true);
        try
        {
            var result = await call();
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

            Status($"{target.Nickname} 에게 {result.Count}개를 우편으로 보냈습니다 — "
                   + "게임 안 우편함에서 받으세요. 받기 전에는 그 프로필의 재산이 아닙니다.");
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

    /// <summary>이름을 안 적었으면 프로필 이름과 시각으로 짓는다. 빈 이름으로 저장하지 않는다.</summary>
    private string NameOrDefault(string nickname, string kind)
    {
        var typed = ExportName.Text?.Trim();
        return string.IsNullOrWhiteSpace(typed)
            ? $"{nickname}-{kind}-{DateTime.Now:yyyyMMdd-HHmmss}"
            : typed;
    }

    private void SetBusy(bool busy)
    {
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
