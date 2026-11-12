namespace HelmetSlotUnion;

/// <summary>
/// 합집합에 **무엇을 담을지** 정하는 순수 판정.
///
/// 왜 생겼나 — 2026-11-12 실측(doc/REVIEW-MODS-AUDIT-2026-11-12.md 덧붙임 2). 이 모드는 헬멧
/// 칸 필터를 모아 합집합을 돌려주는데, 어느 헬멧 하나에 적혀 있던 **데이터베이스에 없는 id**
/// 까지 함께 모아 **모든 헬멧에 복사**하고 있었다. 죽은 참조 하나가 헬멧 174칸으로 불어났다.
///
/// 그래서 담기 전에 "표에 있는 id 인가" 를 본다. 넓히는 일 자체는 그대로다 — 없는 것만 안 담는다.
///
/// SPT 어셈블리를 쓰지 않는다. 시험이 이 파일만 링크해 규칙을 지킨다.
/// </summary>
public static class PoolPolicy
{
    /// <summary>
    /// 이 id 를 합집합에 담아도 되나.
    ///
    /// <paramref name="skipDead"/> 가 꺼져 있으면 예전처럼 전부 담는다(되돌릴 자리를 남겨 둔다).
    /// </summary>
    public static bool ShouldPool(string? id, ISet<string> known, bool skipDead = true)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return !skipDead || known.Contains(id);
    }

    /// <summary>
    /// 담을 것만 걸러 돌려주고, 버린 개수를 함께 준다. 버린 것은 **조용히 사라지지 않는다** —
    /// 부르는 쪽이 로그에 숫자를 적는다.
    /// </summary>
    public static (List<string> Pooled, int Skipped) Filter(
        IEnumerable<string>? ids, ISet<string> known, bool skipDead = true)
    {
        var pooled = new List<string>();
        var skipped = 0;

        foreach (var id in ids ?? [])
        {
            if (ShouldPool(id, known, skipDead))
            {
                pooled.Add(id!);
            }
            else if (!string.IsNullOrWhiteSpace(id))
            {
                skipped++;
            }
        }

        return (pooled, skipped);
    }

    /// <summary>사람이 읽는 꼬리. 버린 것이 없으면 아무 말도 하지 않는다.</summary>
    public static string Describe(int skipped)
    {
        return skipped == 0
            ? string.Empty
            : $" 표에 없는 id {skipped}개는 담지 않았다(다른 헬멧으로 퍼지지 않는다).";
    }
}
