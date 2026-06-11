namespace FriendshipAssistant.Models;

public sealed class GiftAnalysisResult
{
    public IReadOnlyList<GiftCandidate> Loved { get; init; } = Array.Empty<GiftCandidate>();

    public IReadOnlyList<GiftCandidate> Liked { get; init; } = Array.Empty<GiftCandidate>();

    public IReadOnlyList<GiftCandidate> Neutral { get; init; } = Array.Empty<GiftCandidate>();

    public IReadOnlyList<GiftCandidate> Disliked { get; init; } = Array.Empty<GiftCandidate>();

    public IReadOnlyList<GiftCandidate> Hated { get; init; } = Array.Empty<GiftCandidate>();

    public bool HasAnyGift => Loved.Count + Liked.Count + Neutral.Count + Disliked.Count + Hated.Count > 0;

    public IEnumerable<GiftCandidate> All()
    {
        foreach (GiftCandidate candidate in Loved)
            yield return candidate;
        foreach (GiftCandidate candidate in Liked)
            yield return candidate;
        foreach (GiftCandidate candidate in Neutral)
            yield return candidate;
        foreach (GiftCandidate candidate in Disliked)
            yield return candidate;
        foreach (GiftCandidate candidate in Hated)
            yield return candidate;
    }
}
