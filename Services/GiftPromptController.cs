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

        if (this.config.AutoGift)
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

        if (this.giftHistory.IsPromptSuppressed(npc.Name, Game1.currentSeason, Game1.dayOfMonth))
            return GiftPromptResult.NotRemindedToday;

        List<GiftMenuItem> backpackItems = this.CreateBackpackItems(farmer, npc);
        List<GiftMenuItem> storageItems = this.config.ShowStorageItems
            ? this.storageItemService.CreateFromStorage(farmer, npc).ToList()
            : new List<GiftMenuItem>();
        if (backpackItems.Count == 0 && storageItems.Count == 0)
            return GiftPromptResult.NoGifts;

        string? lastGiftItemId = this.giftHistory.GetLastGift(npc.Name)?.ItemId;
        GiftMenuModel backpackModel = GiftMenuModel.FromItems(
            backpackItems,
            lastGiftItemId,
            this.GetCategoryLabel,
            this.translate("ui.lastGifted"));
        GiftMenuModel? storageModel = this.config.ShowStorageItems
            ? GiftMenuModel.FromItems(storageItems, lastGiftItemId, this.GetCategoryLabel, this.translate("ui.lastGifted"))
            : null;

        Game1.activeClickableMenu = new GiftSuggestionMenu(
            npc,
            backpackModel,
            storageModel,
            selected =>
            {
                this.giftGiver.GiveGift(npc, selected, farmer, Game1.currentSeason, Game1.dayOfMonth);
            },
            () => this.giftHistory.SuppressPrompt(npc.Name, Game1.currentSeason, Game1.dayOfMonth),
            this.translate("ui.title"),
            this.translate("ui.close"),
            this.translate("ui.dismissToday"),
            this.translate("ui.backpack"),
            this.translate("ui.storage"),
            this.translate("ui.empty"),
            this.GetBannerText(npc));

        return GiftPromptResult.OpenedMenu;
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
                    () => GiftInventoryConsumption.CanConsumeFromFarmer(farmer, item),
                    () => GiftInventoryConsumption.TryConsumeFromFarmer(farmer, item))));
        }

        return items;
    }

    private static StardewValley.Object? FindObjectById(Farmer farmer, string itemId)
    {
        return farmer.Items.OfType<StardewValley.Object>().FirstOrDefault(item => item.ItemId == itemId && item.Stack > 0);
    }
}
