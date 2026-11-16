using PrestigeTransferKeep;
using Xunit;

namespace PrestigeTransferKeep.Tests;

/// <summary>
/// <see cref="PendingItemsStore"/> 의 판정. 검사마다 다른 사유를 본다.
/// </summary>
public class PendingItemsStoreTests
{
    private sealed class Key;

    [Fact]
    public void 떠_둔_것을_돌려준다()
    {
        var store = new PendingItemsStore();
        var key = new Key();
        var items = new List<string> { "a" };

        store.Save(key, items);

        Assert.Same(items, store.TakeBack(key));
    }

    [Fact]
    public void 한_번_돌려주면_자리에서_치운다()
    {
        // 들고 있으면 프로필 객체가 영원히 메모리에 남는다.
        var store = new PendingItemsStore();
        var key = new Key();
        store.Save(key, new List<string>());

        store.TakeBack(key);

        Assert.Null(store.TakeBack(key));
        Assert.Equal(0, store.Count);
    }

    [Fact]
    public void 두_번_떠도_처음_것이_남는다()
    {
        // 같은 처리에서 두 번 불려도 3332 가 비우기 전의 값이어야 한다.
        var store = new PendingItemsStore();
        var key = new Key();
        var first = new List<string> { "처음" };
        var second = new List<string> { "나중" };

        store.Save(key, first);
        store.Save(key, second);

        Assert.Same(first, store.TakeBack(key));
    }

    [Fact]
    public void null_은_떠_두지_않는다()
    {
        var store = new PendingItemsStore();
        var key = new Key();

        store.Save(key, null);

        Assert.Equal(0, store.Count);
        Assert.Null(store.TakeBack(key));
    }

    [Fact]
    public void 열쇠는_참조로_가린다()
    {
        // 값이 같은 다른 객체가 남의 목록을 가져가면 프로필이 섞인다.
        var store = new PendingItemsStore();
        var a = new List<int> { 1 };
        var b = new List<int> { 1 };   // a 와 같은 값, 다른 객체
        var items = new List<string> { "a 의 것" };

        store.Save(a, items);

        Assert.Null(store.TakeBack(b));
        Assert.Same(items, store.TakeBack(a));
    }

    [Fact]
    public void 프로필이_여럿이어도_섞이지_않는다()
    {
        var store = new PendingItemsStore();
        var k1 = new Key();
        var k2 = new Key();
        var i1 = new List<string> { "하나" };
        var i2 = new List<string> { "둘" };

        store.Save(k1, i1);
        store.Save(k2, i2);

        Assert.Equal(2, store.Count);
        Assert.Same(i2, store.TakeBack(k2));
        Assert.Same(i1, store.TakeBack(k1));
    }

    [Fact]
    public void 떠_두지_않은_열쇠는_null_이다()
    {
        var store = new PendingItemsStore();

        Assert.Null(store.TakeBack(new Key()));
    }

    [Fact]
    public void 비우면_전부_사라진다()
    {
        var store = new PendingItemsStore();
        store.Save(new Key(), new List<string>());
        store.Save(new Key(), new List<string>());

        store.Clear();

        Assert.Equal(0, store.Count);
    }

    [Fact]
    public void 열쇠가_null_이면_막는다()
    {
        var store = new PendingItemsStore();

        Assert.Throws<ArgumentNullException>(() => store.Save(null!, new List<string>()));
        Assert.Throws<ArgumentNullException>(() => store.TakeBack(null!));
    }
}
