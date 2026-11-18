using TraderListView;
using Xunit;

namespace TraderListView.Tests;

public class SortSpecTests
{
    [Fact]
    public void Config_RoundTrips()
    {
        var spec = new SortSpec(SortKey.Price, descending: true);

        Assert.Equal("Price:Desc", spec.ToConfig());
        var back = SortSpec.Parse("Price:Desc");
        Assert.Equal(SortKey.Price, back.Key);
        Assert.True(back.Descending);
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("Price:Sideways")]
    public void Parse_Unreadable_IsDefaultAsc(string text)
    {
        var spec = SortSpec.Parse(text);

        Assert.Equal(SortKey.Default, spec.Key);
        Assert.False(spec.Descending);
    }
}
