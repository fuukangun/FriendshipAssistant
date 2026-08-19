using FriendshipAssistant.Models;

namespace FriendshipAssistant.UI;

public sealed class GiftMenuModel
{
    public GiftMenuModel(IEnumerable<GiftMenuRow> rows)
    {
        this.Rows = rows.ToList();
    }

    public IReadOnlyList<GiftMenuRow> Rows { get; }

    public static GiftMenuModel FromAnalysis(
        GiftAnalysisResult result,
        string? lastGiftItemId,
        Func<GiftTaste, string> categoryLabel,
        string lastGiftText)
    {
        List<GiftMenuRow> rows = new();

        AddGroup(rows, GiftTaste.Loved, result.Loved, lastGiftItemId, categoryLabel, lastGiftText);
        AddGroup(rows, GiftTaste.Liked, result.Liked, lastGiftItemId, categoryLabel, lastGiftText);
        AddGroup(rows, GiftTaste.Neutral, result.Neutral, lastGiftItemId, categoryLabel, lastGiftText);
        AddGroup(rows, GiftTaste.Disliked, result.Disliked, lastGiftItemId, categoryLabel, lastGiftText);
        AddGroup(rows, GiftTaste.Hated, result.Hated, lastGiftItemId, categoryLabel, lastGiftText);

        return new GiftMenuModel(rows);
    }

    public static GiftMenuModel FromItems(
        IEnumerable<GiftMenuItem> items,
        string? lastGiftItemId,
        Func<GiftTaste, string> categoryLabel,
        string lastGiftText)
    {
        ArgumentNullException.ThrowIfNull(items);

        List<GiftMenuRow> rows = new();
        GiftTaste[] tastes =
        {
            GiftTaste.Loved,
            GiftTaste.Liked,
            GiftTaste.Neutral,
            GiftTaste.Disliked,
            GiftTaste.Hated
        };

        foreach (GiftTaste taste in tastes)
        {
            IReadOnlyList<GiftMenuItem> group = items
                .Where(item => item.Candidate.Taste == taste)
                .OrderByDescending(item => item.Candidate.Quality)
                .ThenBy(item => item.Candidate.SalePrice)
                .ThenBy(item => item.Candidate.DisplayName, StringComparer.CurrentCulture)
                .ToList();

            if (group.Count == 0)
                continue;

            rows.Add(new GiftMenuRow(null, categoryLabel(taste), IsHeader: true, IsLastGift: false));
            foreach (GiftMenuItem item in group)
            {
                bool isLastGift = item.Candidate.ItemId == lastGiftItemId;
                string text = isLastGift
                    ? $"{item.Candidate.DisplayName} - {lastGiftText}"
                    : item.Candidate.DisplayName;
                rows.Add(new GiftMenuRow(item.Candidate, text, IsHeader: false, isLastGift)
                {
                    MenuItem = item
                });
            }
        }

        return new GiftMenuModel(rows);
    }

    public IReadOnlyList<GiftMenuRow> GetVisibleRows(int scrollOffset, int visibleCount)
    {
        if (visibleCount <= 0 || this.Rows.Count == 0)
            return Array.Empty<GiftMenuRow>();

        int maxOffset = Math.Max(0, this.Rows.Count - visibleCount);
        int clampedOffset = Math.Clamp(scrollOffset, 0, maxOffset);
        return this.Rows.Skip(clampedOffset).Take(visibleCount).ToList();
    }

    private static void AddGroup(
        ICollection<GiftMenuRow> rows,
        GiftTaste taste,
        IReadOnlyList<GiftCandidate> candidates,
        string? lastGiftItemId,
        Func<GiftTaste, string> categoryLabel,
        string lastGiftText)
    {
        if (candidates.Count == 0)
            return;

        rows.Add(new GiftMenuRow(null, categoryLabel(taste), IsHeader: true, IsLastGift: false));
        foreach (GiftCandidate candidate in candidates)
        {
            bool isLastGift = candidate.ItemId == lastGiftItemId;
            string text = isLastGift
                ? $"{candidate.DisplayName} - {lastGiftText}"
                : candidate.DisplayName;
            rows.Add(new GiftMenuRow(candidate, text, IsHeader: false, isLastGift));
        }
    }
}
