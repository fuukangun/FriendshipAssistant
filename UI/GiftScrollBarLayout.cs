namespace FriendshipAssistant.UI;

public static class GiftScrollBarLayout
{
    public readonly record struct ScrollBarState(
        int TrackX,
        int TrackY,
        int TrackWidth,
        int TrackHeight,
        int ThumbY,
        int ThumbHeight,
        bool IsVisible);

    public static ScrollBarState Calculate(
        int trackX,
        int trackY,
        int trackWidth,
        int trackHeight,
        int totalRows,
        int visibleRows,
        int scrollOffset,
        int contentInset = 0)
    {
        int maxOffset = Math.Max(0, totalRows - visibleRows);
        int innerY = trackY + Math.Max(0, contentInset);
        int innerHeight = Math.Max(1, trackHeight - Math.Max(0, contentInset) * 2);
        if (maxOffset == 0)
            return new ScrollBarState(trackX, trackY, trackWidth, trackHeight, innerY, innerHeight, IsVisible: false);

        int thumbHeight = Math.Max(32, innerHeight * visibleRows / totalRows);
        int travel = Math.Max(1, innerHeight - thumbHeight);
        int clampedOffset = Math.Clamp(scrollOffset, 0, maxOffset);
        int thumbY = innerY + travel * clampedOffset / maxOffset;

        return new ScrollBarState(trackX, trackY, trackWidth, trackHeight, thumbY, thumbHeight, IsVisible: true);
    }

    public static int ScrollOffsetFromThumbY(
        int thumbY,
        int trackY,
        int trackHeight,
        int thumbHeight,
        int totalRows,
        int visibleRows,
        int contentInset = 0)
    {
        int maxOffset = Math.Max(0, totalRows - visibleRows);
        if (maxOffset == 0)
            return 0;

        int innerY = trackY + Math.Max(0, contentInset);
        int innerHeight = Math.Max(1, trackHeight - Math.Max(0, contentInset) * 2);
        int travel = Math.Max(1, innerHeight - thumbHeight);
        int clampedThumbY = Math.Clamp(thumbY, innerY, innerY + travel);
        return (clampedThumbY - innerY) * maxOffset / travel;
    }
}
