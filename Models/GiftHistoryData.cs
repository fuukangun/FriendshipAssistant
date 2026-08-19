namespace FriendshipAssistant.Models;

public sealed class GiftHistoryData
{
    public Dictionary<string, LastGiftEntry> LastGifts { get; set; } = new();

    public Dictionary<string, PromptSuppressionEntry> SuppressedPrompts { get; set; } = new();
}

public sealed class LastGiftEntry
{
    public string ItemId { get; set; } = string.Empty;

    public string ItemDisplayName { get; set; } = string.Empty;

    public string Season { get; set; } = string.Empty;

    public int Day { get; set; }
}

public sealed class PromptSuppressionEntry
{
    public string Season { get; set; } = string.Empty;

    public int Day { get; set; }
}
