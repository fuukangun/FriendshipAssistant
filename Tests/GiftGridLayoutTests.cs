using FriendshipAssistant.Models;
using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftGridLayoutTests
{
    [Fact]
    public void BuildSlots_WrapsCandidatesAcrossConfiguredColumns()
    {
        GiftMenuModel model = new(new[]
        {
            Row("header"),
            Row(Candidate("0")),
            Row(Candidate("1")),
            Row(Candidate("2")),
            Row(Candidate("3")),
            Row(Candidate("4"))
        });

        GiftGridLayout layout = new(columns: 4, slotSize: 64, slotSpacing: 12);
        IReadOnlyList<GiftMenuSlot> slots = layout.BuildSlots(model.Rows, originX: 10, originY: 20);

        Assert.Collection(
            slots,
            slot => Assert.Equal((10, 20), (slot.X, slot.Y)),
            slot => Assert.Equal((86, 20), (slot.X, slot.Y)),
            slot => Assert.Equal((162, 20), (slot.X, slot.Y)),
            slot => Assert.Equal((238, 20), (slot.X, slot.Y)),
            slot => Assert.Equal((10, 96), (slot.X, slot.Y)));
    }

    [Fact]
    public void HitTest_ReturnsCandidateInsideSlot()
    {
        GiftCandidate candidate = Candidate("0");
        GiftGridLayout layout = new(columns: 4, slotSize: 64, slotSpacing: 12);
        IReadOnlyList<GiftMenuSlot> slots = new[]
        {
            new GiftMenuSlot(candidate, X: 10, Y: 20, Size: 64, IsLastGift: false)
        };

        GiftCandidate? hit = layout.HitTest(slots, x: 40, y: 50);

        Assert.Same(candidate, hit);
    }

    private static GiftMenuRow Row(string text)
    {
        return new GiftMenuRow(null, text, IsHeader: true, IsLastGift: false);
    }

    private static GiftMenuRow Row(GiftCandidate candidate)
    {
        return new GiftMenuRow(candidate, candidate.DisplayName, IsHeader: false, IsLastGift: false);
    }

    private static GiftCandidate Candidate(string id)
    {
        return new GiftCandidate(id, $"Item {id}", GiftTaste.Liked, 0, 1, 100);
    }
}
