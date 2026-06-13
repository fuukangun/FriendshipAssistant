using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class StardewGiftCandidateFactoryTests
{
    [Theory]
    [InlineData("StardewValley.Tools.Axe")]
    [InlineData("StardewValley.Tools.MeleeWeapon")]
    [InlineData("StardewValley.Tools.Slingshot")]
    public void IsBlockedGiftPromptItemTypeName_RejectsToolsAndWeapons(string typeName)
    {
        Assert.True(StardewGiftCandidateFactory.IsBlockedGiftPromptItemTypeName(typeName));
    }

    [Fact]
    public void IsBlockedGiftPromptItemTypeName_AllowsObjects()
    {
        Assert.False(StardewGiftCandidateFactory.IsBlockedGiftPromptItemTypeName("StardewValley.Object"));
    }

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
