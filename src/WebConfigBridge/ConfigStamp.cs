using System.Security.Cryptography;
using System.Text;

namespace WebConfigBridge;

/// <summary>설정을 쓴 결과.</summary>
public enum SaveOutcome
{
    /// <summary>파일에 썼다.</summary>
    Saved,

    /// <summary>화면이 읽은 뒤 파일이 밖에서 바뀌었다. <b>쓰지 않았다.</b></summary>
    Conflict,

    /// <summary>파일이 없거나 쓰다가 실패했다.</summary>
    Failed,
}

/// <summary>
/// 패널이 "내가 읽은 그 파일" 을 기억하는 자리.
///
/// 패널은 열릴 때 파일을 읽고 저장할 때 들고 있던 값을 그대로 쓴다. 페이지를 열어둔 사이
/// 파일이 밖에서 바뀌면(직접 편집·다른 도구·다른 탭) 저장이 그 변경을 되돌린다.
/// 실제로 겪었다 — <c>FckWeapons</c> 를 파일에서 끈 것이 나중에 켜져 있었다.
///
/// 그래서 읽을 때 파일 내용의 지문을 여기 적어 두고, 쓰기 직전에 파일을 다시 읽어 견준다.
/// 다르면 쓰지 않고 <see cref="SaveOutcome.Conflict"/> 로 알린다.
///
/// 시각(mtime)이 아니라 내용의 해시를 쓴다 — 같은 초 안에 바뀌거나 시각만 건드려지는 경우에
/// 시각은 믿을 수 없다.
/// </summary>
public sealed class ConfigStamp
{
    /// <summary>마지막으로 읽거나 쓴 파일 내용의 지문. 아직 읽지 않았으면 null.</summary>
    public string? Value { get; set; }

    /// <summary>파일 내용의 지문.</summary>
    public static string Of(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    /// <summary>
    /// 지금 파일 내용이 내가 읽은 그것인지. 아직 아무것도 읽지 않았다면(<see cref="Value"/> 가 null)
    /// 견줄 것이 없으므로 참으로 둔다 — 지문을 쓰지 않는 호출자의 동작을 바꾸지 않는다.
    /// </summary>
    public bool Matches(string? text) =>
        Value is null || (text is not null && Value == Of(text));
}
