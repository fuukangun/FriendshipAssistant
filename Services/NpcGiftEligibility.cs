using StardewValley;

namespace FriendshipAssistant.Services;

public sealed class NpcGiftEligibility
{
    public bool CanOfferGiftPrompt(NPC npc, Farmer farmer)
    {
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(farmer);

        if (!npc.CanSocialize)
            return false;

        if (!farmer.friendshipData.TryGetValue(npc.Name, out Friendship? friendship))
            return false;

        return friendship.GiftsToday <= 0 && friendship.GiftsThisWeek < 2;
    }
}
