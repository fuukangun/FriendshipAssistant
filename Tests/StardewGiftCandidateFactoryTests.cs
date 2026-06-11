using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class StardewGiftCandidateFactoryTests
{
    [Theory]
    [InlineData(0, GiftTaste.Loved)]
    [InlineData(2, GiftTaste.Liked)]
    [InlineData(8, GiftTaste.Neutral)]
    [InlineData(4, GiftTaste.Disliked)]
    [InlineData(6, GiftTaste.Hated)]
    public void MapTaste_UsesStardewGiftTasteReturnCodes(int rawTaste, GiftTaste expected)
    {
        Assert.Equal(expected, StardewGiftCandidateFactory.MapTaste(rawTaste));
    }
}
