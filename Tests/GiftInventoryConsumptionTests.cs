using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftInventoryConsumptionTests
{
    [Fact]
    public void ConsumeOne_DecrementsStackedGift()
    {
        GiftInventoryConsumption.ConsumeResult result = GiftInventoryConsumption.ConsumeOne(stack: 3);

        Assert.False(result.RemoveSlot);
        Assert.Equal(2, result.NewStack);
    }

    [Fact]
    public void ConsumeOne_RemovesSingleGiftStack()
    {
        GiftInventoryConsumption.ConsumeResult result = GiftInventoryConsumption.ConsumeOne(stack: 1);

        Assert.True(result.RemoveSlot);
        Assert.Equal(0, result.NewStack);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConsumeOne_RejectsEmptyStacks(int stack)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GiftInventoryConsumption.ConsumeOne(stack));
    }

}
