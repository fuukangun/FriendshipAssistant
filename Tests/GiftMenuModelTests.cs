using FriendshipAssistant.Models;
using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuModelTests
{
    [Fact]
    public void BuildRows_AddsHeadersAndLastGiftMarker()
    {
        GiftAnalysisResult result = new()
        {
            Loved = new[]
            {
                new GiftCandidate("74", "Prismatic Shard", GiftTaste.Loved, 0, 1, 2000)
            },
            Liked = new[]
            {
                new GiftCandidate("66", "Diamond", GiftTaste.Liked, 2, 1, 750)
            }
        };

        GiftMenuModel model = GiftMenuModel.FromAnalysis(
            result,
            lastGiftItemId: "66",
            categoryLabel: taste => taste.ToString(),
            lastGiftText: "Last gifted");

        Assert.Collection(
            model.Rows,
            row =>
            {
                Assert.True(row.IsHeader);
                Assert.Equal("Loved", row.Text);
            },
            row =>
            {
                Assert.False(row.IsHeader);
                Assert.False(row.IsLastGift);
                Assert.Equal("Prismatic Shard", row.Text);
            },
            row =>
            {
                Assert.True(row.IsHeader);
                Assert.Equal("Liked", row.Text);
            },
            row =>
            {
                Assert.False(row.IsHeader);
                Assert.True(row.IsLastGift);
                Assert.Equal("Diamond - Last gifted", row.Text);
            });
    }

    [Fact]
    public void GetVisibleRows_ClampsScrollOffset()
    {
        GiftMenuModel model = new(new[]
        {
            Row("0"),
            Row("1"),
            Row("2"),
            Row("3"),
            Row("4")
        });

        IReadOnlyList<GiftMenuRow> visible = model.GetVisibleRows(scrollOffset: 10, visibleCount: 2);

        Assert.Collection(
            visible,
            row => Assert.Equal("3", row.Text),
            row => Assert.Equal("4", row.Text));
    }

    private static GiftMenuRow Row(string text)
    {
        return new GiftMenuRow(null, text, IsHeader: false, IsLastGift: false);
    }
}
