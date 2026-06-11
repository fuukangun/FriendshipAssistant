namespace FriendshipAssistant.Services;

public sealed class DialogueContextTracker
{
    private string? currentSpeakerName;

    public bool HasTriggeredForCurrentDialogue { get; private set; }

    public string? CurrentSpeakerName => this.currentSpeakerName;

    public void DialogueOpened(string speakerName)
    {
        if (string.IsNullOrWhiteSpace(speakerName))
            throw new ArgumentException("Speaker name is required.", nameof(speakerName));

        this.currentSpeakerName = speakerName;
        this.HasTriggeredForCurrentDialogue = false;
    }

    public string? DialogueClosed()
    {
        return this.HasTriggeredForCurrentDialogue ? null : this.currentSpeakerName;
    }

    public void MarkTriggered()
    {
        this.HasTriggeredForCurrentDialogue = true;
    }

    public void Clear()
    {
        this.currentSpeakerName = null;
        this.HasTriggeredForCurrentDialogue = false;
    }
}
