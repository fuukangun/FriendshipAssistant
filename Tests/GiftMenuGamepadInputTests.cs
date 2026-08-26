using FriendshipAssistant.UI;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GiftMenuGamepadInputTests
{
    [Theory]
    [InlineData(GiftMenuGamepadCommand.Confirm)]
    [InlineData(GiftMenuGamepadCommand.Cancel)]
    [InlineData(GiftMenuGamepadCommand.BackpackPage)]
    [InlineData(GiftMenuGamepadCommand.StoragePage)]
    [InlineData(GiftMenuGamepadCommand.PageUp)]
    [InlineData(GiftMenuGamepadCommand.PageDown)]
    [InlineData(GiftMenuGamepadCommand.DismissToday)]
    public void Dispatch_InvokesMatchingMenuAction(GiftMenuGamepadCommand command)
    {
        RecordingActions actions = new();

        bool handled = GiftMenuGamepadDispatcher.Dispatch(command, actions);

        Assert.True(handled);
        Assert.Equal(command, actions.InvokedCommand);
    }

    [Theory]
    [InlineData(GiftMenuGamepadCommand.PageUp, -1)]
    [InlineData(GiftMenuGamepadCommand.PageDown, 1)]
    [InlineData(GiftMenuGamepadCommand.Confirm, 0)]
    public void GetScrollDirection_MapsPageCommands(GiftMenuGamepadCommand command, int expected)
    {
        Assert.Equal(expected, GiftMenuGamepadInput.GetScrollDirection(command));
    }

    [Theory]
    [InlineData(0.8f, 0f, GiftMenuGamepadCommand.GridRight)]
    [InlineData(-0.8f, 0f, GiftMenuGamepadCommand.GridLeft)]
    [InlineData(0f, 0.8f, GiftMenuGamepadCommand.GridUp)]
    [InlineData(0f, -0.8f, GiftMenuGamepadCommand.GridDown)]
    [InlineData(0.8f, 0.7f, GiftMenuGamepadCommand.GridRight)]
    [InlineData(0.1f, 0.1f, GiftMenuGamepadCommand.None)]
    public void GetRightStickCommand_SelectsDominantAxis(float x, float y, GiftMenuGamepadCommand expected)
    {
        Assert.Equal(expected, GiftMenuGamepadInput.GetRightStickCommand(x, y, 0.45f));
    }

    [Fact]
    public void RepeatGate_WaitsThenRepeatsAtFixedCadence()
    {
        GamepadRepeatGate gate = new();
        gate.Start(GiftMenuGamepadCommand.PageDown, elapsedMilliseconds: 1000);

        Assert.False(gate.ShouldRepeat(GiftMenuGamepadCommand.PageDown, elapsedMilliseconds: 1399));
        Assert.True(gate.ShouldRepeat(GiftMenuGamepadCommand.PageDown, elapsedMilliseconds: 1400));
        Assert.False(gate.ShouldRepeat(GiftMenuGamepadCommand.PageDown, elapsedMilliseconds: 1499));
        Assert.True(gate.ShouldRepeat(GiftMenuGamepadCommand.PageDown, elapsedMilliseconds: 1500));
        Assert.False(gate.ShouldRepeat(GiftMenuGamepadCommand.PageUp, elapsedMilliseconds: 1600));
    }

    private sealed class RecordingActions : IGiftMenuGamepadActions
    {
        public GiftMenuGamepadCommand InvokedCommand { get; private set; }

        public void Confirm() => this.InvokedCommand = GiftMenuGamepadCommand.Confirm;

        public void Cancel() => this.InvokedCommand = GiftMenuGamepadCommand.Cancel;

        public void SelectBackpackPage() => this.InvokedCommand = GiftMenuGamepadCommand.BackpackPage;

        public void SelectStoragePage() => this.InvokedCommand = GiftMenuGamepadCommand.StoragePage;

        public void PageUp() => this.InvokedCommand = GiftMenuGamepadCommand.PageUp;

        public void PageDown() => this.InvokedCommand = GiftMenuGamepadCommand.PageDown;

        public void DismissToday() => this.InvokedCommand = GiftMenuGamepadCommand.DismissToday;
    }
}
