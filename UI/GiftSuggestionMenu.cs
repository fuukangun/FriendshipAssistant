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
    private const int HeaderHeight = 30;
    private const int RowSpacing = 12;
    private const int VisibleDisplayRows = 5;
    private const int IconBaseSize = 64;
    private const int CloseButtonSize = 48;
    private const int CloseButtonInset = 28;
    private const int ScrollBarWidth = 24;
    private const int ScrollBarInsetRight = 48;
    private const int ScrollBarTop = 160;
    private const int ScrollBarHeight = 300;
    private const int ScrollBarFrameInset = 4;

    private readonly NPC npc;
    private readonly GiftMenuModel model;
    private readonly GiftGroupedGridLayout gridLayout = new(Columns, SlotSize, SlotSpacing, HeaderHeight, RowSpacing);
    private readonly IReadOnlyDictionary<string, Item> inventoryItemsById;
    private readonly Action<GiftCandidate> onGiftSelected;
    private readonly string title;
    private readonly string? bannerText;
    private int scrollRowOffset;
    private bool isDraggingScrollBar;
    private int scrollBarDragOffsetY;

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
        this.bannerText = bannerText;
        GiftMenuChrome.CloseButtonBounds closeBounds = GiftMenuChrome.GetCloseButtonBounds(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            CloseButtonSize,
            CloseButtonInset);
        this.upperRightCloseButton.bounds = new Rectangle(closeBounds.X, closeBounds.Y, closeBounds.Width, closeBounds.Height);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        GiftScrollBarLayout.ScrollBarState scrollBar = this.GetScrollBarState();
        if (scrollBar.IsVisible)
        {
            Rectangle thumbBounds = new(scrollBar.TrackX, scrollBar.ThumbY, scrollBar.TrackWidth, scrollBar.ThumbHeight);
            Rectangle trackBounds = new(scrollBar.TrackX, scrollBar.TrackY, scrollBar.TrackWidth, scrollBar.TrackHeight);
            if (thumbBounds.Contains(x, y))
            {
                this.isDraggingScrollBar = true;
                this.scrollBarDragOffsetY = y - scrollBar.ThumbY;
                return;
            }

            if (trackBounds.Contains(x, y))
            {
                this.SetScrollOffsetFromThumbY(y - scrollBar.ThumbHeight / 2);
                return;
            }
        }

        GiftCandidate? clicked = this.gridLayout.HitTest(this.GetVisibleRows(), x, y);
        if (clicked is not null)
        {
            this.onGiftSelected(clicked);
            this.exitThisMenu();
        }
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);

        if (direction < 0)
            this.scrollRowOffset++;
        else if (direction > 0)
            this.scrollRowOffset--;

        int rowCount = this.gridLayout.BuildRows(this.model.Rows).Count;
        this.scrollRowOffset = Math.Clamp(this.scrollRowOffset, 0, Math.Max(0, rowCount - VisibleDisplayRows));
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);

        if (!this.isDraggingScrollBar)
            return;

        this.SetScrollOffsetFromThumbY(y - this.scrollBarDragOffsetY);
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        this.isDraggingScrollBar = false;
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

        foreach (GiftGridDisplayRow row in this.GetVisibleRows())
        {
            if (row.IsHeader)
                this.DrawHeader(b, row);
            else
                foreach (GiftMenuSlot slot in row.Slots)
                    this.DrawSlot(b, slot);
        }

        this.DrawScrollBar(b);
        this.upperRightCloseButton.draw(b);

        this.drawMouse(b);
        this.DrawHoverTooltip(b);
    }

    private GiftScrollBarLayout.ScrollBarState GetScrollBarState()
    {
        int totalRows = this.gridLayout.BuildRows(this.model.Rows).Count;
        return GiftScrollBarLayout.Calculate(
            trackX: this.xPositionOnScreen + this.width - ScrollBarInsetRight - ScrollBarWidth,
            trackY: this.yPositionOnScreen + ScrollBarTop,
            trackWidth: ScrollBarWidth,
            trackHeight: ScrollBarHeight,
            totalRows,
            VisibleDisplayRows,
            this.scrollRowOffset,
            ScrollBarFrameInset);
    }

    private void SetScrollOffsetFromThumbY(int thumbY)
    {
        GiftScrollBarLayout.ScrollBarState scrollBar = this.GetScrollBarState();
        int totalRows = this.gridLayout.BuildRows(this.model.Rows).Count;
        this.scrollRowOffset = GiftScrollBarLayout.ScrollOffsetFromThumbY(
            thumbY,
            scrollBar.TrackY,
            scrollBar.TrackHeight,
            scrollBar.ThumbHeight,
            totalRows,
            VisibleDisplayRows,
            ScrollBarFrameInset);
    }

    private void DrawScrollBar(SpriteBatch b)
    {
        GiftScrollBarLayout.ScrollBarState scrollBar = this.GetScrollBarState();
        if (!scrollBar.IsVisible)
            return;

        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(403, 383, 6, 6),
            scrollBar.TrackX,
            scrollBar.TrackY,
            scrollBar.TrackWidth,
            scrollBar.TrackHeight,
            Color.White,
            4f,
            drawShadow: false);

        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(435, 463, 6, 10),
            scrollBar.TrackX,
            scrollBar.ThumbY,
            scrollBar.TrackWidth,
            scrollBar.ThumbHeight,
            Color.White,
            4f,
            drawShadow: false);
    }

    private IReadOnlyList<GiftGridDisplayRow> GetVisibleRows()
    {
        return this.gridLayout.PositionRows(
            this.gridLayout.BuildVisibleRows(this.model.Rows, this.scrollRowOffset, VisibleDisplayRows),
            originX: this.xPositionOnScreen + 64,
            originY: this.yPositionOnScreen + 160);
    }

    private void DrawHeader(SpriteBatch b, GiftGridDisplayRow row)
    {
        Utility.drawTextWithShadow(
            b,
            row.HeaderText ?? string.Empty,
            Game1.smallFont,
            new Vector2(row.X, row.Y + 3),
            Color.DarkGoldenrod);
    }

    private void DrawSlot(SpriteBatch b, GiftMenuSlot slot)
    {
        Rectangle bounds = new(slot.X, slot.Y, slot.Size, slot.Size);
        bool hovered = bounds.Contains(Game1.getMouseX(), Game1.getMouseY());
        Color background = hovered
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
        {
            float scale = hovered ? 1.12f : 1f;
            float offset = (IconBaseSize - IconBaseSize * scale) / 2f;
            item.drawInMenu(b, new Vector2(bounds.X + offset, bounds.Y + offset), scale);
        }

        if (slot.IsLastGift)
            Utility.drawTextWithShadow(b, "*", Game1.smallFont, new Vector2(bounds.Right - 18, bounds.Y + 2), Color.Gold);
    }

    private void DrawHoverTooltip(SpriteBatch b)
    {
        GiftMenuSlot? hovered = this.GetVisibleRows()
            .SelectMany(row => row.Slots)
            .FirstOrDefault(slot => new Rectangle(slot.X, slot.Y, slot.Size, slot.Size).Contains(Game1.getMouseX(), Game1.getMouseY()));
        if (hovered is null)
            return;

        string text = hovered.Candidate.DisplayName;
        if (this.inventoryItemsById.TryGetValue(hovered.Candidate.ItemId, out Item? item))
            text = $"{item.DisplayName}\n{item.getDescription()}";

        IClickableMenu.drawHoverText(b, text, Game1.smallFont);
    }
}
