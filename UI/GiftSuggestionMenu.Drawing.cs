using FriendshipAssistant.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed partial class GiftSuggestionMenu
{
    public override void draw(SpriteBatch b)
    {
        Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);
        Utility.drawTextWithShadow(b, $"{this.title}: {this.npc.displayName}", Game1.dialogueFont, new Vector2(this.xPositionOnScreen + 48, this.yPositionOnScreen + 48), Game1.textColor);

        if (!string.IsNullOrWhiteSpace(this.bannerText))
        {
            Rectangle banner = new(this.xPositionOnScreen + 48, this.yPositionOnScreen + 88, this.width - 96, 34);
            b.Draw(Game1.staminaRect, banner, Color.Goldenrod * 0.25f);
            Utility.drawTextWithShadow(b, this.bannerText, Game1.smallFont, new Vector2(banner.X + 16, banner.Y + 6), Color.DarkGoldenrod);
        }

        this.DrawPageNavigation(b);
        IReadOnlyList<GiftGridDisplayRow> visibleRows = this.GetVisibleRows();
        if (visibleRows.Count == 0)
        {
            Utility.drawTextWithShadow(b, this.emptyText, Game1.smallFont, new Vector2(this.xPositionOnScreen + 64, this.yPositionOnScreen + 220), Game1.textColor);
        }
        else
        {
            foreach (GiftGridDisplayRow row in visibleRows)
            {
                if (row.IsHeader)
                    DrawHeader(b, row);
                else
                    foreach (GiftMenuSlot slot in row.Slots)
                        DrawSlot(b, slot);
            }
        }

        this.DrawScrollBar(b);
        this.DrawDismissButton(b);
        this.upperRightCloseButton.draw(b);
        this.drawMouse(b);
        this.DrawHoverTooltip(b);
    }

    private void DrawPageNavigation(SpriteBatch b)
    {
        string label = this.currentPage == GiftPage.Storage ? this.storageText : this.backpackText;
        Utility.drawTextWithShadow(b, label, Game1.smallFont, new Vector2(this.xPositionOnScreen + 110, this.yPositionOnScreen + NavigationY + 9), Game1.textColor);

        if (this.storageModel is null)
            return;

        DrawNavigationButton(b, this.GetPreviousPageBounds(), "<", this.currentPage == GiftPage.Storage);
        DrawNavigationButton(b, this.GetNextPageBounds(), ">", this.currentPage == GiftPage.Backpack);
    }

    private static void DrawNavigationButton(SpriteBatch b, Rectangle bounds, string text, bool enabled)
    {
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, enabled ? Color.White : Color.Gray);
        Utility.drawTextWithShadow(b, text, Game1.dialogueFont, new Vector2(bounds.X + 12, bounds.Y - 2), enabled ? Game1.textColor : Color.Gray);
    }

    private void DrawDismissButton(SpriteBatch b)
    {
        Rectangle bounds = this.GetDismissButtonBounds();
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White);
        Utility.drawTextWithShadow(b, this.dismissTodayText, Game1.smallFont, new Vector2(bounds.X + 18, bounds.Y + 12), Game1.textColor);
    }

    private static void DrawHeader(SpriteBatch b, GiftGridDisplayRow row)
    {
        Utility.drawTextWithShadow(b, row.HeaderText ?? string.Empty, Game1.smallFont, new Vector2(row.X, row.Y + 3), Color.DarkGoldenrod);
    }

    private static void DrawSlot(SpriteBatch b, GiftMenuSlot slot)
    {
        Rectangle bounds = new(slot.X, slot.Y, slot.Size, slot.Size);
        bool hovered = bounds.Contains(Game1.getMouseX(), Game1.getMouseY());
        b.Draw(Game1.staminaRect, bounds, hovered ? Color.Wheat * 0.65f : Color.Wheat * 0.35f);
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White);

        Item? item = slot.MenuItem?.Source.Item;
        if (item is not null)
        {
            float scale = hovered ? 1.12f : 1f;
            float offset = (IconBaseSize - IconBaseSize * scale) / 2f;
            item.drawInMenu(b, new Vector2(bounds.X + offset, bounds.Y + offset), scale);
        }

        if (slot.IsLastGift)
            Utility.drawTextWithShadow(b, "*", Game1.smallFont, new Vector2(bounds.Right - 18, bounds.Y + 2), Color.Gold);
    }

    private void DrawScrollBar(SpriteBatch b)
    {
        GiftScrollBarLayout.ScrollBarState scrollBar = this.GetScrollBarState();
        if (!scrollBar.IsVisible)
            return;

        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBar.TrackX, scrollBar.TrackY, scrollBar.TrackWidth, scrollBar.TrackHeight, Color.White, 4f, drawShadow: false);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(435, 463, 6, 10), scrollBar.TrackX, scrollBar.ThumbY, scrollBar.TrackWidth, scrollBar.ThumbHeight, Color.White, 4f, drawShadow: false);
    }

    private void DrawHoverTooltip(SpriteBatch b)
    {
        GiftMenuSlot? hovered = this.GetVisibleRows()
            .SelectMany(row => row.Slots)
            .FirstOrDefault(slot => new Rectangle(slot.X, slot.Y, slot.Size, slot.Size).Contains(Game1.getMouseX(), Game1.getMouseY()));
        Item? item = hovered?.MenuItem?.Source.Item;
        if (item is not null)
            IClickableMenu.drawHoverText(b, $"{item.DisplayName}\n{item.getDescription()}", Game1.smallFont);
    }
}
