namespace SlotFilterSweep;

/// <summary>
/// "이 id 가 데이터베이스에 있는가" 만 보는 순수 판정.
///
/// SPT 어셈블리를 쓰지 않는다 — 시험이 이 파일만 링크해 규칙을 지킨다. 실제 표를 훑는 일은
/// <c>SlotFilterSweep</c> 가 하고, 여기서는 **무엇을 죽은 것으로 칠지**만 정한다.
/// </summary>
public static class DeadRefScan
{
    /// <summary>
    /// 죽은 참조인가 — 표에 없고, 보호 목록에도 없으면 죽은 것이다.
    ///
    /// 보호 목록(<c>KeepIds</c>)이 표보다 세다. 우리보다 **늦게** 아이템을 만드는 모드가 있으면
    /// 그 부품이 지금은 없어 보여도 나중에 생긴다 — 그런 id 를 사람이 적어 지키는 자리다.
    /// </summary>
    public static bool IsDead(string? id, ISet<string> known, ISet<string>? keep = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false; // 빈 값은 우리가 판단할 것이 아니다. 건드리지 않는다.
        }

        if (keep is not null && keep.Contains(id))
        {
            return false;
        }

        return !known.Contains(id);
    }

    /// <summary>
    /// 필터 하나에서 죽은 참조만 골라 돌려준다. **중복은 한 번만** 센다.
    /// 살아 있는 것은 손대지 않는다 — 이 모드는 빼기만 하고 더하지 않는다.
    /// </summary>
    public static List<string> DeadIn(IEnumerable<string>? filter, ISet<string> known, ISet<string>? keep = null)
    {
        var dead = new List<string>();
        if (filter is null)
        {
            return dead;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var id in filter)
        {
            if (IsDead(id, known, keep) && seen.Add(id!))
            {
                dead.Add(id!);
            }
        }

        return dead;
    }

    /// <summary>
    /// 사람이 읽는 한 줄. **지웠는지 세기만 했는지를 말에 넣는다** — 기본값이 세기만 하는 것이라,
    /// 로그만 보고 "지워졌겠지" 하고 넘어가는 일이 없어야 한다.
    /// </summary>
    public static string Describe(int filters, int deadRefs, int distinctIds, bool applied)
    {
        if (deadRefs == 0)
        {
            return $"필터 {filters}개를 봤다. 죽은 참조 없음.";
        }

        var head = applied ? "지웠다" : "세기만 했다(Apply=false)";
        return $"필터 {filters}개 중 죽은 참조 {deadRefs}개(id {distinctIds}종) — {head}.";
    }
}
