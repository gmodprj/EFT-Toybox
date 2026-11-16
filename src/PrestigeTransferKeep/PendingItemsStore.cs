namespace PrestigeTransferKeep;

/// <summary>
/// 프레스티지 처리가 도는 **그 한 번** 동안 "원래 넘기기로 했던 목록"을 들고 있는 자리.
///
/// 왜 필요한가 — 3332 Prestige Perks 가 `PrestigeHelper.ProcessPendingPrestige` 에 Prefix 를 걸고
/// 그 안에서 `prestige.Items` 를 **빈 목록으로 갈아치운다**(실측 2026-11-16: "vanilla transfer mail
/// suppressed"). 그래서 SPT 본체가 돌 때는 넘길 것이 이미 없다. 우리는 3332 보다 **먼저** 한 번
/// (목록을 떠 두고), **나중에** 한 번(도로 끼워) 끼어든다.
///
/// 열쇠는 `PendingPrestige` 객체 자체다 — 같은 처리 한 번 안에서 같은 인스턴스가 오간다.
/// 참조로 비교하므로 프로필이 여럿이어도 섞이지 않는다.
/// </summary>
public sealed class PendingItemsStore
{
    private readonly Dictionary<object, object> _saved = new(ReferenceEqualityComparer.Instance);

    /// <summary>들고 있는 건수. 시험과 로그에서 샌 것을 본다.</summary>
    public int Count => _saved.Count;

    /// <summary>
    /// 목록을 떠 둔다. <paramref name="items"/> 가 null 이거나 이미 떠 둔 것이 있으면 **덮지 않는다**
    /// — 같은 처리에서 두 번 불려도 처음 값이 남아야 한다.
    /// </summary>
    public void Save(object key, object? items)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (items is null)
        {
            return;
        }

        _saved.TryAdd(key, items);
    }

    /// <summary>
    /// 떠 둔 것을 돌려주고 **자리에서 치운다**. 없으면 null.
    /// 치우는 이유는 한 번 쓰고 나면 들고 있을 이유가 없고, 들고 있으면 프로필 객체가 영원히
    /// 메모리에 남기 때문이다.
    /// </summary>
    public object? TakeBack(object key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_saved.TryGetValue(key, out var items))
        {
            return null;
        }

        _saved.Remove(key);
        return items;
    }

    /// <summary>떠 둔 것을 모두 버린다.</summary>
    public void Clear() => _saved.Clear();
}
