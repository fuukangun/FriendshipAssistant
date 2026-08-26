using FriendshipAssistant.Models;
using FriendshipAssistant.Services;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed partial class GiftSuggestionMenu
{
    private void DismissToday()
    {
        this.onDismissToday();
        this.exitThisMenu();
    }

    private void TrySelectGift(GiftMenuItem selection)
    {
        bool live = selection.Source.CanConsume()
            && StardewGiftCandidateFactory.CanAppearInGiftPrompt(selection.Source.Item);
        if (live && this.onGiftSelected(selection))
        {
            this.exitThisMenu();
            return;
        }

        this.RebuildClickableComponents();
    }

    private void TrackMouseFocus(int x, int y)
    {
        Point current = new(x, y);
        if (current == this.lastMousePosition)
            return;

        this.lastMousePosition = current;
        this.usingGamepadFocus = false;
        this.currentlySnappedComponent = this.allClickableComponents.FirstOrDefault(component => component.containsPoint(x, y));
        int id = this.currentlySnappedComponent?.myID ?? -1;
        if (this.componentStates.TryGetValue(id, out GiftMenuComponentState? state))
            this.SaveCurrentGiftFocus(state.Position.RowIndex, state.Position.Column);
    }

    private IReadOnlyList<GiftGridDisplayRow> GetAllRows()
    {
        IReadOnlyList<GiftMenuRow> liveRows = GetLiveRows(this.GetCurrentModel().Rows);
        return this.gridLayout.BuildRows(liveRows);
    }

    private static IReadOnlyList<GiftMenuRow> GetLiveRows(IReadOnlyList<GiftMenuRow> sourceRows)
    {
        List<GiftMenuRow> result = new();
        GiftMenuRow? pendingHeader = null;
        foreach (GiftMenuRow row in sourceRows)
        {
            if (row.IsHeader)
            {
                pendingHeader = row;
                continue;
            }

            bool live = row.MenuItem is null
                || (row.MenuItem.Source.CanConsume()
                    && StardewGiftCandidateFactory.CanAppearInGiftPrompt(row.MenuItem.Source.Item));
            if (!live)
                continue;
            if (pendingHeader is not null)
            {
                result.Add(pendingHeader);
                pendingHeader = null;
            }
            result.Add(row);
        }
        return result;
    }

    private int GetRestoredOrDefaultFocusId()
    {
        GiftMenuSavedFocus saved = this.GetCurrentSavedFocus();
        IReadOnlyList<GiftMenuFocusCandidate> candidates = this.GetGiftFocusCandidates();
        GiftMenuFocusCandidate? resolved = GiftMenuFocusResolver.Resolve(candidates, saved.Item, saved.Position);
        if (resolved is not null)
            return resolved.ComponentId;

        return -1;
    }

    private int GetFirstGiftFocusId()
    {
        GiftMenuFocusCandidate? first = GiftMenuFocusResolver.ResolveFirst(this.GetGiftFocusCandidates());
        return first?.ComponentId ?? -1;
    }

    private IReadOnlyList<GiftMenuFocusCandidate> GetGiftFocusCandidates()
    {
        return this.componentStates
            .Where(pair => pair.Value.Slot.MenuItem is not null)
            .Select(pair => new GiftMenuFocusCandidate(pair.Value.Slot.MenuItem!, pair.Value.Position, pair.Key))
            .ToList();
    }

    private void FocusGift(int row, int column)
    {
        this.SaveCurrentGiftFocus(row, column);
        this.SetFocus(GetGiftComponentId(row, column));
    }

    private void SaveCurrentGiftFocus(int row, int column)
    {
        IReadOnlyList<GiftGridDisplayRow> rows = this.GetAllRows();
        GiftMenuItem? item = row >= 0 && row < rows.Count && column >= 0 && column < rows[row].Slots.Count
            ? rows[row].Slots[column].MenuItem
            : null;
        GiftMenuSavedFocus saved = new(item, new GiftMenuNavigationPosition(row, column, this.GetCurrentScrollOffset()));
        if (this.currentPage == GiftPage.Storage)
            this.storageFocus = saved;
        else
            this.backpackFocus = saved;
    }

    private GiftMenuSavedFocus GetCurrentSavedFocus()
    {
        return this.currentPage == GiftPage.Storage ? this.storageFocus : this.backpackFocus;
    }

    private bool SetFocus(int id)
    {
        ClickableComponent? component = this.allClickableComponents.FirstOrDefault(candidate => candidate.myID == id);
        this.currentlySnappedComponent = component;
        return component is not null;
    }

    private static int GetGiftComponentId(int row, int column)
    {
        return GiftComponentIdBase + row * Columns + column;
    }
}
