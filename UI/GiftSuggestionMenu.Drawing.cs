using FriendshipAssistant.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed partial class GiftSuggestionMenu
{
    private sealed record NavigationButtonVisual(Rectangle Bounds, string Text, bool Enabled);

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
        bool closeFocused = this.usingGamepadFocus
            && this.currentlySnappedComponent?.myID == CloseComponentId;
        this.upperRightCloseButton.scale = closeFocused
            ? this.upperRightCloseButton.baseScale + FocusedComponentScaleIncrement
            : this.upperRightCloseButton.baseScale;
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

        DrawNavigationButton(
            b,
            new NavigationButtonVisual(this.GetPreviousPageBounds(), "<", this.currentPage == GiftPage.Storage));
        DrawNavigationButton(
            b,
            new NavigationButtonVisual(this.GetNextPageBounds(), ">", this.currentPage == GiftPage.Backpack));
    }

    private void DrawNavigationButton(SpriteBatch b, NavigationButtonVisual visual)
    {
        bool focused = this.usingGamepadFocus && this.currentlySnappedComponent?.bounds == visual.Bounds;
        Color color = visual.Enabled ? (focused ? Color.Wheat : Color.White) : Color.Gray;
        Rectangle bounds = visual.Bounds;
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, color);
        Utility.drawTextWithShadow(b, visual.Text, Game1.dialogueFont, new Vector2(bounds.X + 12, bounds.Y - 2), visual.Enabled ? Game1.textColor : Color.Gray);
    }

    private void DrawDismissButton(SpriteBatch b)
    {
        Rectangle bounds = this.GetDismissButtonBounds();
        bool hovered = bounds.Contains(Game1.getMouseX(), Game1.getMouseY());
        bool focused = this.usingGamepadFocus && this.currentlySnappedComponent?.myID == DismissComponentId;
        IClickableMenu.drawTextureBox(
            b,
            Game1.menuTexture,
            new Rectangle(0, 256, 60, 60),
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            hovered || focused ? Color.Wheat : Color.White);

        Vector2 textSize = Game1.smallFont.MeasureString(this.dismissTodayText);
        Vector2 textPosition = new(
            bounds.X + (bounds.Width - textSize.X) / 2f,
            bounds.Y + (bounds.Height - textSize.Y) / 2f);
        Utility.drawTextWithShadow(b, this.dismissTodayText, Game1.smallFont, textPosition, Game1.textColor);
    }

    private static void DrawHeader(SpriteBatch b, GiftGridDisplayRow row)
    {
        Utility.drawTextWithShadow(b, row.HeaderText ?? string.Empty, Game1.smallFont, new Vector2(row.X, row.Y + 3), Color.DarkGoldenrod);
    }

    private void DrawSlot(SpriteBatch b, GiftMenuSlot slot)
    {
        Rectangle bounds = new(slot.X, slot.Y, slot.Size, slot.Size);
        bool hovered = bounds.Contains(Game1.getMouseX(), Game1.getMouseY());
        bool focused = this.IsGamepadFocused(slot);
        b.Draw(Game1.staminaRect, bounds, hovered || focused ? Color.Wheat * 0.65f : Color.Wheat * 0.35f);
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), bounds.X, bounds.Y, bounds.Width, bounds.Height, Color.White);

        Item? item = slot.MenuItem?.Source.Item;
        if (item is not null)
        {
            float scale = hovered || focused ? 1.12f : 1f;
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
        {
            IClickableMenu.drawHoverText(b, $"{item.DisplayName}\n{item.getDescription()}", Game1.smallFont);
            return;
        }

        if (this.usingGamepadFocus)
            this.DrawGamepadTooltip(b);
    }

    private bool IsGamepadFocused(GiftMenuSlot slot)
    {
        int id = this.currentlySnappedComponent?.myID ?? -1;
        return this.usingGamepadFocus
            && this.componentStates.TryGetValue(id, out GiftMenuComponentState? state)
            && ReferenceEquals(state.Slot.MenuItem?.Source.Item, slot.MenuItem?.Source.Item);
    }

    private void DrawGamepadTooltip(SpriteBatch b)
    {
        int id = this.currentlySnappedComponent?.myID ?? -1;
        if (!this.componentStates.TryGetValue(id, out GiftMenuComponentState? state))
            return;

        GiftMenuSlot focused = state.Slot;
        Item? item = focused.MenuItem?.Source.Item;
        if (item is null)
            return;

        string text = Game1.parseText(
            $"{item.DisplayName}\n{item.getDescription()}",
            Game1.smallFont,
            TooltipTextWrapWidth);
        string[] lines = text.Split('\n');
        int textWidth = (int)Math.Ceiling(lines.Max(line => Game1.smallFont.MeasureString(line).X));
        int tooltipWidth = Math.Clamp(textWidth + TooltipContentPadding, TooltipMinimumWidth, TooltipMaximumWidth);
        int tooltipHeight = Math.Max(TooltipMinimumHeight, lines.Length * Game1.smallFont.LineSpacing + TooltipContentPadding);
        Rectangle viewport = new(
            TooltipViewportInset,
            TooltipViewportInset,
            Game1.uiViewport.Width - TooltipContentPadding,
            Game1.uiViewport.Height - TooltipContentPadding);
        Rectangle menu = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        Rectangle available = Rectangle.Intersect(viewport, menu);
        GiftMenuTooltipBounds placed = GiftMenuTooltipLayout.Place(
            new GiftMenuTooltipBounds(
                new GiftMenuTooltipPoint(focused.X, focused.Y),
                new GiftMenuTooltipSize(focused.Size, focused.Size)),
            new GiftMenuTooltipBounds(
                new GiftMenuTooltipPoint(available.X, available.Y),
                new GiftMenuTooltipSize(available.Width, available.Height)),
            new GiftMenuTooltipSize(tooltipWidth, tooltipHeight));

        IClickableMenu.drawHoverText(
            b,
            text,
            Game1.smallFont,
            overrideX: placed.X,
            overrideY: placed.Y);
    }
}
