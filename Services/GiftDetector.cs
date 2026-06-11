using FriendshipAssistant.Models;

namespace FriendshipAssistant.Services;

public sealed class GiftDetector
{
    private readonly GiftSelectionService selectionService;

    public GiftDetector(GiftSelectionService selectionService)
    {
        this.selectionService = selectionService;
    }

    public GiftAnalysisResult Analyze(IEnumerable<GiftCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        Dictionary<GiftTaste, List<GiftCandidate>> groups = new()
        {
            [GiftTaste.Loved] = new List<GiftCandidate>(),
            [GiftTaste.Liked] = new List<GiftCandidate>(),
            [GiftTaste.Neutral] = new List<GiftCandidate>(),
            [GiftTaste.Disliked] = new List<GiftCandidate>(),
            [GiftTaste.Hated] = new List<GiftCandidate>()
        };

        foreach (GiftCandidate candidate in candidates)
        {
            if (candidate.Stack <= 0)
                continue;

            if (groups.TryGetValue(candidate.Taste, out List<GiftCandidate>? group))
                group.Add(candidate);
        }

        return new GiftAnalysisResult
        {
            Loved = this.selectionService.SortGroup(groups[GiftTaste.Loved]),
            Liked = this.selectionService.SortGroup(groups[GiftTaste.Liked]),
            Neutral = this.selectionService.SortGroup(groups[GiftTaste.Neutral]),
            Disliked = this.selectionService.SortGroup(groups[GiftTaste.Disliked]),
            Hated = this.selectionService.SortGroup(groups[GiftTaste.Hated])
        };
    }
}
