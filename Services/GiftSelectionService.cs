using FriendshipAssistant.Models;

namespace FriendshipAssistant.Services;

public sealed class GiftSelectionService
{
    public GiftCandidate? SelectAutoGift(GiftAnalysisResult result)
    {
        return result.Loved
            .Concat(result.Liked)
            .Concat(result.Neutral)
            .OrderByDescending(candidate => TasteRank(candidate.Taste))
            .ThenByDescending(candidate => candidate.Quality)
            .ThenBy(candidate => candidate.SalePrice)
            .ThenBy(candidate => candidate.DisplayName, StringComparer.CurrentCulture)
            .FirstOrDefault();
    }

    public IReadOnlyList<GiftCandidate> SortGroup(IEnumerable<GiftCandidate> candidates)
    {
        return candidates
            .OrderByDescending(candidate => candidate.Quality)
            .ThenBy(candidate => candidate.SalePrice)
            .ThenBy(candidate => candidate.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }

    private static int TasteRank(GiftTaste taste)
    {
        return taste switch
        {
            GiftTaste.Loved => 3,
            GiftTaste.Liked => 2,
            GiftTaste.Neutral => 1,
            _ => 0
        };
    }
}
