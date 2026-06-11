namespace FriendshipAssistant.Models;

public sealed record GiftCandidate(
    string ItemId,
    string DisplayName,
    GiftTaste Taste,
    int Quality,
    int Stack,
    int SalePrice);
