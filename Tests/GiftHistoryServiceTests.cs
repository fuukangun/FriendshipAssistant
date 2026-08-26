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
    public void SuppressPrompt_MatchesNpcAndFullDate()
    {
        GiftHistoryService service = new();
        GameDate date = new(1, "spring", 1);

        service.SuppressPrompt("Abigail", date);

        Assert.True(service.IsPromptSuppressed("Abigail", date));
        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(2, "spring", 1)));
        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(1, "spring", 2)));
        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(1, "summer", 1)));
        Assert.False(service.IsPromptSuppressed("Sebastian", date));
    }

    [Fact]
    public void SuppressPrompt_ReplacesExistingEntry()
    {
        GiftHistoryService service = new();

        service.SuppressPrompt("Abigail", new GameDate(1, "spring", 1));
        service.SuppressPrompt("Abigail", new GameDate(1, "spring", 2));

        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(1, "spring", 1)));
        Assert.True(service.IsPromptSuppressed("Abigail", new GameDate(1, "spring", 2)));
    }

    [Fact]
    public void Import_NullSuppressionDictionaryStartsEmpty()
    {
        GiftHistoryService service = new();

        service.Import(new GiftHistoryData { SuppressedPrompts = null! });

        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(1, "spring", 1)));
    }

    [Fact]
    public void IsPromptSuppressed_LegacyEntryWithoutYearDoesNotMatch()
    {
        GiftHistoryService service = new();
        service.Import(new GiftHistoryData
        {
            SuppressedPrompts = new Dictionary<string, PromptSuppressionEntry>
            {
                ["Abigail"] = new PromptSuppressionEntry { Season = "spring", Day = 1 }
            }
        });

        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(1, "spring", 1)));
    }

    [Fact]
    public void SuppressPrompt_ExportsCurrentYear()
    {
        GiftHistoryService service = new();

        service.SuppressPrompt("Abigail", new GameDate(3, "spring", 1));

        Assert.Equal(3, service.Export().SuppressedPrompts["Abigail"].Year);
    }

    [Fact]
    public void SuppressionAction_ReadsDateWhenInvoked()
    {
        GiftHistoryService service = new();
        GameDate currentDate = new(1, "spring", 1);
        Action suppress = PromptSuppressionAction.Create(service, "Abigail", () => currentDate);
        currentDate = new GameDate(1, "spring", 2);

        suppress();

        Assert.False(service.IsPromptSuppressed("Abigail", new GameDate(1, "spring", 1)));
        Assert.True(service.IsPromptSuppressed("Abigail", currentDate));
    }
}
