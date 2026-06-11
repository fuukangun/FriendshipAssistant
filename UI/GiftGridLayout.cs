using FriendshipAssistant.Models;

namespace FriendshipAssistant.UI;

public sealed class GiftGridLayout
{
    private readonly int columns;
    private readonly int slotSize;
    private readonly int slotSpacing;

    public GiftGridLayout(int columns, int slotSize, int slotSpacing)
    {
        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns));
        if (slotSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(slotSize));
        if (slotSpacing < 0)
            throw new ArgumentOutOfRangeException(nameof(slotSpacing));

        this.columns = columns;
        this.slotSize = slotSize;
        this.slotSpacing = slotSpacing;
    }

    public IReadOnlyList<GiftMenuSlot> BuildSlots(IEnumerable<GiftMenuRow> rows, int originX, int originY)
    {
        List<GiftMenuSlot> slots = new();

        int itemIndex = 0;
        foreach (GiftMenuRow row in rows)
        {
            if (row.Candidate is null)
                continue;

            int column = itemIndex % this.columns;
            int rowIndex = itemIndex / this.columns;
            int x = originX + column * (this.slotSize + this.slotSpacing);
            int y = originY + rowIndex * (this.slotSize + this.slotSpacing);
            slots.Add(new GiftMenuSlot(row.Candidate, x, y, this.slotSize, row.IsLastGift));
            itemIndex++;
        }

        return slots;
    }

    public GiftCandidate? HitTest(IEnumerable<GiftMenuSlot> slots, int x, int y)
    {
        foreach (GiftMenuSlot slot in slots)
        {
            bool insideX = x >= slot.X && x < slot.X + slot.Size;
            bool insideY = y >= slot.Y && y < slot.Y + slot.Size;
            if (insideX && insideY)
                return slot.Candidate;
        }

        return null;
    }
}
