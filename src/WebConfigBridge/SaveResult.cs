using MudBlazor;

namespace WebConfigBridge;

/// <summary>
/// 패널마다 같은 저장·알림을 되풀이하지 않도록 한곳에 둔다.
/// 저장은 한 번만 호출한다 — 결과를 두 번 묻다가 두 번 쓰는 실수를 막는다.
/// </summary>
public static class SaveResult
{
    public static void Report<T>(ConfigStore store, ISnackbar snackbar, string target, T config)
    {
        var ok = store.Save(target, config);

        snackbar.Add(
            ok ? "저장했다. 적용은 다음 서버 시작부터다." : "저장하지 못했다. 서버 로그를 보라.",
            ok ? Severity.Success : Severity.Error);
    }
}
