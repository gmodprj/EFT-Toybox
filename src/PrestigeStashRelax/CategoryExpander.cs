namespace PrestigeStashRelax;

/// <summary>
/// 분류(Node) id 를 **그 아래 아이템 id 전부로 펼친다.**
///
/// 왜 — 2026-11-15 실측. 클라이언트의 판정은 겉보기에 분류를 거슬러 간다
/// (<c>HandbookEntities.Contains</c> 가 id 목록을 <c>Items</c> 와 <c>Categories</c> 양쪽으로 보고
/// <c>ItemTemplate.IsChildOf</c> 로 부모 사슬을 탄다). 그런데 <c>Backpack</c> 분류를 넣고 게임에서
/// 배낭을 끌자 <b>필터 쪽 거부</b>(<c>UI/Trader/TransferLocked</c> — "Unavailable for transfer")가
/// 떴다. 코드만 보고 "분류 하나면 된다"고 단정한 것이 화면에서 뒤집힌 것이다.
///
/// 원인이 ① 클라이언트가 분류를 실제로는 못 타는 것인지 ② 그 배낭이 모드가 넣은 것이라 바닐라
/// <c>Backpack</c> 사슬에 없는 것인지는 아직 못 가렸다. <b>펼치기는 둘 다를 덮는다</b> — 서버의
/// 살아 있는 아이템 표에는 모드가 넣은 것까지 들어 있으므로, 거기서 자손을 모아 구체 id 로 적으면
/// 분류 판정에 기대지 않는다.
///
/// 뿌리(<c>Item</c>)는 펼치지 않는다 — 1만 개를 적게 되고, 그 경우는 분류 판정이 되든 안 되든
/// 다른 방법이 필요하다.
/// </summary>
public static class CategoryExpander
{
    /// <summary>이보다 많이 나오면 펼치지 않는다. 적어 봐야 득보다 실이 크다.</summary>
    public const int MaxExpandedPerCategory = 5000;

    /// <summary>
    /// <paramref name="categoryIds"/> 각각의 **자손 id**를 모은다. 분류 자신은 넣지 않는다
    /// (분류 id 는 아이템이 아니라서 창고에 들어올 일이 없다 — 호출하는 쪽이 따로 남긴다).
    /// </summary>
    /// <param name="parentById">아이템 표: id → 부모 id. 서버의 <c>templateTable.Items</c> 에서 만든다.</param>
    /// <param name="categoryIds">펼칠 분류 id.</param>
    /// <returns>자손 id. 들어온 순서대로, 중복 없이.</returns>
    public static List<string> Expand(IReadOnlyDictionary<string, string> parentById, IEnumerable<string> categoryIds)
    {
        ArgumentNullException.ThrowIfNull(parentById);
        ArgumentNullException.ThrowIfNull(categoryIds);

        var wanted = new HashSet<string>(categoryIds.Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.Ordinal);
        if (wanted.Count == 0)
        {
            return [];
        }

        // 뿌리는 제외한다 — 사실상 전부가 자손이다.
        wanted.Remove(CategoryCatalog.RootItem);
        if (wanted.Count == 0)
        {
            return [];
        }

        var found = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in parentById.Keys)
        {
            if (wanted.Contains(id))
            {
                continue;   // 분류 자신
            }

            if (HasAncestor(parentById, id, wanted) && seen.Add(id))
            {
                found.Add(id);
            }
        }

        return found.Count > MaxExpandedPerCategory * wanted.Count ? [] : found;
    }

    /// <summary>
    /// <paramref name="id"/> 의 부모 사슬에 <paramref name="wanted"/> 가 하나라도 있나.
    /// 사슬이 돌면(잘못된 데이터) 멈춘다.
    /// </summary>
    private static bool HasAncestor(IReadOnlyDictionary<string, string> parentById, string id, HashSet<string> wanted)
    {
        var walked = new HashSet<string>(StringComparer.Ordinal) { id };
        var current = id;

        while (parentById.TryGetValue(current, out var parent) && !string.IsNullOrEmpty(parent))
        {
            if (wanted.Contains(parent))
            {
                return true;
            }

            if (!walked.Add(parent))
            {
                return false;   // 사슬이 돈다
            }

            current = parent;
        }

        return false;
    }
}
