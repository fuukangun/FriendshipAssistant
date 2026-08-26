using FriendshipAssistant.Config;
using FriendshipAssistant.Helpers;
using FriendshipAssistant.Models;
using FriendshipAssistant.UI;
using StardewValley;

namespace FriendshipAssistant.Services;

public sealed class GiftPromptController
{
    private readonly ModConfig config;
    private readonly GiftDetector detector;
    private readonly GiftSelectionService selectionService;
    private readonly StardewGiftCandidateFactory candidateFactory;
    private readonly StorageItemService storageItemService;
    private readonly NpcGiftEligibility eligibility;
    private readonly GiftGiver giftGiver;
    private readonly GiftHistoryService giftHistory;
    private readonly Func<string, string> translate;
    private readonly Action<Item, string> notify;

    public GiftPromptController(
        ModConfig config,
        GiftDetector detector,
        GiftSelectionService selectionService,
        StardewGiftCandidateFactory candidateFactory,
        StorageItemService storageItemService,
        NpcGiftEligibility eligibility,
        GiftGiver giftGiver,
        GiftHistoryService giftHistory,
        Func<string, string> translate,
        Action<Item, string> notify)
    {
        this.config = config;
        this.detector = detector;
        this.selectionService = selectionService;
        this.candidateFactory = candidateFactory;
        this.storageItemService = storageItemService;
        this.eligibility = eligibility;
        this.giftGiver = giftGiver;
        this.giftHistory = giftHistory;
        this.translate = translate;
        this.notify = notify;
    }

    public GiftPromptResult TryPrompt(NPC npc, Farmer farmer)
    {
        if (!this.eligibility.CanOfferGiftPrompt(npc, farmer))
            return GiftPromptResult.NotEligible;

        GiftAnalysisResult result = this.detector.Analyze(this.candidateFactory.CreateFromInventory(farmer.Items, npc));
        if (!result.HasAnyGift && this.config.AutoGift)
            return GiftPromptResult.NoGifts;

        return this.config.AutoGift
            ? this.TryAutoGift(npc, farmer, result)
            : this.TryOpenManualPrompt(npc, farmer);
    }

    private GiftPromptResult TryAutoGift(NPC npc, Farmer farmer, GiftAnalysisResult result)
    {
        GiftCandidate? selected = this.selectionService.SelectAutoGift(result);
        if (selected is null)
            return GiftPromptResult.NoAutoGiftCandidate;

        StardewValley.Object? item = FindObjectById(farmer, selected.ItemId);
        if (item is null)
            return GiftPromptResult.ItemNotFound;

        Item notificationItem = item.getOne();
        this.giftGiver.GiveGift(npc, item, farmer, Game1.currentSeason, Game1.dayOfMonth);
        this.notify(notificationItem, $"{selected.DisplayName} -> {npc.displayName}");
        return GiftPromptResult.AutoGifted;
    }

    private GiftPromptResult TryOpenManualPrompt(NPC npc, Farmer farmer)
    {
        GameDate today = new(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
        if (this.giftHistory.IsPromptSuppressed(npc.Name, today))
            return GiftPromptResult.NotRemindedToday;

        List<GiftMenuItem> backpackItems = this.CreateBackpackItems(farmer, npc);
        List<GiftMenuItem> storageItems = this.config.ShowStorageItems
            ? this.storageItemService.CreateFromStorage(farmer, npc).ToList()
            : new List<GiftMenuItem>();
        if (backpackItems.Count == 0 && storageItems.Count == 0)
            return GiftPromptResult.NoGifts;

        GiftSuggestionMenuModels models = this.CreateMenuModels(backpackItems, storageItems, npc.Name);
        Game1.activeClickableMenu = new GiftSuggestionMenu(npc, this.CreateMenuOptions(npc, farmer, models));
        return GiftPromptResult.OpenedMenu;
    }

    private GiftSuggestionMenuModels CreateMenuModels(
        IReadOnlyList<GiftMenuItem> backpackItems,
        IReadOnlyList<GiftMenuItem> storageItems,
        string npcName)
    {
        string? lastGiftItemId = this.giftHistory.GetLastGift(npcName)?.ItemId;
        GiftMenuModel backpackModel = GiftMenuModel.FromItems(
            backpackItems,
            lastGiftItemId,
            this.GetCategoryLabel,
            this.translate("ui.lastGifted"));
        GiftMenuModel? storageModel = this.config.ShowStorageItems
            ? GiftMenuModel.FromItems(storageItems, lastGiftItemId, this.GetCategoryLabel, this.translate("ui.lastGifted"))
            : null;
        return new GiftSuggestionMenuModels(backpackModel, storageModel);
    }

    private GiftSuggestionMenuOptions CreateMenuOptions(
        NPC npc,
        Farmer farmer,
        GiftSuggestionMenuModels models)
    {
        Action suppressToday = PromptSuppressionAction.Create(
            this.giftHistory,
            npc.Name,
            () => new GameDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth));

        GiftSuggestionMenuOptions options = new(
            models,
            new GiftSuggestionMenuActions(
                selected => this.giftGiver.GiveGift(
                    npc,
                    selected,
                    farmer,
                    Game1.currentSeason,
                    Game1.dayOfMonth),
                suppressToday),
            new GiftSuggestionMenuText(
                new GiftSuggestionMenuLabels(
                    this.translate("ui.title"),
                    this.translate("ui.close"),
                    this.translate("ui.dismissToday")),
                new GiftSuggestionPageLabels(
                    this.translate("ui.backpack"),
                    this.translate("ui.storage"),
                    this.translate("ui.empty")),
                this.GetBannerText(npc)));
        return options;
    }

    private string GetCategoryLabel(GiftTaste taste)
    {
        return taste switch
        {
            GiftTaste.Loved => this.translate("category.loved"),
            GiftTaste.Liked => this.translate("category.liked"),
            GiftTaste.Neutral => this.translate("category.neutral"),
            GiftTaste.Disliked => this.translate("category.disliked"),
            GiftTaste.Hated => this.translate("category.hated"),
            _ => taste.ToString()
        };
    }

    private string? GetBannerText(NPC npc)
    {
        if (BirthdayHelper.IsBirthday(Game1.currentSeason, Game1.dayOfMonth, npc.Birthday_Season, npc.Birthday_Day))
            return this.translate("birthday.banner").Replace("{{npcName}}", npc.displayName, StringComparison.Ordinal);

        return null;
    }

    private List<GiftMenuItem> CreateBackpackItems(Farmer farmer, NPC npc)
    {
        List<GiftMenuItem> items = new();
        foreach (Item item in farmer.Items.OfType<Item>())
        {
            GiftCandidate? candidate = this.candidateFactory.Create(item, npc);
            if (candidate is null)
                continue;

            items.Add(new GiftMenuItem(
                candidate,
                new GiftItemSource(
                    GiftItemSourceKind.Backpack,
                    item,
                    new GiftSourceAccess(
                        () => true,
                        () => GiftInventoryConsumption.CanConsumeFromFarmer(farmer, item),
                        () => GiftInventoryConsumption.TryConsumeFromFarmer(farmer, item)))));
        }

        return items;
    }

    private static StardewValley.Object? FindObjectById(Farmer farmer, string itemId)
    {
        return farmer.Items.OfType<StardewValley.Object>().FirstOrDefault(item => item.ItemId == itemId && item.Stack > 0);
    }
}
