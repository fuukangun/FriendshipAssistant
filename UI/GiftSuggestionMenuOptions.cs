using FriendshipAssistant.Models;

namespace FriendshipAssistant.UI;

public sealed record GiftSuggestionMenuModels(GiftMenuModel Backpack, GiftMenuModel? Storage);

public sealed record GiftSuggestionMenuActions(
    Func<GiftMenuItem, bool> SelectGift,
    Action DismissToday);

public sealed record GiftSuggestionMenuLabels(string Title, string Close, string DismissToday);

public sealed record GiftSuggestionPageLabels(string Backpack, string Storage, string Empty);

public sealed record GiftSuggestionMenuText(
    GiftSuggestionMenuLabels Menu,
    GiftSuggestionPageLabels Pages,
    string? Banner);

public sealed record GiftSuggestionMenuOptions(
    GiftSuggestionMenuModels Models,
    GiftSuggestionMenuActions Actions,
    GiftSuggestionMenuText Text);
