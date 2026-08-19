using StardewValley;

namespace FriendshipAssistant.Models;

public enum GiftItemSourceKind
{
    Backpack,
    Storage
}

public sealed record GiftItemSource(
    GiftItemSourceKind Kind,
    Item Item,
    Func<bool> CanConsume,
    Func<bool> TryConsume);

public sealed record GiftMenuItem(
    GiftCandidate Candidate,
    GiftItemSource Source);
