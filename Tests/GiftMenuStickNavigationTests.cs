using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuStickNavigationTests
{
    [Fact]
    public void GetComponentCenter_TargetsFocusedGiftCenter()
    {
        Assert.Equal(
            new GiftMenuPointerPosition(132, 232),
            GiftMenuPointerNavigator.GetComponentCenter(100, 200, 64, 64));
    }

    [Theory]
    [InlineData(GiftMenuFocusTarget.TopChrome, 1, GiftMenuFocusTarget.Gift)]
    [InlineData(GiftMenuFocusTarget.Gift, 1, GiftMenuFocusTarget.Dismiss)]
    [InlineData(GiftMenuFocusTarget.Dismiss, 1, GiftMenuFocusTarget.Dismiss)]
    [InlineData(GiftMenuFocusTarget.Dismiss, -1, GiftMenuFocusTarget.Gift)]
    [InlineData(GiftMenuFocusTarget.Gift, -1, GiftMenuFocusTarget.TopChrome)]
    [InlineData(GiftMenuFocusTarget.TopChrome, -1, GiftMenuFocusTarget.TopChrome)]
    public void MoveRegion_ChangesOnlyBetweenMenuRegions(
        GiftMenuFocusTarget current,
        int direction,
        GiftMenuFocusTarget expected)
    {
        Assert.Equal(expected, GiftMenuRegionNavigator.MoveRegion(current, direction));
    }

    [Theory]
    [InlineData(100, 100, 1, 0, 116, 100)]
    [InlineData(100, 100, -1, 0, 84, 100)]
    [InlineData(100, 100, 0, 1, 100, 116)]
    public void MovePointer_UsesStableScreenStep(
        int x,
        int y,
        int horizontal,
        int vertical,
        int expectedX,
        int expectedY)
    {
        Assert.Equal(
            new GiftMenuPointerPosition(expectedX, expectedY),
            GiftMenuPointerNavigator.MovePointer(new GiftMenuPointerPosition(x, y), horizontal, vertical, 16));
    }
}
