namespace FriendshipAssistant.UI;

public readonly record struct GiftMenuTooltipPoint(int X, int Y);

public readonly record struct GiftMenuTooltipBounds(GiftMenuTooltipPoint Position, GiftMenuTooltipSize Size)
{
    public int X => this.Position.X;

    public int Y => this.Position.Y;

    public int Width => this.Size.Width;

    public int Height => this.Size.Height;

    public int Right => this.X + this.Width;

    public int Bottom => this.Y + this.Height;
}

public readonly record struct GiftMenuTooltipSize(int Width, int Height);

public static class GiftMenuTooltipLayout
{
    private const int Gap = 12;

    public static GiftMenuTooltipBounds Place(
        GiftMenuTooltipBounds focusedSlot,
        GiftMenuTooltipBounds availableBounds,
        GiftMenuTooltipSize tooltipSize)
    {
        if (availableBounds.Width <= 0 || availableBounds.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(availableBounds));
        if (tooltipSize.Width <= 0 || tooltipSize.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(tooltipSize));

        int width = Math.Min(tooltipSize.Width, availableBounds.Width);
        int height = Math.Min(tooltipSize.Height, availableBounds.Height);
        int rightX = focusedSlot.Right + Gap;
        int x = rightX + width <= availableBounds.Right
            ? rightX
            : focusedSlot.X - Gap - width;

        x = Math.Clamp(x, availableBounds.X, availableBounds.Right - width);
        int y = Math.Clamp(focusedSlot.Y, availableBounds.Y, availableBounds.Bottom - height);
        return new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(x, y), new GiftMenuTooltipSize(width, height));
    }
}
