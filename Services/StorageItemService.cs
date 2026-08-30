using FriendshipAssistant.Models;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace FriendshipAssistant.Services;

public sealed class StorageItemService
{
    private sealed record StorageScanContext(
        ICollection<GiftMenuItem> Selections,
        ISet<object> ScannedContainers,
        NPC Npc);

    private sealed record StorageContainer(IInventory Items, Func<bool> IsAccessible);

    private sealed record StorageAnchor(GameLocation Location, Chest Chest)
    {
        public bool IsAvailable()
        {
            return this.Location.Objects.Values.Any(value => ReferenceEquals(value, this.Chest))
                || ReferenceEquals(this.Location.GetFridge(onlyUnlocked: false), this.Chest);
        }
    }

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
        StorageScanContext context = new(selections, scannedContainers, npc);
        List<StorageAnchor> junimoAnchors = new();

        Utility.ForEachLocation(
            location =>
            {
                foreach (Chest chest in location.Objects.Values.OfType<Chest>())
                {
                    StorageAnchor anchor = new(location, chest);
                    if (string.Equals(chest.GlobalInventoryId, FarmerTeam.GlobalInventoryId_JunimoChest, StringComparison.Ordinal))
                        junimoAnchors.Add(anchor);
                    else
                        this.AddChest(context, chest, anchor);
                }

                Chest? fridge = location.GetFridge(onlyUnlocked: false);
                if (fridge is not null)
                    this.AddChest(context, fridge, new StorageAnchor(location, fridge));

                return true;
            },
            includeInteriors: true,
            includeGenerated: false);

        if (junimoAnchors.Count > 0)
        {
            Func<bool> isJunimoAccessible = () => junimoAnchors.Any(anchor => anchor.IsAvailable());
            foreach (StorageAnchor anchor in junimoAnchors)
            {
                IInventory items = anchor.Chest.GetItemsForPlayer();
                this.AddItems(context, new StorageContainer(items, isJunimoAccessible));
            }
        }
        return selections;
    }

    private void AddChest(
        StorageScanContext context,
        Chest chest,
        StorageAnchor anchor)
    {
        this.AddItems(context, new StorageContainer(chest.GetItemsForPlayer(), anchor.IsAvailable));
    }

    private void AddItems(
        StorageScanContext context,
        StorageContainer container)
    {
        if (!context.ScannedContainers.Add(container.Items))
            return;

        foreach (Item item in container.Items.Where(item => item is not null && item.Stack > 0))
        {
            GiftCandidate? candidate = this.candidateFactory.Create(item, context.Npc);
            if (candidate is null)
                continue;

            context.Selections.Add(new GiftMenuItem(
                candidate,
                new GiftItemSource(
                    GiftItemSourceKind.Storage,
                    item,
                    new GiftSourceAccess(
                        container.IsAccessible,
                        () => GiftInventoryConsumption.Contains(container.Items, item),
                        () => GiftInventoryConsumption.TryConsume(container.Items, item)))));
        }
    }
}
