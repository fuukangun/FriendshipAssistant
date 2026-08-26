namespace FriendshipAssistant.UI;

public static class GiftMenuRegionNavigator
{
    public static GiftMenuFocusTarget MoveRegion(GiftMenuFocusTarget current, int direction)
    {
        if (direction is not (-1 or 1))
            throw new ArgumentOutOfRangeException(nameof(direction));

        return (current, direction) switch
        {
            (GiftMenuFocusTarget.TopChrome, 1) => GiftMenuFocusTarget.Gift,
            (GiftMenuFocusTarget.Gift, 1) => GiftMenuFocusTarget.Dismiss,
            (GiftMenuFocusTarget.Dismiss, -1) => GiftMenuFocusTarget.Gift,
            (GiftMenuFocusTarget.Gift, -1) => GiftMenuFocusTarget.TopChrome,
            _ => current
        };
    }
}

public readonly record struct GiftMenuPointerPosition(int X, int Y);

public static class GiftMenuPointerNavigator
{
    public static GiftMenuPointerPosition GetComponentCenter(
        int x,
        int y,
        int width,
        int height)
    {
        return new GiftMenuPointerPosition(x + width / 2, y + height / 2);
    }

    public static GiftMenuPointerPosition MovePointer(
        GiftMenuPointerPosition current,
        int horizontal,
        int vertical,
        int step)
    {
        if (horizontal is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(horizontal));
        if (vertical is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(vertical));
        if (step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step));

        return new GiftMenuPointerPosition(
            current.X + horizontal * step,
            current.Y + vertical * step);
    }
}
