using FriendshipAssistant.Models;
using StardewValley;
using StardewValley.Objects;

namespace FriendshipAssistant.Services;

public sealed class StorageItemService
{
    private readonly StardewGiftCandidateFactory candidateFactory;

    public StorageItemService(StardewGiftCandidateFactory candidateFactory)
    {
        this.candidateFactory = candidateFactory;
    }

    public IReadOnlyList<GiftMenuItem> CreateFromStorage(Farmer farmer, NPC npc)
    {
        ArgumentNullException.ThrowIfNull(farmer);
        ArgumentNullException.ThrowIfNull(npc);

        List<GiftMenuItem> selections = new();
        HashSet<object> scannedContainers = new();
        bool hasJunimoChest = false;

        Utility.ForEachLocation(
            location =>
            {
                foreach (Chest chest in location.Objects.Values.OfType<Chest>())
                {
                    if (string.Equals(chest.GlobalInventoryId, FarmerTeam.GlobalInventoryId_JunimoChest, StringComparison.Ordinal))
                        hasJunimoChest = true;
                    else
                        this.AddChest(selections, scannedContainers, chest, npc);
                }

                Chest? fridge = location.GetFridge(onlyUnlocked: false);
                if (fridge is not null)
                    this.AddChest(selections, scannedContainers, fridge, npc);

                return true;
            },
            includeInteriors: true,
            includeGenerated: false);

        if (hasJunimoChest)
        {
            IList<Item> junimoItems = farmer.team.GetOrCreateGlobalInventory(FarmerTeam.GlobalInventoryId_JunimoChest);
            this.AddItems(selections, scannedContainers, junimoItems, npc);
        }
        return selections;
    }

    private void AddChest(
        ICollection<GiftMenuItem> selections,
        ISet<object> scannedContainers,
        Chest chest,
        NPC npc)
    {
        if (string.Equals(chest.GlobalInventoryId, FarmerTeam.GlobalInventoryId_JunimoChest, StringComparison.Ordinal))
            return;

        this.AddItems(selections, scannedContainers, chest.Items, npc);
    }

    private void AddItems(
        ICollection<GiftMenuItem> selections,
        ISet<object> scannedContainers,
        IList<Item> items,
        NPC npc)
    {
        if (!scannedContainers.Add(items))
            return;

        foreach (Item item in items.Where(item => item is not null && item.Stack > 0))
        {
            GiftCandidate? candidate = this.candidateFactory.Create(item, npc);
            if (candidate is null)
                continue;

            selections.Add(new GiftMenuItem(
                candidate,
                new GiftItemSource(
                    GiftItemSourceKind.Storage,
                    item,
                    () => GiftInventoryConsumption.Contains(items, item),
                    () => GiftInventoryConsumption.TryConsume(items, item))));
        }
    }
}
