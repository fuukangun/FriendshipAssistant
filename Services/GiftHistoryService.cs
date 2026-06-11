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
    }
}
