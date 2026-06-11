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

    [Fact]
    public void IsBirthday_ReturnsTrueWhenSeasonAndDayMatch()
    {
        Assert.True(BirthdayHelper.IsBirthday("spring", 13, "spring", 13));
    }

    [Fact]
    public void IsBirthday_ReturnsFalseWhenSeasonOrDayDiffers()
    {
        Assert.False(BirthdayHelper.IsBirthday("spring", 13, "summer", 13));
        Assert.False(BirthdayHelper.IsBirthday("spring", 13, "spring", 14));
    }
}
