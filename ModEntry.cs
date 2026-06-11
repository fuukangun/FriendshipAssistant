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
            key => this.Helper.Translation.Get(key),
            message => Game1.addHUDMessage(new HUDMessage(message)));

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.Saving += this.OnSaving;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
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
        this.giftHistory.Import(null);
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu?.GetType().Name == "DialogueBox")
        {
            string? speakerName = TryGetDialogueSpeakerName(e.NewMenu);
            if (!string.IsNullOrWhiteSpace(speakerName))
                this.dialogueTracker.DialogueOpened(speakerName);
            return;
        }

        if (e.OldMenu?.GetType().Name == "DialogueBox" && e.NewMenu is null)
        {
            string? speakerName = this.dialogueTracker.DialogueClosed();
            if (speakerName is null)
                return;

            this.dialogueTracker.MarkTriggered();
            NPC? npc = Game1.getCharacterFromName(speakerName);
            if (npc is not null && Game1.player is not null)
                this.promptController?.TryPrompt(npc, Game1.player);
        }
    }

    private static string? TryGetDialogueSpeakerName(IClickableMenu menu)
    {
        if (menu is not DialogueBox dialogueBox)
            return null;

        object? speaker = typeof(DialogueBox)
            .GetField("speaker", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(dialogueBox);

        return speaker is NPC npc ? npc.Name : null;
    }
}
