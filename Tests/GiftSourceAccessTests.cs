using FriendshipAssistant.Models;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftSourceAccessTests
{
    [Fact]
    public void UnavailableSource_CannotConsumeAndDoesNotInvokeMutation()
    {
        bool consumed = false;
        GiftSourceAccess access = new(
            isAccessible: () => false,
            canConsume: () => true,
            tryConsume: () => consumed = true);

        Assert.False(access.CanConsume());
        Assert.False(access.TryConsume());
        Assert.False(consumed);
    }
}
