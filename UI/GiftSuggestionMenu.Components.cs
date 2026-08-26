using StardewValley.Menus;

namespace FriendshipAssistant.UI;

public sealed partial class GiftSuggestionMenu
{
    public override void populateClickableComponentList()
    {
        this.RebuildClickableComponents();
    }

    private void ConfigureComponentNeighbors()
    {
        foreach ((int id, GiftMenuComponentState state) in this.componentStates)
        {
            ClickableComponent component = this.GetComponent(id);
            component.leftNeighborID = this.GetHorizontalNeighborId(state.Position, -1);
            component.rightNeighborID = this.GetHorizontalNeighborId(state.Position, 1);
            component.upNeighborID = this.GetVerticalNeighborId(state.Position, -1);
            component.downNeighborID = this.GetVerticalNeighborId(state.Position, 1);
        }
    }

    private int GetHorizontalNeighborId(GiftMenuNavigationPosition position, int direction)
    {
        GiftMenuNavigationPosition? target = GiftMenuComponentNavigation.FindHorizontalNeighbor(
            this.componentStates.Values.Select(state => state.Position).ToList(),
            position,
            direction);
        return GiftMenuComponentNavigation.GetComponentIdOrIgnore(
            target,
            position => GetGiftComponentId(position.RowIndex, position.Column));
    }

    private int GetVerticalNeighborId(GiftMenuNavigationPosition position, int direction)
    {
        GiftMenuNavigationPosition? target = GiftMenuComponentNavigation.FindVerticalNeighbor(
            this.componentStates.Values.Select(state => state.Position).ToList(),
            position,
            direction);
        return GiftMenuComponentNavigation.GetComponentIdOrIgnore(
            target,
            position => GetGiftComponentId(position.RowIndex, position.Column));
    }

    private ClickableComponent GetComponent(int id)
    {
        return this.allClickableComponents.First(component => component.myID == id);
    }
}
