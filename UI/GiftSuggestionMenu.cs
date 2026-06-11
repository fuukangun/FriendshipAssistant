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
    private const int RowHeight = 72;
    private const int VisibleRows = 5;

    private readonly NPC npc;
    private readonly GiftMenuModel model;
    private readonly Action<GiftCandidate> onGiftSelected;
    private readonly string title;
    private readonly string closeText;
    private readonly string? bannerText;
    private int scrollOffset;

    public GiftSuggestionMenu(
        NPC npc,
        GiftMenuModel model,
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
        this.onGiftSelected = onGiftSelected;
        this.title = title;
        this.closeText = closeText;
        this.bannerText = bannerText;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        IReadOnlyList<GiftMenuRow> visibleRows = this.model.GetVisibleRows(this.scrollOffset, VisibleRows);
        for (int index = 0; index < visibleRows.Count; index++)
        {
            Rectangle row = this.GetRowBounds(index);
            if (!row.Contains(x, y))
                continue;

            GiftMenuRow menuRow = visibleRows[index];
            if (menuRow.Candidate is null)
                return;

            this.onGiftSelected(menuRow.Candidate);
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
            this.scrollOffset++;
        else if (direction > 0)
            this.scrollOffset--;

        this.scrollOffset = Math.Clamp(this.scrollOffset, 0, Math.Max(0, this.model.Rows.Count - VisibleRows));
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

        IReadOnlyList<GiftMenuRow> visibleRows = this.model.GetVisibleRows(this.scrollOffset, VisibleRows);
        for (int index = 0; index < visibleRows.Count; index++)
        {
            GiftMenuRow rowData = visibleRows[index];
            Rectangle row = this.GetRowBounds(index);
            Color rowColor = rowData.IsHeader ? Color.SaddleBrown * 0.3f : Color.Wheat * 0.25f;
            b.Draw(Game1.staminaRect, row, rowColor);

            string text = rowData.Candidate is null
                ? rowData.Text
                : $"{rowData.Text}  ({rowData.Candidate.Taste}, Q{rowData.Candidate.Quality})";
            Color textColor = rowData.IsLastGift ? Color.Gray : Game1.textColor;
            Utility.drawTextWithShadow(
                b,
                text,
                Game1.smallFont,
                new Vector2(row.X + 20, row.Y + 18),
                textColor);
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
    }

    private Rectangle GetRowBounds(int index)
    {
        return new Rectangle(
            this.xPositionOnScreen + 48,
            this.yPositionOnScreen + 118 + index * RowHeight,
            this.width - 96,
            RowHeight - 8);
    }
}
