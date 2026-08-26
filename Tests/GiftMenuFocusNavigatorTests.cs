using FriendshipAssistant.Models;
using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuFocusNavigatorTests
{
    [Fact]
    public void MoveDown_SkipsHeaderAndClampsColumnOnShortRow()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(3, 0, 1);

        GiftMenuFocusResult result = GiftMenuFocusNavigator.MoveVertical(
            Request(rows, new GiftMenuNavigationPosition(0, 2, 0), visibleRows: 4),
            direction: 1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.Equal(2, result.RowIndex);
        Assert.Equal(0, result.Column);
        Assert.Equal(0, result.ScrollOffset);
    }

    [Fact]
    public void MoveDown_ScrollsWhenNextGiftRowIsBelowViewport()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(2, 0, 2, 0, 2);

        GiftMenuFocusResult result = GiftMenuFocusNavigator.MoveVertical(
            Request(rows, new GiftMenuNavigationPosition(2, 1, 0), visibleRows: 3),
            direction: 1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.Equal(4, result.RowIndex);
        Assert.Equal(1, result.Column);
        Assert.Equal(2, result.ScrollOffset);
    }

    [Fact]
    public void MoveDown_FromPageFinalGiftRowKeepsFocusInsideGiftGrid()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(2, 0, 1);

        GiftMenuFocusResult result = GiftMenuFocusNavigator.MoveVertical(
            Request(rows, new GiftMenuNavigationPosition(2, 0, 0), visibleRows: 4),
            direction: 1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.Equal(2, result.RowIndex);
        Assert.Equal(0, result.Column);
        Assert.Equal(0, result.ScrollOffset);
    }

    [Fact]
    public void MoveUp_FromFirstGiftRowKeepsFocusInsideGiftGrid()
    {
        GiftMenuFocusResult result = GiftMenuFocusNavigator.MoveVertical(
            Request(Rows(2), new GiftMenuNavigationPosition(0, 0, 0), visibleRows: 4),
            direction: -1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.Equal(0, result.RowIndex);
        Assert.Equal(0, result.Column);
        Assert.Equal(0, result.ScrollOffset);
    }

    [Fact]
    public void ScrollPage_PreservesColumnAndClampsAtBottom()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(3, 0, 3, 0, 1);

        GiftMenuFocusResult result = GiftMenuFocusNavigator.ScrollPage(
            Request(rows, new GiftMenuNavigationPosition(2, 2, 0), visibleRows: 3),
            direction: 1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.Equal(4, result.RowIndex);
        Assert.Equal(0, result.Column);
        Assert.Equal(2, result.ScrollOffset);
    }

    [Fact]
    public void ScrollPage_WhenTargetRowIsHeader_KeepsFocusInsideNewViewport()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1);

        GiftMenuFocusResult result = GiftMenuFocusNavigator.ScrollPage(
            Request(rows, new GiftMenuNavigationPosition(8, 0, 8), visibleRows: 4),
            direction: -1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.InRange(result.RowIndex, 4, 7);
        Assert.Equal(4, result.ScrollOffset);
    }

    [Fact]
    public void GetPageScrollAnchor_WhenChromeIsFocused_UsesFirstVisibleGift()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(0, 2, 0, 1, 1);
        GiftMenuNavigationRequest request = Request(
            rows,
            new GiftMenuNavigationPosition(-1, 0, 1),
            visibleRows: 3);

        GiftMenuNavigationPosition? result = GiftMenuFocusNavigator.GetPageScrollAnchor(request);

        Assert.Equal(new GiftMenuNavigationPosition(1, 0, 1), result);
    }

    [Fact]
    public void MoveWithinViewport_DoesNotScrollPastVisibleGiftRows()
    {
        IReadOnlyList<GiftGridDisplayRow> rows = Rows(1, 1, 1, 1, 1, 1);

        GiftMenuFocusResult result = GiftMenuFocusNavigator.MoveWithinViewport(
            Request(rows, new GiftMenuNavigationPosition(3, 0, 1), visibleRows: 3),
            direction: 1);

        Assert.Equal(GiftMenuFocusTarget.Gift, result.Target);
        Assert.Equal(3, result.RowIndex);
        Assert.Equal(1, result.ScrollOffset);
    }

    private static GiftMenuNavigationRequest Request(
        IReadOnlyList<GiftGridDisplayRow> rows,
        GiftMenuNavigationPosition position,
        int visibleRows)
    {
        return new GiftMenuNavigationRequest(rows, position, visibleRows);
    }

    private static IReadOnlyList<GiftGridDisplayRow> Rows(params int[] slotCounts)
    {
        List<GiftGridDisplayRow> rows = new();
        int itemId = 0;
        foreach (int slotCount in slotCounts)
        {
            if (slotCount == 0)
            {
                rows.Add(GiftGridDisplayRow.Header("Group", 30));
                continue;
            }

            List<GiftMenuSlot> slots = new();
            for (int column = 0; column < slotCount; column++)
            {
                GiftCandidate candidate = new((itemId++).ToString(), "Item", GiftTaste.Liked, 0, 1, 10);
                slots.Add(new GiftMenuSlot(candidate, column * 70, 0, 64, false));
            }
            rows.Add(GiftGridDisplayRow.Items(slots, 74));
        }
        return rows;
    }
}
