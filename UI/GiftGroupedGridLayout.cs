using FriendshipAssistant.Models;

namespace FriendshipAssistant.UI;

public sealed class GiftGroupedGridLayout
{
    private readonly int columns;
    private readonly int slotSize;
    private readonly int slotSpacing;
    private readonly int headerHeight;
    private readonly int rowSpacing;

    public GiftGroupedGridLayout(int columns, int slotSize, int slotSpacing, int headerHeight, int rowSpacing)
    {
        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns));
        if (slotSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(slotSize));
        if (slotSpacing < 0)
            throw new ArgumentOutOfRangeException(nameof(slotSpacing));
        if (headerHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(headerHeight));
        if (rowSpacing < 0)
            throw new ArgumentOutOfRangeException(nameof(rowSpacing));

        this.columns = columns;
        this.slotSize = slotSize;
        this.slotSpacing = slotSpacing;
        this.headerHeight = headerHeight;
        this.rowSpacing = rowSpacing;
    }

    public int ContentRowHeight => this.slotSize + this.rowSpacing;

    public IReadOnlyList<GiftGridDisplayRow> BuildRows(IEnumerable<GiftMenuRow> rows)
    {
        List<GiftGridDisplayRow> displayRows = new();
        List<GiftMenuSlot> currentSlots = new();
        int currentSlotIndex = 0;

        foreach (GiftMenuRow row in rows)
        {
            if (row.IsHeader)
            {
                FlushSlots(displayRows, currentSlots);
                displayRows.Add(GiftGridDisplayRow.Header(row.Text, this.headerHeight));
                currentSlotIndex = 0;
                continue;
            }

            if (row.Candidate is null)
                continue;

            if (currentSlots.Count == this.columns)
            {
                FlushSlots(displayRows, currentSlots);
                currentSlotIndex = 0;
            }

            int column = currentSlotIndex % this.columns;
            currentSlots.Add(new GiftMenuSlot(
                row.Candidate,
                X: column * (this.slotSize + this.slotSpacing),
                Y: 0,
                Size: this.slotSize,
                row.IsLastGift));
            currentSlotIndex++;
        }

        FlushSlots(displayRows, currentSlots);
        return displayRows;
    }

    public IReadOnlyList<GiftGridDisplayRow> BuildVisibleRows(
        IEnumerable<GiftMenuRow> rows,
        int scrollRowOffset,
        int visibleRowCount)
    {
        if (visibleRowCount <= 0)
            return Array.Empty<GiftGridDisplayRow>();

        return this.BuildRows(rows)
            .Skip(Math.Max(0, scrollRowOffset))
            .Take(visibleRowCount)
            .ToList();
    }

    public IReadOnlyList<GiftGridDisplayRow> PositionRows(
        IEnumerable<GiftGridDisplayRow> rows,
        int originX,
        int originY)
    {
        List<GiftGridDisplayRow> positioned = new();
        int y = originY;

        foreach (GiftGridDisplayRow row in rows)
        {
            if (row.IsHeader)
            {
                positioned.Add(row with { X = originX, Y = y });
            }
            else
            {
                positioned.Add(row with
                {
                    X = originX,
                    Y = y,
                    Slots = row.Slots
                        .Select(slot => slot with { X = originX + slot.X, Y = y })
                        .ToList()
                });
            }

            y += row.Height;
        }

        return positioned;
    }

    public GiftCandidate? HitTest(IEnumerable<GiftGridDisplayRow> rows, int x, int y)
    {
        foreach (GiftGridDisplayRow row in rows)
        {
            foreach (GiftMenuSlot slot in row.Slots)
            {
                bool insideX = x >= slot.X && x < slot.X + slot.Size;
                bool insideY = y >= slot.Y && y < slot.Y + slot.Size;
                if (insideX && insideY)
                    return slot.Candidate;
            }
        }

        return null;
    }

    private void FlushSlots(ICollection<GiftGridDisplayRow> rows, ICollection<GiftMenuSlot> slots)
    {
        if (slots.Count == 0)
            return;

        rows.Add(GiftGridDisplayRow.Items(slots.ToList(), this.ContentRowHeight));
        slots.Clear();
    }
}
