using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuComponentNavigationTests
{
    private static readonly GiftMenuNavigationPosition[] Positions =
    {
        new(0, 0, 0),
        new(0, 1, 0),
        new(1, 0, 0),
        new(1, 1, 0)
    };

    [Fact]
    public void GetComponentIdOrIgnore_WhenNeighborIsMissingReturnsStardewIgnoreId()
    {
        int result = GiftMenuComponentNavigation.GetComponentIdOrIgnore(
            position: null,
            _ => 42);

        Assert.Equal(-500, result);
    }

    [Fact]
    public void GetComponentIdOrIgnore_WhenNeighborExistsReturnsItsComponentId()
    {
        int result = GiftMenuComponentNavigation.GetComponentIdOrIgnore(
            new GiftMenuNavigationPosition(2, 3, 0),
            position => 1000 + position.RowIndex * 7 + position.Column);

        Assert.Equal(1017, result);
    }

    [Theory]
    [InlineData(0, 0, -1)]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, -1)]
    [InlineData(1, 1, 1)]
    public void FindHorizontalNeighbor_AtOuterEdgeReturnsNoNeighbor(
        int row,
        int column,
        int direction)
    {
        GiftMenuNavigationPosition? result = GiftMenuComponentNavigation.FindHorizontalNeighbor(
            Positions,
            new GiftMenuNavigationPosition(row, column, 0),
            direction);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, 1)]
    [InlineData(1, 1, -1, 1, 0)]
    public void FindHorizontalNeighbor_TowardGridReturnsAdjacentGift(
        int row,
        int column,
        int direction,
        int expectedRow,
        int expectedColumn)
    {
        GiftMenuNavigationPosition? result = GiftMenuComponentNavigation.FindHorizontalNeighbor(
            Positions,
            new GiftMenuNavigationPosition(row, column, 0),
            direction);

        Assert.Equal(new GiftMenuNavigationPosition(expectedRow, expectedColumn, 0), result);
    }

    [Theory]
    [InlineData(0, 0, -1)]
    [InlineData(0, 1, -1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 1)]
    public void FindVerticalNeighbor_AtOuterEdgeReturnsNoNeighbor(
        int row,
        int column,
        int direction)
    {
        GiftMenuNavigationPosition? result = GiftMenuComponentNavigation.FindVerticalNeighbor(
            Positions,
            new GiftMenuNavigationPosition(row, column, 0),
            direction);

        Assert.Null(result);
    }

    [Fact]
    public void FindVerticalNeighbor_TowardGridReturnsClosestColumn()
    {
        GiftMenuNavigationPosition[] positions =
        {
            new(0, 0, 0),
            new(0, 1, 0),
            new(1, 0, 0)
        };

        GiftMenuNavigationPosition? result = GiftMenuComponentNavigation.FindVerticalNeighbor(
            positions,
            new GiftMenuNavigationPosition(0, 1, 0),
            direction: 1);

        Assert.Equal(new GiftMenuNavigationPosition(1, 0, 0), result);
    }
}
