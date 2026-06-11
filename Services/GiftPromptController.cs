using FriendshipAssistant.Config;
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
    private readonly Func<string, string> translate;
    private readonly Action<string> notify;

    public GiftPromptController(
        ModConfig config,
        GiftDetector detector,
        GiftSelectionService selectionService,
        StardewGiftCandidateFactory candidateFactory,
        NpcGiftEligibility eligibility,
        GiftGiver giftGiver,
        Func<string, string> translate,
        Action<string> notify)
    {
        this.config = config;
        this.detector = detector;
        this.selectionService = selectionService;
        this.candidateFactory = candidateFactory;
        this.eligibility = eligibility;
        this.giftGiver = giftGiver;
        this.translate = translate;
        this.notify = notify;
    }

    public void TryPrompt(NPC npc, Farmer farmer)
    {
        if (!this.eligibility.CanOfferGiftPrompt(npc, farmer))
            return;

        GiftAnalysisResult result = this.detector.Analyze(this.candidateFactory.CreateFromInventory(farmer.Items, npc));
        if (!result.HasAnyGift)
            return;

        if (this.config.AutoGift)
        {
            GiftCandidate? selected = this.selectionService.SelectAutoGift(result);
            if (selected is null)
                return;

            StardewValley.Object? item = FindObjectById(farmer, selected.ItemId);
            if (item is null)
                return;

            this.giftGiver.GiveGift(npc, item, farmer, Game1.currentSeason, Game1.dayOfMonth);
            this.notify($"{selected.DisplayName} -> {npc.displayName}");
            return;
        }

        IReadOnlyList<GiftCandidate> candidates = result.All().ToList();
        Game1.activeClickableMenu = new GiftSuggestionMenu(
            npc,
            candidates,
            selected =>
            {
                StardewValley.Object? item = FindObjectById(farmer, selected.ItemId);
                if (item is not null)
                    this.giftGiver.GiveGift(npc, item, farmer, Game1.currentSeason, Game1.dayOfMonth);
            },
            this.translate("ui.title"),
            this.translate("ui.close"));
    }

    private static StardewValley.Object? FindObjectById(Farmer farmer, string itemId)
    {
        return farmer.Items.OfType<StardewValley.Object>().FirstOrDefault(item => item.ItemId == itemId && item.Stack > 0);
    }
}
