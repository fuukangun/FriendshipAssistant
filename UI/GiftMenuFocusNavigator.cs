using FriendshipAssistant.Models;

namespace FriendshipAssistant.UI;

public enum GiftMenuFocusTarget
{
    Gift,
    TopChrome,
    Dismiss
}

public readonly record struct GiftMenuFocusResult(
    GiftMenuFocusTarget Target,
    GiftMenuNavigationPosition Position)
{
    public int RowIndex => this.Position.RowIndex;

    public int Column => this.Position.Column;

    public int ScrollOffset => this.Position.ScrollOffset;
}

public readonly record struct GiftMenuNavigationPosition(int RowIndex, int Column, int ScrollOffset);

public readonly record struct GiftMenuNavigationRequest(
    IReadOnlyList<GiftGridDisplayRow> Rows,
    GiftMenuNavigationPosition Position,
    int VisibleRows);

internal readonly record struct GiftMenuRowRange(int Start, int EndExclusive);

internal readonly record struct GiftMenuRowSearch(int Start, int Direction, GiftMenuRowRange Range);

public sealed record GiftMenuFocusCandidate(
    object Identity,
    GiftMenuNavigationPosition Position,
    int ComponentId);

public static class GiftMenuFocusResolver
{
    public static GiftMenuFocusCandidate? ResolveFirst(
        IReadOnlyList<GiftMenuFocusCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        return candidates
            .OrderBy(candidate => candidate.Position.RowIndex)
            .ThenBy(candidate => candidate.Position.Column)
            .FirstOrDefault();
    }

    public static GiftMenuFocusCandidate? Resolve(
        IReadOnlyList<GiftMenuFocusCandidate> candidates,
        object? savedIdentity,
        GiftMenuNavigationPosition fallbackPosition)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        GiftMenuFocusCandidate? identityMatch = candidates.FirstOrDefault(
            candidate => ReferenceEquals(candidate.Identity, savedIdentity));
        if (identityMatch is not null)
            return identityMatch;

        return candidates
            .OrderBy(candidate => Math.Abs(candidate.Position.RowIndex - fallbackPosition.RowIndex)
                + Math.Abs(candidate.Position.Column - fallbackPosition.Column))
            .ThenBy(candidate => candidate.Position.RowIndex)
            .ThenBy(candidate => candidate.Position.Column)
            .FirstOrDefault();
    }
}

public static class GiftMenuFocusNavigator
{
    public static GiftMenuFocusResult MoveVertical(GiftMenuNavigationRequest request, int direction)
    {
        IReadOnlyList<GiftGridDisplayRow> rows = request.Rows;
        GiftMenuNavigationPosition position = request.Position;
        int rowIndex = position.RowIndex;
        int column = position.Column;
        int scrollOffset = position.ScrollOffset;
        int visibleRows = request.VisibleRows;
        Validate(request, direction);

        int targetRow = FindItemRow(rows, rowIndex + direction, direction);
        if (targetRow < 0)
        {
            return new GiftMenuFocusResult(GiftMenuFocusTarget.Gift, position);
        }

        int targetColumn = Math.Min(column, rows[targetRow].Slots.Count - 1);
        int targetOffset = KeepVisible(targetRow, request);
        return new GiftMenuFocusResult(
            GiftMenuFocusTarget.Gift,
            new GiftMenuNavigationPosition(targetRow, targetColumn, targetOffset));
    }

    public static GiftMenuFocusResult ScrollPage(GiftMenuNavigationRequest request, int direction)
    {
        IReadOnlyList<GiftGridDisplayRow> rows = request.Rows;
        GiftMenuNavigationPosition position = request.Position;
        int rowIndex = position.RowIndex;
        int column = position.Column;
        int scrollOffset = position.ScrollOffset;
        int visibleRows = request.VisibleRows;
        Validate(request, direction);

        int maxOffset = Math.Max(0, rows.Count - visibleRows);
        int targetOffset = Math.Clamp(scrollOffset + direction * visibleRows, 0, maxOffset);
        int relativeRow = Math.Clamp(rowIndex - scrollOffset, 0, visibleRows - 1);
        int desiredRow = Math.Clamp(targetOffset + relativeRow, 0, rows.Count - 1);
        GiftMenuRowRange targetRange = new(targetOffset, Math.Min(rows.Count, targetOffset + visibleRows));
        int targetRow = FindNearestItemRow(rows, new GiftMenuRowSearch(desiredRow, direction, targetRange));
        if (targetRow < 0)
            return new GiftMenuFocusResult(
                GiftMenuFocusTarget.Dismiss,
                new GiftMenuNavigationPosition(rowIndex, column, targetOffset));

        int targetColumn = Math.Min(column, rows[targetRow].Slots.Count - 1);
        return new GiftMenuFocusResult(
            GiftMenuFocusTarget.Gift,
            new GiftMenuNavigationPosition(targetRow, targetColumn, targetOffset));
    }

