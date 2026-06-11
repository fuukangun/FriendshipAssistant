using FriendshipAssistant.Helpers;
using Xunit;

namespace FriendshipAssistant.Tests;

public class BirthdayHelperTests
{
    [Fact]
    public void IsFestivalOrBirthday_ReturnsFalse_ByDefault()
    {
        Assert.False(BirthdayHelper.IsFestivalOrBirthday());
    }
}
