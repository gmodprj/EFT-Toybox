using MudBlazor;

namespace WebConfigBridge;

/// <summary>
/// 패널마다 같은 저장·알림을 되풀이하지 않도록 한곳에 둔다.
/// 저장은 한 번만 호출한다 — 결과를 두 번 묻다가 두 번 쓰는 실수를 막는다.
///
/// 화면이 읽은 뒤 파일이 밖에서 바뀌었으면 묻는다. 사람이 덮어쓰기를 고르지 않으면
/// 쓰지 않고 파일에 있는 값을 다시 불러온다 — 묵은 값으로 남의 변경을 되돌리지 않도록.
/// </summary>
public static class SaveResult
{
    public static async Task ReportAsync<T>(
        ConfigStore store,
        ISnackbar snackbar,
        IDialogService dialog,
        string target,
        T config,
        ConfigStamp stamp,
        Action reload)
    {
        var outcome = store.Save(target, config, stamp);

        if (outcome == SaveOutcome.Conflict)
        {
            var overwrite = await dialog.ShowMessageBoxAsync(
                "파일이 밖에서 바뀌었다",
                "이 화면을 연 뒤에 설정 파일이 바뀌었다. 덮어쓰면 그 변경이 사라진다.",
                yesText: "화면 값으로 덮어쓰기",
                cancelText: "파일 값 불러오기") ?? false;

            if (!overwrite)
            {
                reload();
                snackbar.Add("저장하지 않았다. 파일에 있는 값을 다시 불러왔다.", Severity.Info);
                return;
            }

            outcome = store.Save(target, config, stamp, force: true);
        }

        var ok = outcome == SaveOutcome.Saved;
        snackbar.Add(
            ok ? "저장했다. 적용은 다음 서버 시작부터다." : "저장하지 못했다. 서버 로그를 보라.",
            ok ? Severity.Success : Severity.Error);
    }
}
