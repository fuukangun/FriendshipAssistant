using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftSelectionServiceTests
{
    [Fact]
    public void SelectAutoGift_ChoosesLovedBeforeHigherQualityLiked()
    {
        GiftAnalysisResult result = new()
        {
            Loved = new[]
            {
                new GiftCandidate("74", "Prismatic Shard", GiftTaste.Loved, 0, 1, 2000)
            },
            Liked = new[]
            {
                new GiftCandidate("66", "Diamond", GiftTaste.Liked, 4, 1, 750)
            }
        };

        GiftCandidate? selected = new GiftSelectionService().SelectAutoGift(result);

        Assert.NotNull(selected);
        Assert.Equal("74", selected.ItemId);
    }

    [Fact]
    public void SelectAutoGift_ExcludesDislikedAndHatedItems()
    {
        GiftAnalysisResult result = new()
        {
            Disliked = new[]
            {
                new GiftCandidate("168", "Trash", GiftTaste.Disliked, 4, 1, 0)
            },
            Hated = new[]
            {
                new GiftCandidate("330", "Clay", GiftTaste.Hated, 4, 1, 20)
            }
        };

        GiftCandidate? selected = new GiftSelectionService().SelectAutoGift(result);

        Assert.Null(selected);
    }

    [Fact]
    public void SortGroup_OrdersByQualityThenLowerSalePrice()
    {
        GiftCandidate normalExpensive = new("1", "Expensive", GiftTaste.Liked, 0, 1, 500);
        GiftCandidate goldCheap = new("2", "Cheap Gold", GiftTaste.Liked, 2, 1, 50);
        GiftCandidate normalCheap = new("3", "Cheap", GiftTaste.Liked, 0, 1, 10);

        IReadOnlyList<GiftCandidate> sorted = new GiftSelectionService().SortGroup(
            new[] { normalExpensive, goldCheap, normalCheap });

        Assert.Collection(
            sorted,
            candidate => Assert.Equal("2", candidate.ItemId),
            candidate => Assert.Equal("3", candidate.ItemId),
            candidate => Assert.Equal("1", candidate.ItemId));
    }
}
