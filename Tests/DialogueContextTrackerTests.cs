using FriendshipAssistant.Services;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class DialogueContextTrackerTests
{
    [Fact]
    public void DialogueClosed_ReturnsSpeakerOnlyUntilTriggered()
    {
        DialogueContextTracker tracker = new();

        tracker.DialogueOpened("Abigail");
        string? speaker = tracker.DialogueClosed();
        tracker.MarkTriggered();

        Assert.Equal("Abigail", speaker);
        Assert.Null(tracker.DialogueClosed());
    }

    [Fact]
    public void Clear_RemovesSpeakerAndTriggerState()
    {
        DialogueContextTracker tracker = new();

        tracker.DialogueOpened("Abigail");
        tracker.MarkTriggered();
        tracker.Clear();

        Assert.Null(tracker.CurrentSpeakerName);
        Assert.False(tracker.HasTriggeredForCurrentDialogue);
    }
}
