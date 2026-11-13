using System.Text.Json.Serialization;

namespace AirdropCaller.App;

/// <summary>
/// 사람이 끌어 맞춘 창 모양. 창 크기·자리, 세 칸(프리셋·품목·아이템 찾기)의 너비,
/// 품목 표의 칸 너비를 담는다 — 창을 닫을 때 적고 다음에 열 때 되돌린다.
///
/// **왜 문자열인가.** WPF 의 너비는 픽셀일 수도 있고 별(`*`, 남는 자리를 나눠 갖기)일 수도 있다.
/// 숫자로만 적으면 끌지 않은 칸까지 전부 픽셀로 굳어, 창을 넓혀도 가운데가 늘어나지 않는다.
/// 그래서 **WPF 가 말하는 그대로** 적는다 — `"210"`·`"*"`·`"1.5*"`.
///
/// 되돌리는 쪽은 **읽지 못하는 값을 만나면 그 칸만 건너뛴다**(XAML 의 기본값이 남는다).
/// 설정 파일이 손으로 고쳐지거나 옛 판일 수 있어서, 한 칸 때문에 창이 안 뜨면 안 된다.
/// </summary>
public class WindowLayout
{
    [JsonPropertyName("width")] public double Width { get; set; }
    [JsonPropertyName("height")] public double Height { get; set; }

    /// <summary>창의 왼위 자리. 화면 밖이면 되돌리지 않는다(모니터가 바뀌었을 수 있다).</summary>
    [JsonPropertyName("left")] public double Left { get; set; } = double.NaN;
    [JsonPropertyName("top")] public double Top { get; set; } = double.NaN;

    [JsonPropertyName("maximized")] public bool Maximized { get; set; }

    /// <summary>왼쪽 "프리셋" 칸의 너비.</summary>
    [JsonPropertyName("presetColumn")] public string PresetColumn { get; set; } = string.Empty;

    /// <summary>오른쪽 "아이템 찾기" 칸의 너비. 가운데 칸은 남는 자리를 받으므로 적지 않는다.</summary>
    [JsonPropertyName("searchColumn")] public string SearchColumn { get; set; } = string.Empty;

    /// <summary>품목 표의 칸 너비. 열쇠는 **칸 차례**(0부터)다 — 머리글은 바뀔 수 있다.</summary>
    [JsonPropertyName("itemColumns")] public Dictionary<string, string> ItemColumns { get; set; } = [];

    /// <summary>적어 둔 것이 하나라도 있나. 없으면 되돌릴 것이 없다(처음 켠 것이다).</summary>
    [JsonIgnore]
    public bool HasAnything =>
        Width > 0 || Height > 0 || Maximized
        || !string.IsNullOrWhiteSpace(PresetColumn)
        || !string.IsNullOrWhiteSpace(SearchColumn)
        || ItemColumns.Count > 0;
}
