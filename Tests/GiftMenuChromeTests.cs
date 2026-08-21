using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuChromeTests
{
    [Fact]
    public void GetCloseButtonBounds_PositionsButtonInsideDialogueCorner()
    {
        GiftMenuChrome.CloseButtonBounds bounds = GiftMenuChrome.GetCloseButtonBounds(
            menuX: 100,
            menuY: 200,
            menuWidth: 720,
            buttonSize: 48,
            inset: 28);

        Assert.Equal(744, bounds.X);
        Assert.Equal(228, bounds.Y);
        Assert.Equal(48, bounds.Width);
        Assert.Equal(48, bounds.Height);
    }

    [Fact]
    public void GetDismissButtonBounds_CentersCompactButtonAboveBottomBorder()
    {
        GiftMenuChrome.ActionButtonBounds bounds = GiftMenuChrome.GetDismissButtonBounds(
            menuX: 100,
            menuY: 200,
            menuWidth: 720,
            menuHeight: 560,
            buttonWidth: 220,
            buttonHeight: 40,
            bottomInset: 34);

        Assert.Equal(350, bounds.X);
        Assert.Equal(686, bounds.Y);
        Assert.Equal(220, bounds.Width);
        Assert.Equal(40, bounds.Height);
        Assert.True(bounds.Y + bounds.Height < 760);
    }
}
