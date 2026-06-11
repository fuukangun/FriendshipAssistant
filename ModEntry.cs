using FriendshipAssistant.Config;
using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FriendshipAssistant;

public sealed class ModEntry : Mod
{
    private const string GiftHistoryKey = "gift-history";

    private readonly DialogueContextTracker dialogueTracker = new();
    private readonly GiftHistoryService giftHistory = new();
    private GiftPromptController? promptController;
    private string? pendingSpeakerName;
    private bool wasDialogueBoxOpen;
    private ModConfig config = new();

    public override void Entry(IModHelper helper)
    {
        this.config = helper.ReadConfig<ModConfig>();
        GiftSelectionService selectionService = new();
        this.promptController = new GiftPromptController(
            this.config,
            new GiftDetector(selectionService),
            selectionService,
            new StardewGiftCandidateFactory(),
            new NpcGiftEligibility(),
            new GiftGiver(this.giftHistory),
            this.giftHistory,
            key => this.Helper.Translation.Get(key),
            message => Game1.addHUDMessage(new HUDMessage(message)));

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.Saving += this.OnSaving;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;

        Monitor.Log("FriendshipAssistant loaded.", LogLevel.Info);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IGenericModConfigMenuApi? gmcm =
            this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(
            this.ModManifest,
            reset: () => this.config = new ModConfig(),
            save: () => this.Helper.WriteConfig(this.config));

        gmcm.AddBoolOption(
            this.ModManifest,
            getValue: () => this.config.AutoGift,
            setValue: value => this.config.AutoGift = value,
            name: () => this.Helper.Translation.Get("gmcm.autoGift.name"),
            tooltip: () => this.Helper.Translation.Get("gmcm.autoGift.tooltip"));
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        GiftHistoryData? data = this.Helper.Data.ReadSaveData<GiftHistoryData>(GiftHistoryKey);
        this.giftHistory.Import(data);
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        this.Helper.Data.WriteSaveData(GiftHistoryKey, this.giftHistory.Export());
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.dialogueTracker.Clear();
        this.pendingSpeakerName = null;
        this.giftHistory.Import(null);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (this.pendingSpeakerName is null || Game1.activeClickableMenu is not null || Game1.player is null)
        {
            this.TrackDialogueMenu();
            return;
        }

        string speakerName = this.pendingSpeakerName;
        this.pendingSpeakerName = null;
        this.TrackDialogueMenu();

        NPC? npc = Game1.getCharacterFromName(speakerName);
        if (npc is null)
        {
            this.Monitor.Log($"Gift prompt skipped: NPC '{speakerName}' was not found.", LogLevel.Info);
            return;
        }

        GiftPromptResult result = this.promptController?.TryPrompt(npc, Game1.player) ?? GiftPromptResult.NotEligible;
        this.Monitor.Log($"Gift prompt result for {npc.Name}: {result}.", LogLevel.Info);
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu?.GetType().Name == "DialogueBox")
        {
            string? speakerName = TryGetDialogueSpeakerName(e.NewMenu);
            if (!string.IsNullOrWhiteSpace(speakerName))
            {
                this.dialogueTracker.DialogueOpened(speakerName);
                this.Monitor.Log($"Dialogue opened with {speakerName}.", LogLevel.Info);
            }
            return;
        }

        if (e.OldMenu?.GetType().Name == "DialogueBox")
        {
            string? speakerName = this.dialogueTracker.DialogueClosed();
            if (speakerName is null)
                return;

            this.dialogueTracker.MarkTriggered();
            this.pendingSpeakerName = speakerName;
            this.Monitor.Log($"Dialogue closed with {speakerName}; prompt queued.", LogLevel.Info);
        }
    }

    private void TrackDialogueMenu()
    {
        bool isDialogueBoxOpen = Game1.activeClickableMenu?.GetType().Name == "DialogueBox";
        if (isDialogueBoxOpen)
        {
            IClickableMenu? activeMenu = Game1.activeClickableMenu;
            string? speakerName = activeMenu is null ? null : TryGetDialogueSpeakerName(activeMenu);
            if (!string.IsNullOrWhiteSpace(speakerName) && this.dialogueTracker.CurrentSpeakerName != speakerName)
            {
                this.dialogueTracker.DialogueOpened(speakerName);
                this.Monitor.Log($"Dialogue tracked with {speakerName}.", LogLevel.Info);
            }

            this.wasDialogueBoxOpen = true;
            return;
        }

        if (!this.wasDialogueBoxOpen)
            return;

        this.wasDialogueBoxOpen = false;
        string? closedSpeakerName = this.dialogueTracker.DialogueClosed();
        if (closedSpeakerName is null)
            return;

        this.dialogueTracker.MarkTriggered();
        this.pendingSpeakerName = closedSpeakerName;
        this.Monitor.Log($"Dialogue tracked closed with {closedSpeakerName}; prompt queued.", LogLevel.Info);
    }

    private static string? TryGetDialogueSpeakerName(IClickableMenu menu)
    {
        if (menu is not DialogueBox dialogueBox)
            return null;

        object? speaker = typeof(DialogueBox)
            .GetField("speaker", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(dialogueBox);

        if (speaker is NPC npc)
            return npc.Name;

        object? currentSpeaker = typeof(Game1)
            .GetField("currentSpeaker", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(null);

        return currentSpeaker is NPC fallbackNpc ? fallbackNpc.Name : null;
    }
}
