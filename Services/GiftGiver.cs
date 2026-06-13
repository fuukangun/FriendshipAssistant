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
        ConsumeGiftFromInventory(farmer, item);

        this.historyService.RecordGift(
            npc.Name,
            new GiftCandidate(itemId, displayName, GiftTaste.Neutral, quality, 1, salePrice),
            season,
            day);

        return true;
    }

    private static void ConsumeGiftFromInventory(Farmer farmer, StardewValley.Object item)
    {
        GiftInventoryConsumption.ConsumeResult result = GiftInventoryConsumption.ConsumeOne(item.Stack);

        for (int index = 0; index < farmer.Items.Count; index++)
        {
            if (!ReferenceEquals(farmer.Items[index], item))
                continue;

            if (result.RemoveSlot)
                farmer.Items[index] = null!;
            else
                item.Stack = result.NewStack;
            return;
        }

        item.Stack = result.NewStack;
    }
}
