using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuFocusResolverTests
{
    [Fact]
    public void ResolveFirst_ReturnsTopLeftGiftRegardlessOfCandidateOrder()
    {
        IReadOnlyList<GiftMenuFocusCandidate> candidates = new[]
        {
            Candidate(new object(), new GiftMenuNavigationPosition(3, 1, 0), componentId: 30),
            Candidate(new object(), new GiftMenuNavigationPosition(1, 2, 0), componentId: 12),
            Candidate(new object(), new GiftMenuNavigationPosition(1, 0, 0), componentId: 10)
        };

        GiftMenuFocusCandidate? result = GiftMenuFocusResolver.ResolveFirst(candidates);

        Assert.Equal(10, result?.ComponentId);
    }

    [Fact]
    public void ResolveFirst_WhenPageHasNoGiftReturnsNull()
    {
        GiftMenuFocusCandidate? result = GiftMenuFocusResolver.ResolveFirst(
            Array.Empty<GiftMenuFocusCandidate>());

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_PrefersSavedIdentityAfterPositionChanges()
    {
        object saved = new();
        IReadOnlyList<GiftMenuFocusCandidate> candidates = new[]
        {
            Candidate(new object(), new GiftMenuNavigationPosition(1, 0, 0), componentId: 10),
            Candidate(saved, new GiftMenuNavigationPosition(3, 1, 0), componentId: 20)
        };

        GiftMenuFocusCandidate? result = GiftMenuFocusResolver.Resolve(
            candidates,
            saved,
            new GiftMenuNavigationPosition(1, 0, 0));

        Assert.Equal(20, result?.ComponentId);
    }

    [Fact]
    public void Resolve_WhenSavedIdentityIsGone_UsesNearestPosition()
    {
        IReadOnlyList<GiftMenuFocusCandidate> candidates = new[]
        {
            Candidate(new object(), new GiftMenuNavigationPosition(1, 0, 0), componentId: 10),
            Candidate(new object(), new GiftMenuNavigationPosition(2, 2, 0), componentId: 20)
        };

        GiftMenuFocusCandidate? result = GiftMenuFocusResolver.Resolve(
            candidates,
            new object(),
            new GiftMenuNavigationPosition(2, 1, 0));

        Assert.Equal(20, result?.ComponentId);
    }

    private static GiftMenuFocusCandidate Candidate(
        object identity,
        GiftMenuNavigationPosition position,
        int componentId)
    {
        return new GiftMenuFocusCandidate(identity, position, componentId);
    }
}
