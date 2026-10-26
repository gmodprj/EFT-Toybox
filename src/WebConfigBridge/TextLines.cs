namespace WebConfigBridge;

/// <summary>
/// 문자열 목록을 여러 줄 입력칸과 주고받는다.
/// 사람이 적는 칸이라 빈 줄과 앞뒤 공백을 그대로 넘기면 설정이 더러워진다.
/// </summary>
public static class TextLines
{
    public static string Join(List<string>? values) =>
        values is null ? string.Empty : string.Join(Environment.NewLine, values);

    public static List<string> Split(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                  .ToList();
}
