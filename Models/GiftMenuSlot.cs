namespace FriendshipAssistant.Models;

public sealed record GiftMenuSlot(
    GiftCandidate Candidate,
    int X,
    int Y,
    int Size,
    bool IsLastGift);
