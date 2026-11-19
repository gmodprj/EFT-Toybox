using TraderListView;
using Xunit;

namespace TraderListView.Tests
{
    public class RowLookTests
    {
        [Fact]
        public void ExaminedItemShowsItsRealName()
        {
            Assert.Equal("Gold Deer Skull", RowLook.Name(true, "Gold Deer Skull", "알 수 없는 아이템"));
        }

        [Fact]
        public void UnexaminedItemHidesItsNameLikeTheGrid()
        {
            Assert.Equal("알 수 없는 아이템", RowLook.Name(false, "Gold Deer Skull", "알 수 없는 아이템"));
        }

        [Fact]
        public void ExaminedIconIsDrawnAsIs()
        {
            Assert.Equal((1f, 1f, 1f, 1f), RowLook.IconTint(true));
        }

        [Fact]
        public void UnexaminedIconIsASilhouette()
        {
            // 게임이 미조사 물건을 끌 때 쓰는 색과 같다(ItemView.OnBeginDrag: new Color(0,0,0,0.85f)).
            Assert.Equal((0f, 0f, 0f, 0.85f), RowLook.IconTint(false));
        }

        [Theory]
        [InlineData(0f, 4f, 0f)]
        [InlineData(1f, 4f, 0.25f)]
        [InlineData(4f, 4f, 1f)]
        [InlineData(9f, 4f, 1f)]
        [InlineData(-1f, 4f, 0f)]
        [InlineData(1f, 0f, 1f)]
        public void ExamineProgressFillsOverTheExamineTime(float elapsed, float duration, float expected)
        {
            Assert.Equal(expected, RowLook.ExamineProgress(elapsed, duration), 3);
        }

        [Theory]
        [InlineData(RowLook.Button.Left, 1, RowLook.Action.Select)]
        [InlineData(RowLook.Button.Left, 2, RowLook.Action.Inspect)]
        [InlineData(RowLook.Button.Left, 3, RowLook.Action.Inspect)]
        [InlineData(RowLook.Button.Right, 1, RowLook.Action.ContextMenu)]
        [InlineData(RowLook.Button.Right, 2, RowLook.Action.ContextMenu)]
        [InlineData(RowLook.Button.Middle, 1, RowLook.Action.None)]
        public void ClickRoutesLikeTheGrid(RowLook.Button button, int clickCount, RowLook.Action expected)
        {
            Assert.Equal(expected, RowLook.Route(button, clickCount));
        }
    }
}
