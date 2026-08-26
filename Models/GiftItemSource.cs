using StardewValley;

namespace FriendshipAssistant.Models;

public enum GiftItemSourceKind
{
    Backpack,
    Storage
}

public sealed class GiftSourceAccess
{
    private readonly Func<bool> isAccessible;
    private readonly Func<bool> canConsume;
    private readonly Func<bool> tryConsume;

    public GiftSourceAccess(Func<bool> isAccessible, Func<bool> canConsume, Func<bool> tryConsume)
    {
        this.isAccessible = isAccessible;
        this.canConsume = canConsume;
        this.tryConsume = tryConsume;
    }

    public bool CanConsume()
    {
        return this.isAccessible() && this.canConsume();
    }

    public bool TryConsume()
    {
        return this.isAccessible() && this.tryConsume();
    }
}

public sealed record GiftItemSource(GiftItemSourceKind Kind, Item Item, GiftSourceAccess Access)
{
    public bool CanConsume() => this.Access.CanConsume();

    public bool TryConsume() => this.Access.TryConsume();
}

public sealed record GiftMenuItem(
    GiftCandidate Candidate,
    GiftItemSource Source);
