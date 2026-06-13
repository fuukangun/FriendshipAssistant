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
}
