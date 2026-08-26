using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuTooltipLayoutTests
{
    [Fact]
    public void Place_UsesRightSideWhenItFits()
    {
        GiftMenuTooltipBounds result = GiftMenuTooltipLayout.Place(
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(100, 100), new GiftMenuTooltipSize(64, 64)),
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(0, 0), new GiftMenuTooltipSize(500, 400)),
            new GiftMenuTooltipSize(180, 100));

        Assert.Equal(
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(176, 100), new GiftMenuTooltipSize(180, 100)),
            result);
    }

    [Fact]
    public void Place_UsesLeftSideWhenRightDoesNotFit()
    {
        GiftMenuTooltipBounds result = GiftMenuTooltipLayout.Place(
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(350, 100), new GiftMenuTooltipSize(64, 64)),
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(0, 0), new GiftMenuTooltipSize(500, 400)),
            new GiftMenuTooltipSize(180, 100));

        Assert.Equal(
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(158, 100), new GiftMenuTooltipSize(180, 100)),
            result);
    }

    [Fact]
    public void Place_ClampsToAvailableBounds()
    {
        GiftMenuTooltipBounds result = GiftMenuTooltipLayout.Place(
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(10, 370), new GiftMenuTooltipSize(64, 64)),
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(20, 30), new GiftMenuTooltipSize(300, 370)),
            new GiftMenuTooltipSize(400, 120));

        Assert.Equal(
            new GiftMenuTooltipBounds(new GiftMenuTooltipPoint(20, 280), new GiftMenuTooltipSize(300, 120)),
            result);
    }
}
