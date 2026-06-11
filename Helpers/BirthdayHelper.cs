namespace FriendshipAssistant.Helpers;

public static class BirthdayHelper
{
    public static bool IsFestivalOrBirthday() => false;

    public static bool IsBirthday(string currentSeason, int currentDay, string birthdaySeason, int birthdayDay)
    {
        return string.Equals(currentSeason, birthdaySeason, StringComparison.OrdinalIgnoreCase)
            && currentDay == birthdayDay;
    }
}