    public static GiftMenuNavigationPosition? GetPageScrollAnchor(GiftMenuNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Rows);
        if (request.VisibleRows <= 0)
            throw new ArgumentOutOfRangeException(nameof(request));

        int offset = Math.Clamp(request.Position.ScrollOffset, 0, Math.Max(0, request.Rows.Count - request.VisibleRows));
        int row = request.Position.RowIndex;
        if (row >= offset && row < Math.Min(request.Rows.Count, offset + request.VisibleRows)
            && request.Rows[row].Slots.Count > 0)
        {
            int column = Math.Min(request.Position.Column, request.Rows[row].Slots.Count - 1);
            return new GiftMenuNavigationPosition(row, Math.Max(0, column), offset);
        }

        int firstGiftRow = FindItemRow(request.Rows, offset, 1);
        return firstGiftRow >= 0 && firstGiftRow < offset + request.VisibleRows
            ? new GiftMenuNavigationPosition(firstGiftRow, 0, offset)
            : null;
    }

    public static GiftMenuFocusResult MoveWithinViewport(
        GiftMenuNavigationRequest request,
        int direction)
    {
        Validate(request, direction);
        GiftMenuNavigationPosition position = request.Position;
        int start = position.RowIndex + direction;
        int end = Math.Min(request.Rows.Count, position.ScrollOffset + request.VisibleRows);
        int targetRow = FindItemRowInRange(request.Rows, start, direction, request.Position.ScrollOffset, end);
        if (targetRow < 0)
            return new GiftMenuFocusResult(GiftMenuFocusTarget.Gift, position);

        int targetColumn = Math.Min(position.Column, request.Rows[targetRow].Slots.Count - 1);
        return new GiftMenuFocusResult(
            GiftMenuFocusTarget.Gift,
            new GiftMenuNavigationPosition(targetRow, targetColumn, position.ScrollOffset));
    }

    private static int FindItemRow(IReadOnlyList<GiftGridDisplayRow> rows, int start, int direction)
    {
        for (int index = start; index >= 0 && index < rows.Count; index += direction)
        {
            if (rows[index].Slots.Count > 0)
                return index;
        }
        return -1;
    }

    private static int FindItemRowInRange(
        IReadOnlyList<GiftGridDisplayRow> rows,
        int start,
        int direction,
        int rangeStart,
        int rangeEnd)
    {
        for (int index = start; index >= rangeStart && index < rangeEnd; index += direction)
        {
            if (rows[index].Slots.Count > 0)
                return index;
        }
        return -1;
    }

    private static int FindNearestItemRow(IReadOnlyList<GiftGridDisplayRow> rows, GiftMenuRowSearch search)
    {
        for (int index = search.Start;
             index >= search.Range.Start && index < search.Range.EndExclusive;
             index += search.Direction)
        {
            if (rows[index].Slots.Count > 0)
                return index;
        }

        for (int index = search.Start - search.Direction;
             index >= search.Range.Start && index < search.Range.EndExclusive;
             index -= search.Direction)
        {
            if (rows[index].Slots.Count > 0)
                return index;
        }
        return -1;
    }

    private static int KeepVisible(int rowIndex, GiftMenuNavigationRequest request)
    {
        int offset = request.Position.ScrollOffset;
        int visibleRows = request.VisibleRows;
        int adjusted = offset;
        if (rowIndex < offset)
            adjusted = rowIndex;
        else if (rowIndex >= offset + visibleRows)
            adjusted = rowIndex - visibleRows + 1;

        return Math.Clamp(adjusted, 0, Math.Max(0, request.Rows.Count - visibleRows));
    }

    private static void Validate(GiftMenuNavigationRequest request, int direction)
    {
        ArgumentNullException.ThrowIfNull(request.Rows);
        if (request.Rows.Count == 0)
            throw new ArgumentException("At least one display row is required.", nameof(request));
        int rowIndex = request.Position.RowIndex;
        if (rowIndex < 0 || rowIndex >= request.Rows.Count || request.Rows[rowIndex].Slots.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(request));
        if (request.VisibleRows <= 0)
            throw new ArgumentOutOfRangeException(nameof(request));
        if (direction is not (-1 or 1))
            throw new ArgumentOutOfRangeException(nameof(direction));
    }
}
