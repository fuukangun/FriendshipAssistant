using FriendshipAssistant.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed partial class GiftSuggestionMenu : IClickableMenu
{
    private const int MenuWidth = 720;
    private const int MenuHeight = 560;
    private const int SlotSize = 64;
    private const int SlotSpacing = 14;
    private const int Columns = 7;
    private const int HeaderHeight = 30;
    private const int RowSpacing = 12;
    private const int VisibleDisplayRows = 4;
    private const int IconBaseSize = 64;
    private const int CloseButtonSize = 48;
    private const int CloseButtonInset = 28;
    private const int ScrollBarWidth = 24;
    private const int ScrollBarInsetRight = 48;
    private const int ScrollBarTop = 166;
    private const int ScrollBarHeight = 280;
    private const int ScrollBarFrameInset = 4;
    private const int NavigationY = 116;
    private const int NavigationButtonSize = 40;
    private const int DismissButtonWidth = 220;
    private const int DismissButtonHeight = 40;
    private const int DismissButtonBottomInset = 34;

    private enum GiftPage
    {
        Backpack,
        Storage
    }

    private readonly NPC npc;
    private readonly GiftMenuModel backpackModel;
    private readonly GiftMenuModel? storageModel;
    private readonly GiftGroupedGridLayout gridLayout = new(Columns, SlotSize, SlotSpacing, HeaderHeight, RowSpacing);
    private readonly Action<GiftMenuItem> onGiftSelected;
    private readonly Action onDismissToday;
    private readonly string title;
    private readonly string closeText;
    private readonly string dismissTodayText;
    private readonly string backpackText;
    private readonly string storageText;
    private readonly string emptyText;
    private readonly string? bannerText;
    private int backpackScrollRowOffset;
    private int storageScrollRowOffset;
    private GiftPage currentPage;
    private bool isDraggingScrollBar;
    private int scrollBarDragOffsetY;

    public GiftSuggestionMenu(
        NPC npc,
        GiftMenuModel backpackModel,
        GiftMenuModel? storageModel,
        Action<GiftMenuItem> onGiftSelected,
        Action onDismissToday,
        string title,
        string closeText,
        string dismissTodayText,
        string backpackText,
        string storageText,
        string emptyText,
        string? bannerText = null)
        : base(
            Game1.uiViewport.Width / 2 - MenuWidth / 2,
            Game1.uiViewport.Height / 2 - MenuHeight / 2,
            MenuWidth,
            MenuHeight,
            showUpperRightCloseButton: true)
    {
        this.npc = npc;
        this.backpackModel = backpackModel;
        this.storageModel = storageModel;
        this.onGiftSelected = onGiftSelected;
        this.onDismissToday = onDismissToday;
        this.title = title;
        this.closeText = closeText;
        this.dismissTodayText = dismissTodayText;
        this.backpackText = backpackText;
        this.storageText = storageText;
        this.emptyText = emptyText;
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

        if (this.storageModel is not null && this.GetPreviousPageBounds().Contains(x, y))
        {
            this.currentPage = GiftPage.Backpack;
            return;
        }

        if (this.storageModel is not null && this.GetNextPageBounds().Contains(x, y))
        {
            this.currentPage = GiftPage.Storage;
            return;
        }

        if (this.GetDismissButtonBounds().Contains(x, y))
        {
            this.onDismissToday();
            this.exitThisMenu();
            return;
        }

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

        GiftMenuItem? clicked = this.gridLayout.HitTestMenuItem(this.GetVisibleRows(), x, y);
        if (clicked is not null)
        {
            this.onGiftSelected(clicked);
            this.exitThisMenu();
        }
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);

        ref int scrollOffset = ref this.GetCurrentScrollOffset();
        if (direction < 0)
            scrollOffset++;
        else if (direction > 0)
            scrollOffset--;

        int rowCount = this.gridLayout.BuildRows(this.GetCurrentModel().Rows).Count;
        scrollOffset = Math.Clamp(scrollOffset, 0, Math.Max(0, rowCount - VisibleDisplayRows));
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);

        if (this.isDraggingScrollBar)
            this.SetScrollOffsetFromThumbY(y - this.scrollBarDragOffsetY);
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        this.isDraggingScrollBar = false;
    }

    private GiftMenuModel GetCurrentModel()
    {
        return this.currentPage == GiftPage.Storage && this.storageModel is not null
            ? this.storageModel
            : this.backpackModel;
    }

    private ref int GetCurrentScrollOffset()
    {
        return ref this.currentPage == GiftPage.Storage
            ? ref this.storageScrollRowOffset
            : ref this.backpackScrollRowOffset;
    }

    private IReadOnlyList<GiftGridDisplayRow> GetVisibleRows()
    {
        return this.gridLayout.PositionRows(
            this.gridLayout.BuildVisibleRows(this.GetCurrentModel().Rows, this.GetCurrentScrollOffset(), VisibleDisplayRows),
            originX: this.xPositionOnScreen + 64,
            originY: this.yPositionOnScreen + ScrollBarTop);
    }

    private Rectangle GetPreviousPageBounds()
    {
        return new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + NavigationY, NavigationButtonSize, NavigationButtonSize);
    }

    private Rectangle GetNextPageBounds()
    {
        return new Rectangle(this.xPositionOnScreen + this.width - 88, this.yPositionOnScreen + NavigationY, NavigationButtonSize, NavigationButtonSize);
    }

    private Rectangle GetDismissButtonBounds()
    {
        GiftMenuChrome.ActionButtonBounds bounds = GiftMenuChrome.GetDismissButtonBounds(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            DismissButtonWidth,
            DismissButtonHeight,
            DismissButtonBottomInset);
        return new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private GiftScrollBarLayout.ScrollBarState GetScrollBarState()
    {
        int totalRows = this.gridLayout.BuildRows(this.GetCurrentModel().Rows).Count;
        return GiftScrollBarLayout.Calculate(
            trackX: this.xPositionOnScreen + this.width - ScrollBarInsetRight - ScrollBarWidth,
            trackY: this.yPositionOnScreen + ScrollBarTop,
            trackWidth: ScrollBarWidth,
            trackHeight: ScrollBarHeight,
            totalRows,
            VisibleDisplayRows,
            this.GetCurrentScrollOffset(),
            ScrollBarFrameInset);
    }

    private void SetScrollOffsetFromThumbY(int thumbY)
    {
        GiftScrollBarLayout.ScrollBarState scrollBar = this.GetScrollBarState();
        int totalRows = this.gridLayout.BuildRows(this.GetCurrentModel().Rows).Count;
        this.GetCurrentScrollOffset() = GiftScrollBarLayout.ScrollOffsetFromThumbY(
            thumbY,
            scrollBar.TrackY,
            scrollBar.TrackHeight,
            scrollBar.ThumbHeight,
            totalRows,
            VisibleDisplayRows,
            ScrollBarFrameInset);
    }

}
