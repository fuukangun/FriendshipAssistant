using FriendshipAssistant.Models;
using StardewValley;

namespace FriendshipAssistant.Services;

public sealed class StardewGiftCandidateFactory
{
    public GiftCandidate? Create(Item? item, NPC npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        if (item is null || item.Stack <= 0)
            return null;

        GiftTaste taste = MapTaste(npc.getGiftTasteForThisItem(item));
        return new GiftCandidate(
            item.ItemId,
            item.DisplayName,
            taste,
            item.Quality,
            item.Stack,
            item.salePrice());
    }

    public IEnumerable<GiftCandidate> CreateFromInventory(IEnumerable<Item?> inventory, NPC npc)
    {
        foreach (Item? item in inventory)
        {
            GiftCandidate? candidate = this.Create(item, npc);
            if (candidate is not null)
                yield return candidate;
        }
    }

    public static GiftTaste MapTaste(int rawTaste)
    {
        return rawTaste switch
        {
            0 => GiftTaste.Loved,
            2 => GiftTaste.Liked,
            8 => GiftTaste.Neutral,
            4 => GiftTaste.Disliked,
            6 => GiftTaste.Hated,
            _ => GiftTaste.Neutral
        };
    }
}
