using FriendshipAssistant.Models;
using StardewValley;

namespace FriendshipAssistant.Services;

public sealed class GiftGiver
{
    private readonly GiftHistoryService historyService;

    public GiftGiver(GiftHistoryService historyService)
    {
        this.historyService = historyService;
    }

    public bool GiveGift(NPC npc, StardewValley.Object item, Farmer farmer, string season, int day)
    {
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(farmer);

        if (item.Stack <= 0)
            return false;

        string itemId = item.ItemId;
        string displayName = item.DisplayName;
        int quality = item.Quality;
        int salePrice = item.salePrice();

        npc.receiveGift(item, farmer);

        this.historyService.RecordGift(
            npc.Name,
            new GiftCandidate(itemId, displayName, GiftTaste.Neutral, quality, Math.Max(item.Stack, 1), salePrice),
            season,
            day);

        return true;
    }
}
