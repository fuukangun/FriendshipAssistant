using FriendshipAssistant.Models;

namespace FriendshipAssistant.Services;

public sealed class GiftHistoryService
{
    private GiftHistoryData data = new();

    public LastGiftEntry? GetLastGift(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            throw new ArgumentException("NPC name is required.", nameof(npcName));

        return this.data.LastGifts.TryGetValue(npcName, out LastGiftEntry? entry) ? entry : null;
    }

    public bool IsPromptSuppressed(string npcName, GameDate date)
    {
        ValidateDateArguments(npcName, date);

        return this.data.SuppressedPrompts.TryGetValue(npcName, out PromptSuppressionEntry? entry)
            && entry.Year == date.Year
            && string.Equals(entry.Season, date.Season, StringComparison.Ordinal)
            && entry.Day == date.Day;
    }

    public void SuppressPrompt(string npcName, GameDate date)
    {
        ValidateDateArguments(npcName, date);

        this.data.SuppressedPrompts[npcName] = new PromptSuppressionEntry
        {
            Year = date.Year,
            Season = date.Season,
            Day = date.Day
        };
    }

    public void RecordGift(string npcName, GiftCandidate candidate, string season, int day)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            throw new ArgumentException("NPC name is required.", nameof(npcName));
        if (string.IsNullOrWhiteSpace(season))
            throw new ArgumentException("Season is required.", nameof(season));

        ArgumentNullException.ThrowIfNull(candidate);

        this.data.LastGifts[npcName] = new LastGiftEntry
        {
            ItemId = candidate.ItemId,
            ItemDisplayName = candidate.DisplayName,
            Season = season,
            Day = day
        };
    }

    public GiftHistoryData Export()
    {
        return this.data;
    }

    public void Import(GiftHistoryData? loadedData)
    {
        this.data = loadedData ?? new GiftHistoryData();
        this.data.LastGifts ??= new Dictionary<string, LastGiftEntry>();
        this.data.SuppressedPrompts ??= new Dictionary<string, PromptSuppressionEntry>();
    }

    private static void ValidateDateArguments(string npcName, GameDate date)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            throw new ArgumentException("NPC name is required.", nameof(npcName));
        if (date.Year <= 0)
            throw new ArgumentOutOfRangeException(nameof(date));
        if (string.IsNullOrWhiteSpace(date.Season))
            throw new ArgumentException("Season is required.", nameof(date));
        if (date.Day <= 0)
            throw new ArgumentOutOfRangeException(nameof(date));
    }
}
