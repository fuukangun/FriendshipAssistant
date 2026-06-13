namespace FriendshipAssistant.UI;

public static class GiftMenuChrome
{
    public readonly record struct CloseButtonBounds(int X, int Y, int Width, int Height);

    public static CloseButtonBounds GetCloseButtonBounds(
        int menuX,
        int menuY,
        int menuWidth,
        int buttonSize,
        int inset)
    {
        return new CloseButtonBounds(
            menuX + menuWidth - buttonSize - inset,
            menuY + inset,
            buttonSize,
            buttonSize);
    }
}
