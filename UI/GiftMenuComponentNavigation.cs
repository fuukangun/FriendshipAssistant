using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public static class GiftMenuComponentNavigation
{
    public static int GetComponentIdOrIgnore(
        GiftMenuNavigationPosition? position,
        Func<GiftMenuNavigationPosition, int> getComponentId)
    {
        ArgumentNullException.ThrowIfNull(getComponentId);
        return position is null
            ? ClickableComponent.ID_ignore
            : getComponentId(position.Value);
    }

    public static GiftMenuNavigationPosition? FindHorizontalNeighbor(
        IReadOnlyCollection<GiftMenuNavigationPosition> positions,
        GiftMenuNavigationPosition current,
        int direction)
    {
        Validate(positions, direction);
        GiftMenuNavigationPosition? target = positions
            .Where(position => position.RowIndex == current.RowIndex
                && position.Column == current.Column + direction)
            .Select(position => (GiftMenuNavigationPosition?)position)
            .FirstOrDefault();
        return target;
    }

    public static GiftMenuNavigationPosition? FindVerticalNeighbor(
        IReadOnlyCollection<GiftMenuNavigationPosition> positions,
        GiftMenuNavigationPosition current,
        int direction)
    {
        Validate(positions, direction);
        IEnumerable<GiftMenuNavigationPosition> candidates = direction < 0
            ? positions.Where(position => position.RowIndex < current.RowIndex)
            : positions.Where(position => position.RowIndex > current.RowIndex);
        int? targetRow = direction < 0
            ? candidates.Select(position => (int?)position.RowIndex).Max()
            : candidates.Select(position => (int?)position.RowIndex).Min();
        if (targetRow is null)
            return null;

        return candidates
            .Where(position => position.RowIndex == targetRow)
            .OrderBy(position => Math.Abs(position.Column - current.Column))
            .First();
    }

    private static void Validate(
        IReadOnlyCollection<GiftMenuNavigationPosition> positions,
        int direction)
    {
        ArgumentNullException.ThrowIfNull(positions);
        if (direction is not (-1 or 1))
            throw new ArgumentOutOfRangeException(nameof(direction));
    }
}
