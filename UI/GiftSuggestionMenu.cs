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
    private const int TooltipViewportInset = 16;
    private const int TooltipContentPadding = 32;
    private const int TooltipMinimumWidth = 160;
    private const int TooltipMaximumWidth = 360;
    private const int TooltipMinimumHeight = 96;
    private const int TooltipTextWrapWidth = 328;
    private const float FocusedComponentScaleIncrement = 0.1f;

    private enum GiftPage
    {
        Backpack,
        Storage
    }

    private readonly NPC npc;
    private readonly GiftMenuModel backpackModel;
    private readonly GiftMenuModel? storageModel;
    private readonly GiftGroupedGridLayout gridLayout = new(Columns, SlotSize, SlotSpacing, HeaderHeight, RowSpacing);
    private readonly Func<GiftMenuItem, bool> onGiftSelected;
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
        GiftSuggestionMenuOptions options)
        : base(
            Game1.uiViewport.Width / 2 - MenuWidth / 2,
            Game1.uiViewport.Height / 2 - MenuHeight / 2,
            MenuWidth,
            MenuHeight,
            showUpperRightCloseButton: true)
        {
        this.npc = npc;
        this.backpackModel = options.Models.Backpack;
        this.storageModel = options.Models.Storage;
        this.onGiftSelected = options.Actions.SelectGift;
        this.onDismissToday = options.Actions.DismissToday;
        this.title = options.Text.Menu.Title;
        this.closeText = options.Text.Menu.Close;
        this.dismissTodayText = options.Text.Menu.DismissToday;
        this.backpackText = options.Text.Pages.Backpack;
        this.storageText = options.Text.Pages.Storage;
        this.emptyText = options.Text.Pages.Empty;
        this.bannerText = options.Text.Banner;
        if (this.backpackModel.Rows.Count == 0 && this.storageModel?.Rows.Count > 0)
            this.currentPage = GiftPage.Storage;

        GiftMenuChrome.CloseButtonBounds closeBounds = GiftMenuChrome.GetCloseButtonBounds(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            CloseButtonSize,
            CloseButtonInset);
        this.upperRightCloseButton.bounds = new Rectangle(closeBounds.X, closeBounds.Y, closeBounds.Width, closeBounds.Height);
        this.InitializeGamepadComponents();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.usingGamepadFocus = false;
        base.receiveLeftClick(x, y, playSound);

        if (this.storageModel is not null && this.GetPreviousPageBounds().Contains(x, y))
        {
            this.SwitchPage(GiftPage.Backpack);
            return;
        }

        if (this.storageModel is not null && this.GetNextPageBounds().Contains(x, y))
        {
            this.SwitchPage(GiftPage.Storage);
            return;
        }

        if (this.GetDismissButtonBounds().Contains(x, y))
        {
            this.DismissToday();
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
            this.TrySelectGift(clicked);
    }

    public override void receiveScrollWheelAction(int direction)
    {
        this.usingGamepadFocus = false;
        base.receiveScrollWheelAction(direction);

        ref int scrollOffset = ref this.GetCurrentScrollOffset();
        if (direction < 0)
            scrollOffset++;
        else if (direction > 0)
            scrollOffset--;

        int rowCount = this.GetAllRows().Count;
        scrollOffset = Math.Clamp(scrollOffset, 0, Math.Max(0, rowCount - VisibleDisplayRows));
        this.RebuildClickableComponents();
    }

    public override void leftClickHeld(int x, int y)
    {
        this.usingGamepadFocus = false;
        base.leftClickHeld(x, y);

        if (this.isDraggingScrollBar)
            this.SetScrollOffsetFromThumbY(y - this.scrollBarDragOffsetY);
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        this.isDraggingScrollBar = false;
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        this.TrackMouseFocus(x, y);
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
            this.GetAllRows().Skip(this.GetCurrentScrollOffset()).Take(VisibleDisplayRows),
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
        int totalRows = this.GetAllRows().Count;
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
        int totalRows = this.GetAllRows().Count;
        this.GetCurrentScrollOffset() = GiftScrollBarLayout.ScrollOffsetFromThumbY(
            thumbY,
            scrollBar.TrackY,
            scrollBar.TrackHeight,
            scrollBar.ThumbHeight,
            totalRows,
            VisibleDisplayRows,
            ScrollBarFrameInset);
        this.RebuildClickableComponents();
    }

}
