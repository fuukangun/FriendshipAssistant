namespace FriendshipAssistant.Models;

public sealed record GiftGridDisplayRow(
    string? HeaderText,
    IReadOnlyList<GiftMenuSlot> Slots,
    int X,
    int Y,
    int Height)
{
    public bool IsHeader => this.HeaderText is not null;

    public static GiftGridDisplayRow Header(string text, int height)
    {
        return new GiftGridDisplayRow(text, Array.Empty<GiftMenuSlot>(), 0, 0, height);
    }

    public static GiftGridDisplayRow Items(IReadOnlyList<GiftMenuSlot> slots, int height)
    {
        return new GiftGridDisplayRow(null, slots, 0, 0, height);
    }
}
