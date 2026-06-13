using FriendshipAssistant.Models;
using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftGroupedGridLayoutTests
{
    [Fact]
    public void BuildRows_KeepsTasteHeadersAndWrapsItemsWithinEachGroup()
    {
        GiftGroupedGridLayout layout = new(columns: 3, slotSize: 64, slotSpacing: 12, headerHeight: 30, rowSpacing: 10);

        IReadOnlyList<GiftGridDisplayRow> rows = layout.BuildRows(new[]
        {
            Header("Loved"),
            Row(Candidate("0")),
            Row(Candidate("1")),
            Row(Candidate("2")),
            Row(Candidate("3")),
            Header("Liked"),
            Row(Candidate("4"))
        });

        Assert.Collection(
            rows,
            row =>
            {
                Assert.True(row.IsHeader);
                Assert.Equal("Loved", row.HeaderText);
            },
            row =>
            {
                Assert.False(row.IsHeader);
                Assert.Equal(new[] { "0", "1", "2" }, row.Slots.Select(slot => slot.Candidate.ItemId));
                Assert.Equal(new[] { 0, 76, 152 }, row.Slots.Select(slot => slot.X));
            },
            row =>
            {
                Assert.False(row.IsHeader);
                Assert.Single(row.Slots);
                Assert.Equal("3", row.Slots[0].Candidate.ItemId);
                Assert.Equal(0, row.Slots[0].X);
            },
            row =>
            {
                Assert.True(row.IsHeader);
                Assert.Equal("Liked", row.HeaderText);
            },
            row =>
            {
                Assert.False(row.IsHeader);
                Assert.Single(row.Slots);
                Assert.Equal("4", row.Slots[0].Candidate.ItemId);
                Assert.Equal(0, row.Slots[0].X);
            });
    }

    [Fact]
    public void PositionRows_AppliesAbsoluteOriginAndVerticalFlow()
    {
        GiftGroupedGridLayout layout = new(columns: 2, slotSize: 64, slotSpacing: 12, headerHeight: 30, rowSpacing: 10);

        IReadOnlyList<GiftGridDisplayRow> positioned = layout.PositionRows(
            layout.BuildRows(new[]
            {
                Header("Loved"),
                Row(Candidate("0")),
                Row(Candidate("1")),
                Header("Liked")
            }),
            originX: 100,
            originY: 200);

        Assert.Equal(200, positioned[0].Y);
        Assert.Equal(230, positioned[1].Y);
        Assert.Equal(304, positioned[2].Y);
        Assert.Equal(new[] { 100, 176 }, positioned[1].Slots.Select(slot => slot.X));
        Assert.All(positioned[1].Slots, slot => Assert.Equal(230, slot.Y));
    }

    [Fact]
    public void HitTest_ReturnsCandidateInsidePositionedSlot()
    {
        GiftGroupedGridLayout layout = new(columns: 2, slotSize: 64, slotSpacing: 12, headerHeight: 30, rowSpacing: 10);
        IReadOnlyList<GiftGridDisplayRow> rows = layout.PositionRows(
            layout.BuildRows(new[]
            {
                Header("Loved"),
                Row(Candidate("0"))
            }),
            originX: 100,
            originY: 200);

        GiftCandidate? hit = layout.HitTest(rows, x: 132, y: 250);

        Assert.NotNull(hit);
        Assert.Equal("0", hit.ItemId);
    }

    private static GiftMenuRow Header(string text)
    {
        return new GiftMenuRow(null, text, IsHeader: true, IsLastGift: false);
    }

    private static GiftMenuRow Row(GiftCandidate candidate)
    {
        return new GiftMenuRow(candidate, candidate.DisplayName, IsHeader: false, IsLastGift: false);
    }

    private static GiftCandidate Candidate(string id)
    {
        return new GiftCandidate(id, $"Item {id}", GiftTaste.Liked, 0, 1, 100);
    }
}
