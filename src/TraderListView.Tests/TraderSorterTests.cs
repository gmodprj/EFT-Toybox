using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class TraderSorterTests
{
    private static OfferRow Row(string name, double? price = 100, long stock = 1, int loyalty = 1, int index = 0) =>
        new OfferRow(name, price, stock, loyalty, false, index, null);

    private static string[] Names(IEnumerable<OfferRow> rows) => rows.Select(r => r.Name).ToArray();

    private static SortSpec Spec(SortKey key, bool desc = false) => new SortSpec(key, desc);

    [Fact]
    public void Sort_Default_KeepsDefaultIndexOrder()
    {
        var rows = new[] { Row("c", index: 2), Row("a", index: 0), Row("b", index: 1) };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Default));

        Assert.Equal(new[] { "a", "b", "c" }, Names(sorted));
    }

    [Fact]
    public void Sort_PriceAsc_ThenNameThenDefaultIndex()
    {
        var rows = new[]
        {
            Row("zeta", 50, index: 0),
            Row("alpha", 10, index: 1),
            Row("beta", 50, index: 2),
            Row("beta", 50, index: 3),
        };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Price));

        Assert.Equal(new[] { "alpha", "beta", "beta", "zeta" }, Names(sorted));
        Assert.Equal(new[] { 1, 2, 3, 0 }, sorted.Select(r => r.DefaultIndex).ToArray());
    }

    [Fact]
    public void Sort_PriceDesc_ReversesPriceOnly()
    {
        var rows = new[] { Row("b", 50, index: 0), Row("a", 50, index: 1), Row("c", 90, index: 2) };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Price, desc: true));

        // 가격만 내림, 동률(50)은 여전히 이름 오름
        Assert.Equal(new[] { "c", "a", "b" }, Names(sorted));
    }

    [Fact]
    public void Sort_UnknownPrice_LastInBothDirections()
    {
        var rows = new[] { Row("y", null), Row("m", 20), Row("x", null), Row("n", 10) };

        var asc = TraderSorter.Sort(rows, Spec(SortKey.Price));
        var desc = TraderSorter.Sort(rows, Spec(SortKey.Price, desc: true));

        Assert.Equal(new[] { "n", "m", "x", "y" }, Names(asc));
        Assert.Equal(new[] { "m", "n", "x", "y" }, Names(desc));
    }

    [Fact]
    public void Sort_Name_IsCaseInsensitiveOrdinal()
    {
        var rows = new[] { Row("AK", index: 1), Row("ak", index: 0), Row("Bolt", index: 2) };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Name));

        Assert.Equal(new[] { 0, 1, 2 }, sorted.Select(r => r.DefaultIndex).ToArray());
    }

    [Fact]
    public void Sort_Stock_Desc()
    {
        var rows = new[] { Row("a", stock: 5), Row("b", stock: 999999), Row("c", stock: 50) };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Stock, desc: true));

        Assert.Equal(new[] { "b", "c", "a" }, Names(sorted));
    }

    [Fact]
    public void Sort_Loyalty_Asc()
    {
        var rows = new[] { Row("a", loyalty: 3), Row("b", loyalty: 1), Row("c", loyalty: 2) };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Loyalty));

        Assert.Equal(new[] { "b", "c", "a" }, Names(sorted));
    }

    [Fact]
    public void Sort_EmptyInput_ReturnsEmpty()
    {
        var sorted = TraderSorter.Sort(Array.Empty<OfferRow>(), Spec(SortKey.Price));

        Assert.Empty(sorted);
    }

    [Fact]
    public void Sort_DuplicateNames_StableByDefaultIndex()
    {
        var rows = new[] { Row("same", 10, index: 3), Row("same", 10, index: 1), Row("same", 10, index: 2) };

        var sorted = TraderSorter.Sort(rows, Spec(SortKey.Name, desc: true));

        Assert.Equal(new[] { 1, 2, 3 }, sorted.Select(r => r.DefaultIndex).ToArray());
    }
}
