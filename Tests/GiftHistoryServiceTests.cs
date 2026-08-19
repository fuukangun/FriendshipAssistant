using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftHistoryServiceTests
{
    [Fact]
    public void RecordGift_ReplacesLastGiftForNpc()
    {
        GiftHistoryService service = new();
        GiftCandidate first = new("74", "Prismatic Shard", GiftTaste.Loved, 0, 1, 2000);
        GiftCandidate second = new("66", "Diamond", GiftTaste.Liked, 2, 1, 750);

        service.RecordGift("Abigail", first, "spring", 1);
        service.RecordGift("Abigail", second, "spring", 2);

        LastGiftEntry? entry = service.GetLastGift("Abigail");

        Assert.NotNull(entry);
        Assert.Equal("66", entry.ItemId);
        Assert.Equal(2, entry.Day);
    }

    [Fact]
    public void Import_NullDataStartsEmpty()
    {
        GiftHistoryService service = new();

        service.Import(null);

        Assert.Null(service.GetLastGift("Abigail"));
    }

    [Fact]
    public void SuppressPrompt_MatchesNpcAndDate()
    {
        GiftHistoryService service = new();

        service.SuppressPrompt("Abigail", "spring", 1);

        Assert.True(service.IsPromptSuppressed("Abigail", "spring", 1));
        Assert.False(service.IsPromptSuppressed("Abigail", "spring", 2));
        Assert.False(service.IsPromptSuppressed("Abigail", "summer", 1));
        Assert.False(service.IsPromptSuppressed("Sebastian", "spring", 1));
    }

    [Fact]
    public void SuppressPrompt_ReplacesExistingEntry()
    {
        GiftHistoryService service = new();

        service.SuppressPrompt("Abigail", "spring", 1);
        service.SuppressPrompt("Abigail", "spring", 2);

        Assert.False(service.IsPromptSuppressed("Abigail", "spring", 1));
        Assert.True(service.IsPromptSuppressed("Abigail", "spring", 2));
    }

    [Fact]
    public void Import_NullSuppressionDictionaryStartsEmpty()
    {
        GiftHistoryService service = new();

        service.Import(new GiftHistoryData { SuppressedPrompts = null! });

        Assert.False(service.IsPromptSuppressed("Abigail", "spring", 1));
    }
}
