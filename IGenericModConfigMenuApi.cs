namespace FriendshipAssistant;

public interface IGenericModConfigMenuApi
{
    void Register(object mod, Action reset, Action save, bool titleScreenOnly = false);

    void AddBoolOption(
        object mod,
        Func<bool> getValue,
        Action<bool> setValue,
        Func<string> name,
        Func<string>? tooltip = null,
        string? fieldId = null);
}
