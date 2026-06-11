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

    private readonly NPC npc;
    private readonly IReadOnlyList<GiftCandidate> candidates;
    private readonly Action<GiftCandidate> onGiftSelected;
    private readonly string title;
    private readonly string closeText;

    public GiftSuggestionMenu(
        NPC npc,
        IEnumerable<GiftCandidate> candidates,
        Action<GiftCandidate> onGiftSelected,
        string title,
        string closeText)
        : base(
            Game1.uiViewport.Width / 2 - MenuWidth / 2,
            Game1.uiViewport.Height / 2 - MenuHeight / 2,
            MenuWidth,
            MenuHeight,
            showUpperRightCloseButton: true)
    {
        this.npc = npc;
        this.candidates = candidates.ToList();
        this.onGiftSelected = onGiftSelected;
        this.title = title;
        this.closeText = closeText;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        for (int index = 0; index < this.candidates.Count; index++)
        {
            Rectangle row = this.GetRowBounds(index);
            if (!row.Contains(x, y))
                continue;

            this.onGiftSelected(this.candidates[index]);
            this.exitThisMenu();
            return;
        }

        Rectangle closeBounds = new(this.xPositionOnScreen + this.width / 2 - 80, this.yPositionOnScreen + this.height - 76, 160, 52);
        if (closeBounds.Contains(x, y))
            this.exitThisMenu();
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

        int maxRows = Math.Min(this.candidates.Count, 5);
        for (int index = 0; index < maxRows; index++)
        {
            GiftCandidate candidate = this.candidates[index];
            Rectangle row = this.GetRowBounds(index);
            b.Draw(Game1.staminaRect, row, Color.Wheat * 0.25f);

            Utility.drawTextWithShadow(
                b,
                $"{candidate.DisplayName}  ({candidate.Taste}, Q{candidate.Quality})",
                Game1.smallFont,
                new Vector2(row.X + 20, row.Y + 18),
                Game1.textColor);
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
