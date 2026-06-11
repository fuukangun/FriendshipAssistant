using FriendshipAssistant.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed class GiftSuggestionMenu : IClickableMenu
{
    private const int MenuWidth = 720;
    private const int MenuHeight = 520;
    private const int SlotSize = 64;
    private const int SlotSpacing = 14;
    private const int Columns = 7;
    private const int VisibleGridRows = 3;

    private readonly NPC npc;
    private readonly GiftMenuModel model;
    private readonly GiftGridLayout gridLayout = new(Columns, SlotSize, SlotSpacing);
    private readonly IReadOnlyDictionary<string, Item> inventoryItemsById;
    private readonly Action<GiftCandidate> onGiftSelected;
    private readonly string title;
    private readonly string closeText;
    private readonly string? bannerText;
    private int scrollOffset;

    public GiftSuggestionMenu(
        NPC npc,
        GiftMenuModel model,
        IEnumerable<Item?> inventoryItems,
        Action<GiftCandidate> onGiftSelected,
        string title,
        string closeText,
        string? bannerText = null)
        : base(
            Game1.uiViewport.Width / 2 - MenuWidth / 2,
            Game1.uiViewport.Height / 2 - MenuHeight / 2,
            MenuWidth,
            MenuHeight,
            showUpperRightCloseButton: true)
    {
        this.npc = npc;
        this.model = model;
        this.inventoryItemsById = inventoryItems
            .OfType<Item>()
            .Where(item => item.Stack > 0)
            .GroupBy(item => item.ItemId)
            .ToDictionary(group => group.Key, group => group.First());
        this.onGiftSelected = onGiftSelected;
        this.title = title;
        this.closeText = closeText;
        this.bannerText = bannerText;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        GiftCandidate? clicked = this.gridLayout.HitTest(this.GetVisibleSlots(), x, y);
        if (clicked is not null)
        {
            this.onGiftSelected(clicked);
            this.exitThisMenu();
            return;
        }

        Rectangle closeBounds = new(this.xPositionOnScreen + this.width / 2 - 80, this.yPositionOnScreen + this.height - 76, 160, 52);
        if (closeBounds.Contains(x, y))
            this.exitThisMenu();
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);

        if (direction < 0)
            this.scrollOffset += Columns;
        else if (direction > 0)
            this.scrollOffset -= Columns;

        int itemCount = this.model.Rows.Count(row => row.Candidate is not null);
        int visibleCount = Columns * VisibleGridRows;
        this.scrollOffset = Math.Clamp(this.scrollOffset, 0, Math.Max(0, itemCount - visibleCount));
    }

    public override void draw(SpriteBatch b)
    {
        Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

        Utility.drawTextWithShadow(
            b,
            $"{this.title}: {this.npc.displayName}",
            Game1.dialogueFont,
            new Vector2(this.xPositionOnScreen + 48, this.yPositionOnScreen + 48),
            Game1.textColor);

        if (!string.IsNullOrWhiteSpace(this.bannerText))
        {
            Rectangle banner = new(this.xPositionOnScreen + 48, this.yPositionOnScreen + 88, this.width - 96, 40);
            b.Draw(Game1.staminaRect, banner, Color.Goldenrod * 0.25f);
            Utility.drawTextWithShadow(
                b,
                this.bannerText,
                Game1.smallFont,
                new Vector2(banner.X + 16, banner.Y + 9),
                Color.DarkGoldenrod);
        }

        this.DrawCategorySummary(b);

        foreach (GiftMenuSlot slot in this.GetVisibleSlots())
        {
            this.DrawSlot(b, slot);
        }

        Rectangle closeBounds = new(this.xPositionOnScreen + this.width / 2 - 80, this.yPositionOnScreen + this.height - 76, 160, 52);
        b.Draw(Game1.staminaRect, closeBounds, Color.SaddleBrown * 0.35f);
        Utility.drawTextWithShadow(
            b,
            this.closeText,
            Game1.smallFont,
            new Vector2(closeBounds.X + 46, closeBounds.Y + 14),
            Game1.textColor);

        this.drawMouse(b);
        this.DrawHoverTooltip(b);
    }

    private IReadOnlyList<GiftMenuSlot> GetVisibleSlots()
    {
        IReadOnlyList<GiftMenuRow> itemRows = this.model.Rows
            .Where(row => row.Candidate is not null)
            .Skip(this.scrollOffset)
            .Take(Columns * VisibleGridRows)
            .ToList();

        return this.gridLayout.BuildSlots(
            itemRows,
            originX: this.xPositionOnScreen + 64,
            originY: this.yPositionOnScreen + 160);
    }

    private void DrawCategorySummary(SpriteBatch b)
    {
        IReadOnlyList<string> labels = this.model.Rows
            .Where(row => row.IsHeader)
            .Select(row => row.Text)
            .ToList();
        if (labels.Count == 0)
            return;

        Utility.drawTextWithShadow(
            b,
            string.Join(" / ", labels),
            Game1.smallFont,
            new Vector2(this.xPositionOnScreen + 64, this.yPositionOnScreen + 126),
            Game1.textColor);
    }

    private void DrawSlot(SpriteBatch b, GiftMenuSlot slot)
    {
        Rectangle bounds = new(slot.X, slot.Y, slot.Size, slot.Size);
        Color background = bounds.Contains(Game1.getMouseX(), Game1.getMouseY())
            ? Color.Wheat * 0.65f
            : Color.Wheat * 0.35f;

        b.Draw(Game1.staminaRect, bounds, background);
        IClickableMenu.drawTextureBox(
            b,
            Game1.menuTexture,
            new Rectangle(0, 256, 60, 60),
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            Color.White);

        if (this.inventoryItemsById.TryGetValue(slot.Candidate.ItemId, out Item? item))
            item.drawInMenu(b, new Vector2(bounds.X + 8, bounds.Y + 8), 1f);

        if (slot.IsLastGift)
            Utility.drawTextWithShadow(b, "*", Game1.smallFont, new Vector2(bounds.Right - 18, bounds.Y + 2), Color.Gold);
    }

    private void DrawHoverTooltip(SpriteBatch b)
    {
        GiftMenuSlot? hovered = this.GetVisibleSlots()
            .FirstOrDefault(slot => new Rectangle(slot.X, slot.Y, slot.Size, slot.Size).Contains(Game1.getMouseX(), Game1.getMouseY()));
        if (hovered is null)
            return;

        string text = hovered.IsLastGift
            ? $"{hovered.Candidate.DisplayName}\n{hovered.Candidate.Taste}\n{this.model.Rows.First(row => row.Candidate == hovered.Candidate).Text}"
            : $"{hovered.Candidate.DisplayName}\n{hovered.Candidate.Taste}";
        IClickableMenu.drawHoverText(b, text, Game1.smallFont);
    }
}
