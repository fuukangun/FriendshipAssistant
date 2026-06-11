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
    private readonly NpcGiftEligibility eligibility;
    private readonly GiftGiver giftGiver;
    private readonly GiftHistoryService giftHistory;
    private readonly Func<string, string> translate;
    private readonly Action<string> notify;

    public GiftPromptController(
        ModConfig config,
        GiftDetector detector,
        GiftSelectionService selectionService,
        StardewGiftCandidateFactory candidateFactory,
        NpcGiftEligibility eligibility,
        GiftGiver giftGiver,
        GiftHistoryService giftHistory,
        Func<string, string> translate,
        Action<string> notify)
    {
        this.config = config;
        this.detector = detector;
        this.selectionService = selectionService;
        this.candidateFactory = candidateFactory;
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
        if (!result.HasAnyGift)
            return GiftPromptResult.NoGifts;

        if (this.config.AutoGift)
        {
            GiftCandidate? selected = this.selectionService.SelectAutoGift(result);
            if (selected is null)
                return GiftPromptResult.NoAutoGiftCandidate;

            StardewValley.Object? item = FindObjectById(farmer, selected.ItemId);
            if (item is null)
                return GiftPromptResult.ItemNotFound;

            this.giftGiver.GiveGift(npc, item, farmer, Game1.currentSeason, Game1.dayOfMonth);
            this.notify($"{selected.DisplayName} -> {npc.displayName}");
            return GiftPromptResult.AutoGifted;
        }

        string? lastGiftItemId = this.giftHistory.GetLastGift(npc.Name)?.ItemId;
        GiftMenuModel model = GiftMenuModel.FromAnalysis(
            result,
            lastGiftItemId,
            this.GetCategoryLabel,
            this.translate("ui.lastGifted"));

        Game1.activeClickableMenu = new GiftSuggestionMenu(
            npc,
            model,
            farmer.Items,
            selected =>
            {
                StardewValley.Object? item = FindObjectById(farmer, selected.ItemId);
                if (item is not null)
                    this.giftGiver.GiveGift(npc, item, farmer, Game1.currentSeason, Game1.dayOfMonth);
            },
            this.translate("ui.title"),
            this.translate("ui.close"),
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

    private static StardewValley.Object? FindObjectById(Farmer farmer, string itemId)
    {
        return farmer.Items.OfType<StardewValley.Object>().FirstOrDefault(item => item.ItemId == itemId && item.Stack > 0);
    }
}
