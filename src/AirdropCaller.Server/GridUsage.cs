namespace AirdropCaller.Server;

/// <summary>
/// 상자 격자가 얼마나 찼는지 센다.
///
/// 왜 세나 — 지금까지 창이 할 수 있는 말은 "몇 개 담았다 / 못 담았다" 뿐이었다. **얼마나
/// 남았는지**는 못 담는 일이 실제로 벌어진 뒤에야 알았다(방탄복 20개를 120칸 상자에 넣고
/// "자리 없음" 열네 줄을 받은 적이 있다). 마른 조립과 함께 쓰면 부르기 **전에** 알 수 있다.
///
/// SPT 어셈블리를 쓰지 않는다 — 시험이 이 파일만 링크해 셈을 본다.
/// 칸 지도는 SPT 의 `TryFillContainerMapWithItem` 이 채운 그대로다: **0 이 빈 칸**이다.
/// </summary>
public static class GridUsage
{
    /// <summary>찬 칸 수. 0 이 아닌 칸을 센다.</summary>
    public static int Used(int[,] map)
    {
        if (map is null)
        {
            return 0;
        }

        var used = 0;
        foreach (var cell in map)
        {
            if (cell != 0)
            {
                used++;
            }
        }

        return used;
    }

    /// <summary>칸 수 전부.</summary>
    public static int Total(int[,] map) => map?.Length ?? 0;

    /// <summary>격자가 여럿이면 합쳐 센다 — 상자 하나가 격자를 여러 개 가질 수 있다.</summary>
    public static (int Used, int Total) Sum(IEnumerable<int[,]>? maps)
    {
        var used = 0;
        var total = 0;
        foreach (var map in maps ?? [])
        {
            used += Used(map);
            total += Total(map);
        }

        return (used, total);
    }

    /// <summary>
    /// 사람이 읽는 한 토막. 칸 수를 모르면(격자가 없으면) 아는 척하지 않는다.
    /// </summary>
    public static string Describe(int used, int total)
    {
        if (total <= 0)
        {
            return "칸 수를 모른다";
        }

        var percent = (int)Math.Round(used * 100.0 / total, MidpointRounding.AwayFromZero);
        return $"칸 {used}/{total} ({percent}%)";
    }
}
