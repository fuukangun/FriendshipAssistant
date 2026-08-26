using Microsoft.Xna.Framework.Input;

namespace FriendshipAssistant.UI;

public enum GiftMenuGamepadCommand
{
    None,
    Confirm,
    Cancel,
    BackpackPage,
    StoragePage,
    PageUp,
    PageDown,
    DismissToday,
    GridLeft,
    GridRight,
    GridUp,
    GridDown,
}

public interface IGiftMenuGamepadActions
{
    void Confirm();

    void Cancel();

    void SelectBackpackPage();

    void SelectStoragePage();

    void PageUp();

    void PageDown();

    void DismissToday();
}

public static class GiftMenuGamepadDispatcher
{
    public static bool Dispatch(GiftMenuGamepadCommand command, IGiftMenuGamepadActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        switch (command)
        {
            case GiftMenuGamepadCommand.Confirm: actions.Confirm(); break;
            case GiftMenuGamepadCommand.Cancel: actions.Cancel(); break;
            case GiftMenuGamepadCommand.BackpackPage: actions.SelectBackpackPage(); break;
            case GiftMenuGamepadCommand.StoragePage: actions.SelectStoragePage(); break;
            case GiftMenuGamepadCommand.PageUp: actions.PageUp(); break;
            case GiftMenuGamepadCommand.PageDown: actions.PageDown(); break;
            case GiftMenuGamepadCommand.DismissToday: actions.DismissToday(); break;
            default: return false;
        }
        return true;
    }
}

public static class GiftMenuGamepadInput
{
    public static GiftMenuGamepadCommand GetRightStickCommand(
        float x,
        float y,
        float deadzone)
    {
        if (deadzone < 0 || deadzone > 1)
            throw new ArgumentOutOfRangeException(nameof(deadzone));
        if (Math.Max(Math.Abs(x), Math.Abs(y)) <= deadzone)
            return GiftMenuGamepadCommand.None;

        return Math.Abs(x) >= Math.Abs(y)
            ? x > 0 ? GiftMenuGamepadCommand.GridRight : GiftMenuGamepadCommand.GridLeft
            : y > 0 ? GiftMenuGamepadCommand.GridUp : GiftMenuGamepadCommand.GridDown;
    }

    public static bool IsDirectionalButton(Buttons button)
    {
        return button is Buttons.DPadLeft
            or Buttons.DPadRight
            or Buttons.DPadUp
            or Buttons.DPadDown
            or Buttons.LeftThumbstickLeft
            or Buttons.LeftThumbstickRight
            or Buttons.LeftThumbstickUp
            or Buttons.LeftThumbstickDown
            or Buttons.RightThumbstickLeft
            or Buttons.RightThumbstickRight
            or Buttons.RightThumbstickUp
            or Buttons.RightThumbstickDown;
    }

    public static int GetScrollDirection(GiftMenuGamepadCommand command)
    {
        return command switch
        {
            GiftMenuGamepadCommand.PageUp => -1,
            GiftMenuGamepadCommand.PageDown => 1,
            _ => 0
        };
    }

}

public sealed class GamepadRepeatGate
{
    private const double InitialDelayMilliseconds = 400;
    private const double RepeatIntervalMilliseconds = 100;

    private GiftMenuGamepadCommand activeCommand;
    private double nextRepeatAt;

    public void Start(GiftMenuGamepadCommand command, double elapsedMilliseconds)
    {
        this.activeCommand = command;
        this.nextRepeatAt = elapsedMilliseconds + InitialDelayMilliseconds;
    }

    public bool ShouldRepeat(GiftMenuGamepadCommand command, double elapsedMilliseconds)
    {
        if (command != this.activeCommand || elapsedMilliseconds < this.nextRepeatAt)
            return false;

        this.nextRepeatAt = elapsedMilliseconds + RepeatIntervalMilliseconds;
        return true;
    }
}
