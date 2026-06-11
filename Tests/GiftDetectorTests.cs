using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftDetectorTests
{
    [Fact]
    public void Analyze_GroupsCandidatesByTasteAndSkipsEmptyStacks()
    {
        GiftCandidate loved = new("74", "Prismatic Shard", GiftTaste.Loved, 0, 1, 2000);
        GiftCandidate liked = new("66", "Diamond", GiftTaste.Liked, 0, 1, 750);
        GiftCandidate empty = new("168", "Trash", GiftTaste.Hated, 0, 0, 0);

        GiftAnalysisResult result = new GiftDetector(new GiftSelectionService()).Analyze(
            new[] { loved, liked, empty });

        Assert.Single(result.Loved);
        Assert.Single(result.Liked);
        Assert.Empty(result.Hated);
        Assert.True(result.HasAnyGift);
    }
}
