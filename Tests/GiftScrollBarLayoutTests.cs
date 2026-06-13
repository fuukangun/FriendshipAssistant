using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftScrollBarLayoutTests
{
    [Fact]
    public void Calculate_HidesScrollBarWhenAllRowsFit()
    {
        GiftScrollBarLayout.ScrollBarState state = GiftScrollBarLayout.Calculate(
            trackX: 700,
            trackY: 160,
            trackWidth: 24,
            trackHeight: 300,
            totalRows: 5,
            visibleRows: 5,
            scrollOffset: 0);

        Assert.False(state.IsVisible);
    }

    [Fact]
    public void Calculate_MovesThumbByScrollOffset()
    {
        GiftScrollBarLayout.ScrollBarState state = GiftScrollBarLayout.Calculate(
            trackX: 700,
            trackY: 160,
            trackWidth: 24,
            trackHeight: 300,
            contentInset: 4,
            totalRows: 10,
            visibleRows: 5,
            scrollOffset: 5);

        Assert.True(state.IsVisible);
        Assert.Equal(146, state.ThumbHeight);
        Assert.Equal(310, state.ThumbY);
        Assert.Equal(state.TrackY + state.TrackHeight - 4, state.ThumbY + state.ThumbHeight);
    }

    [Fact]
    public void ScrollOffsetFromThumbY_ConvertsDraggedThumbPosition()
    {
        int offset = GiftScrollBarLayout.ScrollOffsetFromThumbY(
            thumbY: 310,
            trackY: 160,
            trackHeight: 300,
            contentInset: 4,
            thumbHeight: 146,
            totalRows: 10,
            visibleRows: 5);

        Assert.Equal(5, offset);
    }

    [Fact]
    public void Calculate_ClampsBottomOfThumbToInsideFrameWithoutExtraPadding()
    {
        GiftScrollBarLayout.ScrollBarState state = GiftScrollBarLayout.Calculate(
            trackX: 700,
            trackY: 160,
            trackWidth: 24,
            trackHeight: 300,
            contentInset: 4,
            totalRows: 20,
            visibleRows: 5,
            scrollOffset: 999);

        Assert.Equal(state.TrackY + state.TrackHeight - 4, state.ThumbY + state.ThumbHeight);
    }

    [Fact]
    public void Calculate_ClampsTopOfThumbToInsideFrameWithoutExtraPadding()
    {
        GiftScrollBarLayout.ScrollBarState state = GiftScrollBarLayout.Calculate(
            trackX: 700,
            trackY: 160,
            trackWidth: 24,
            trackHeight: 300,
            contentInset: 4,
            totalRows: 20,
            visibleRows: 5,
            scrollOffset: -999);

        Assert.Equal(state.TrackY + 4, state.ThumbY);
    }
}
