namespace FriendshipAssistant.Models;

public sealed record GiftMenuRow(
    GiftCandidate? Candidate,
    string Text,
    bool IsHeader,
    bool IsLastGift);
