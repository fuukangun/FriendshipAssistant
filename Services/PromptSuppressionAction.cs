using FriendshipAssistant.Models;

namespace FriendshipAssistant.Services;

public static class PromptSuppressionAction
{
    public static Action Create(
        GiftHistoryService history,
        string npcName,
        Func<GameDate> getCurrentDate)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(getCurrentDate);
        return () => history.SuppressPrompt(npcName, getCurrentDate());
    }
}
