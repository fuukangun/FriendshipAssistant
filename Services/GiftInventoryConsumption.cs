using StardewValley;

namespace FriendshipAssistant.Services;

public static class GiftInventoryConsumption
{
    public readonly record struct ConsumeResult(int NewStack, bool RemoveSlot);

    public static ConsumeResult ConsumeOne(int stack)
    {
        if (stack <= 0)
            throw new ArgumentOutOfRangeException(nameof(stack));

        int newStack = stack - 1;
        return new ConsumeResult(newStack, newStack == 0);
    }

    public static bool TryConsume(IList<Item> items, Item selected)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selected);

        for (int index = 0; index < items.Count; index++)
        {
            if (!ReferenceEquals(items[index], selected) || selected.Stack <= 0)
                continue;

            ConsumeResult result = ConsumeOne(selected.Stack);
            if (result.RemoveSlot)
                items.RemoveAt(index);
            else
                selected.Stack = result.NewStack;
            return true;
        }

        return false;
    }

    public static bool Contains(IList<Item> items, Item selected)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selected);

        return selected.Stack > 0 && items.Any(item => ReferenceEquals(item, selected));
    }

    public static bool TryConsumeFromFarmer(Farmer farmer, Item selected)
    {
        ArgumentNullException.ThrowIfNull(farmer);
        ArgumentNullException.ThrowIfNull(selected);

        for (int index = 0; index < farmer.Items.Count; index++)
        {
            if (!ReferenceEquals(farmer.Items[index], selected) || selected.Stack <= 0)
                continue;

            ConsumeResult result = ConsumeOne(selected.Stack);
            if (result.RemoveSlot)
                farmer.Items[index] = null!;
            else
                selected.Stack = result.NewStack;
            return true;
        }

        return false;
    }

    public static bool CanConsumeFromFarmer(Farmer farmer, Item selected)
    {
        ArgumentNullException.ThrowIfNull(farmer);
        ArgumentNullException.ThrowIfNull(selected);

        return selected.Stack > 0 && farmer.Items.Any(item => ReferenceEquals(item, selected));
    }
}
