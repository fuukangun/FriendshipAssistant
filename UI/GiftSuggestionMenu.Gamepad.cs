using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed partial class GiftSuggestionMenu : IGiftMenuGamepadActions
{
    private sealed record GiftMenuComponentState(GiftMenuSlot Slot, GiftMenuNavigationPosition Position);

    private sealed record GiftMenuSavedFocus(GiftMenuItem? Item, GiftMenuNavigationPosition Position);

    private const int CloseComponentId = 100;
    private const int PreviousPageComponentId = 101;
    private const int NextPageComponentId = 102;
    private const int DismissComponentId = 103;
    private const int GiftComponentIdBase = 1000;
    private const float StickDeadzone = 0.45f;

    private readonly Dictionary<int, GiftMenuComponentState> componentStates = new();
    private readonly GamepadRepeatGate repeatGate = new();
    private Point lastMousePosition;
    private bool usingGamepadFocus;
    private GiftMenuGamepadCommand lastRightStickCommand;
    private readonly GamepadRepeatGate rightStickRepeatGate = new();
    private GiftMenuSavedFocus backpackFocus = new(null, new GiftMenuNavigationPosition(-1, 0, 0));
    private GiftMenuSavedFocus storageFocus = new(null, new GiftMenuNavigationPosition(-1, 0, 0));

    public override void receiveGamePadButton(Buttons button)
    {
        if (GiftMenuGamepadInput.IsDirectionalButton(button))
            return;

        this.usingGamepadFocus = true;
        if (this.TryHandleGamepadCommand(button))
            return;

        base.receiveGamePadButton(button);
    }

    public override void gamePadButtonHeld(Buttons button)
    {
        if (GiftMenuGamepadInput.IsDirectionalButton(button))
            return;

        GiftMenuGamepadCommand command = GetGamepadCommand(button);
        int direction = GiftMenuGamepadInput.GetScrollDirection(command);
        if (direction != 0)
        {
            if (this.repeatGate.ShouldRepeat(command, GetElapsedMilliseconds()))
            {
                this.usingGamepadFocus = true;
                GiftMenuGamepadDispatcher.Dispatch(command, this);
            }
            return;
        }

        base.gamePadButtonHeld(button);
    }

    private bool TryHandleGamepadCommand(Buttons button)
    {
        GiftMenuGamepadCommand command = GetGamepadCommand(button);
        if (GiftMenuGamepadInput.GetScrollDirection(command) != 0)
            this.repeatGate.Start(command, GetElapsedMilliseconds());
        return GiftMenuGamepadDispatcher.Dispatch(command, this);
    }

    public override void update(GameTime time)
    {
        base.update(time);

        GamePadState state = GamePad.GetState(PlayerIndex.One);
        if (!state.IsConnected)
            return;

        this.UpdateRightStick(state, GetElapsedMilliseconds());
    }

    private static GiftMenuGamepadCommand GetGamepadCommand(Buttons button)
    {
        return button switch
        {
            Buttons.A => GiftMenuGamepadCommand.Confirm,
            Buttons.B => GiftMenuGamepadCommand.Cancel,
            Buttons.LeftShoulder => GiftMenuGamepadCommand.BackpackPage,
            Buttons.RightShoulder => GiftMenuGamepadCommand.StoragePage,
            Buttons.LeftTrigger => GiftMenuGamepadCommand.PageUp,
            Buttons.RightTrigger => GiftMenuGamepadCommand.PageDown,
            Buttons.X => GiftMenuGamepadCommand.DismissToday,
            _ => GiftMenuGamepadCommand.None
        };
    }

    private static double GetElapsedMilliseconds()
    {
        return Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
    }

    void IGiftMenuGamepadActions.Confirm() => this.ActivateFocusedComponent();

    void IGiftMenuGamepadActions.Cancel() => this.exitThisMenu();

    void IGiftMenuGamepadActions.SelectBackpackPage() => this.SwitchPage(GiftPage.Backpack);

    void IGiftMenuGamepadActions.SelectStoragePage() => this.SwitchPage(GiftPage.Storage);

    void IGiftMenuGamepadActions.PageUp() => this.ScrollFocusedPage(-1);

    void IGiftMenuGamepadActions.PageDown() => this.ScrollFocusedPage(1);

    void IGiftMenuGamepadActions.DismissToday() => this.DismissToday();

    private void UpdateRightStick(GamePadState state, double elapsedMilliseconds)
    {
        Vector2 stick = state.ThumbSticks.Right;
        GiftMenuGamepadCommand command = GiftMenuGamepadInput.GetRightStickCommand(
            stick.X,
            stick.Y,
            StickDeadzone);
        if (command == GiftMenuGamepadCommand.None)
        {
            this.lastRightStickCommand = GiftMenuGamepadCommand.None;
            return;
        }

        this.usingGamepadFocus = true;
        if (command != this.lastRightStickCommand)
        {
            this.lastRightStickCommand = command;
            this.rightStickRepeatGate.Start(command, elapsedMilliseconds);
            this.MoveFocusWithRightStick(command);
        }
        else if (this.rightStickRepeatGate.ShouldRepeat(command, elapsedMilliseconds))
        {
            this.MoveFocusWithRightStick(command);
        }
    }

    private void MoveFocusWithRightStick(GiftMenuGamepadCommand command)
    {
        switch (command)
        {
            case GiftMenuGamepadCommand.GridLeft:
                if (this.MoveFocusHorizontal(-1))
                    this.MovePointerToFocusedComponent();
                break;
            case GiftMenuGamepadCommand.GridRight:
                if (this.MoveFocusHorizontal(1))
                    this.MovePointerToFocusedComponent();
                break;
            case GiftMenuGamepadCommand.GridUp:
                this.MoveFocusVerticalWithinViewport(-1);
                break;
            case GiftMenuGamepadCommand.GridDown:
                this.MoveFocusVerticalWithinViewport(1);
                break;
        }
    }

    public override void snapToDefaultClickableComponent()
    {
        this.FocusFirstGiftAndMovePointer();
    }

    private void InitializeGamepadComponents()
    {
        this.lastMousePosition = new Point(Game1.getMouseX(), Game1.getMouseY());
        this.GetCurrentScrollOffset() = 0;
        this.RebuildClickableComponents();
        this.snapToDefaultClickableComponent();
    }

    private void RebuildClickableComponents()
    {
        this.allClickableComponents = new List<ClickableComponent>();
        this.componentStates.Clear();

        this.upperRightCloseButton.myID = CloseComponentId;

        IReadOnlyList<GiftGridDisplayRow> rows = this.GetAllRows();
        ref int currentOffset = ref this.GetCurrentScrollOffset();
        currentOffset = Math.Clamp(currentOffset, 0, Math.Max(0, rows.Count - VisibleDisplayRows));
        int offset = currentOffset;
        IReadOnlyList<GiftGridDisplayRow> visible = this.gridLayout.PositionRows(
            rows.Skip(offset).Take(VisibleDisplayRows),
            this.xPositionOnScreen + 64,
            this.yPositionOnScreen + ScrollBarTop);

        for (int localRow = 0; localRow < visible.Count; localRow++)
        {
            int rowIndex = offset + localRow;
            for (int column = 0; column < visible[localRow].Slots.Count; column++)
            {
                GiftMenuSlot slot = visible[localRow].Slots[column];
                int id = GetGiftComponentId(rowIndex, column);
                Rectangle bounds = new(slot.X, slot.Y, slot.Size, slot.Size);
                this.allClickableComponents.Add(new ClickableComponent(bounds, slot.Candidate.DisplayName) { myID = id });
                GiftMenuNavigationPosition position = new(rowIndex, column, offset);
                this.componentStates[id] = new GiftMenuComponentState(slot, position);
            }
        }

        this.ConfigureComponentNeighbors();
        this.SetFocus(this.GetRestoredOrDefaultFocusId());
    }

    private void ActivateFocusedComponent()
    {
        int id = this.currentlySnappedComponent?.myID ?? -1;
        if (id == CloseComponentId)
            this.exitThisMenu();
        else if (id == PreviousPageComponentId)
            this.SwitchPage(GiftPage.Backpack);
        else if (id == NextPageComponentId)
            this.SwitchPage(GiftPage.Storage);
        else if (this.componentStates.TryGetValue(id, out GiftMenuComponentState? state) && state.Slot.MenuItem is not null)
            this.TrySelectGift(state.Slot.MenuItem);
    }

    private bool MoveFocusHorizontal(int direction)
    {
        int id = this.currentlySnappedComponent?.myID ?? -1;
        if (this.componentStates.TryGetValue(id, out GiftMenuComponentState? state))
        {
            int targetColumn = state.Position.Column + direction;
            int targetId = GetGiftComponentId(state.Position.RowIndex, targetColumn);
            if (targetColumn >= 0 && this.componentStates.ContainsKey(targetId))
            {
                this.FocusGift(state.Position.RowIndex, targetColumn);
                return true;
            }
        }

        return false;
    }

    private void MovePointerToFocusedComponent()
    {
        ClickableComponent? component = this.currentlySnappedComponent;
        if (component is null)
            return;

        GiftMenuPointerPosition target = GiftMenuPointerNavigator.GetComponentCenter(
            component.bounds.X,
            component.bounds.Y,
            component.bounds.Width,
            component.bounds.Height);
        Game1.setMousePosition(target.X, target.Y);
        this.lastMousePosition = new Point(target.X, target.Y);
    }

    private void FocusFirstGiftAndMovePointer()
    {
        this.usingGamepadFocus = true;
        if (this.SetFocus(this.GetFirstGiftFocusId()))
            this.MovePointerToFocusedComponent();
    }

    private void MoveFocusVerticalWithinViewport(int direction)
    {
        int id = this.currentlySnappedComponent?.myID ?? -1;
        if (!this.componentStates.TryGetValue(id, out GiftMenuComponentState? state))
            return;

        GiftMenuFocusResult result = GiftMenuFocusNavigator.MoveWithinViewport(
            this.CreateNavigationRequest(state.Position),
            direction);
        this.ApplyFocusResult(result);
    }

    private void ScrollFocusedPage(int direction)
    {
        int id = this.currentlySnappedComponent?.myID ?? -1;
        GiftMenuNavigationPosition current = this.componentStates.TryGetValue(id, out GiftMenuComponentState? state)
            ? state.Position
            : this.GetCurrentSavedFocus().Position;
        GiftMenuNavigationRequest request = this.CreateNavigationRequest(current);
        GiftMenuNavigationPosition? anchor = GiftMenuFocusNavigator.GetPageScrollAnchor(request);
        if (anchor is null)
            return;

        GiftMenuFocusResult result = GiftMenuFocusNavigator.ScrollPage(
            new GiftMenuNavigationRequest(request.Rows, anchor.Value, request.VisibleRows),
            direction);
        this.ApplyFocusResult(result);
    }

    private void ApplyFocusResult(GiftMenuFocusResult result)
    {
        if (result.Target == GiftMenuFocusTarget.TopChrome)
        {
            return;
        }
        if (result.Target == GiftMenuFocusTarget.Dismiss)
        {
            return;
        }

        this.GetCurrentScrollOffset() = result.ScrollOffset;
        this.SaveCurrentGiftFocus(result.RowIndex, result.Column);
        this.RebuildClickableComponents();
        this.FocusGift(result.RowIndex, result.Column);
    }

    private GiftMenuNavigationRequest CreateNavigationRequest(GiftMenuNavigationPosition position)
    {
        GiftMenuNavigationPosition current = new(
            position.RowIndex,
            position.Column,
            this.GetCurrentScrollOffset());
        return new GiftMenuNavigationRequest(this.GetAllRows(), current, VisibleDisplayRows);
    }

    private void SwitchPage(GiftPage page)
    {
        if (page == this.currentPage || this.storageModel is null)
            return;
        this.currentPage = page;
        this.GetCurrentScrollOffset() = 0;
        this.RebuildClickableComponents();
        this.snapToDefaultClickableComponent();
    }

}
