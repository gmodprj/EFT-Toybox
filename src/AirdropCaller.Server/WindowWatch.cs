using SPTarkov.DI.Annotations;

namespace AirdropCaller.Server;

/// <summary>
/// 외부 창이 아직 보고 있는가.
///
/// 플러그인은 레이드 내내 서버를 두드린다. 그런데 **창이 꺼져 있으면 아무도 그 보고를
/// 읽지 않는다.** 그래도 1초마다 두드리고 있었더니 게임 로그가 우리 것으로 가득 찼다
/// (최근 4000줄 중 3892줄, 실측 2026-10-29). 쓰지도 않을 보고를 그 속도로 받을 이유가 없다.
///
/// 창은 상태를 2초마다 물어본다. 그 호출을 여기서 적어 두고, 최근에 물어본 적이
/// 없으면 플러그인에게 "천천히 보내라" 고 일러 준다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class WindowWatch
{
    /// <summary>이만큼 창이 조용하면 꺼진 것으로 본다. 창의 상태 주기(2초)보다 넉넉히 길게.</summary>
    public static readonly TimeSpan QuietAfter = TimeSpan.FromSeconds(30);

    private long _lastTicks = DateTime.MinValue.Ticks;

    public void Note() => Interlocked.Exchange(ref _lastTicks, DateTime.UtcNow.Ticks);

    public bool Watching =>
        DateTime.UtcNow - new DateTime(Interlocked.Read(ref _lastTicks)) <= QuietAfter;
}
