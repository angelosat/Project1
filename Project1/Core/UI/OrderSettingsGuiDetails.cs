using Project1.Core.Screens;
using Project1.Core.Systems.Crafting;
using Project1.Framework.Helpers;
using Project1.Framework.UI;

namespace Project1.Core.UI;

internal class OrderSettingsGuiDetails : GroupBox
{
    readonly LabelNew LabelWorker;
    readonly SliderNewInt SliderMastery;
    readonly ListCollapsible<Def> ListCollapsible;
    readonly CraftingOrder Order;

    public OrderSettingsGuiDetails(CraftingOrder order)
    {
        var map = order.Workstation.Map;
        this.LabelWorker = new(() => $"Current Worker: {map.World.Get(order.CurrentWorker)?.Name ?? "none"}");

        this.Order = order;
        this.SliderMastery = new(
            () => this.Order.MinMastery, 
            v => Ingame.Instance.Events.Post(new PlayerSetOrderMinMasteryEvent(order.Workstation.Map.ID, order.Id, v)), 
            100, 0, 100, 1);
        this.SliderMastery.InvalidateOn(order.Notifier);

        var entries = IngredientGroupBuilder.BuildNew(order);
        this.ListCollapsible = new();
        this.ListCollapsible.Build(entries);
        order.Workstation.Map.Events.ListenTo<CraftOrderUpdatedEvent>(OnOrderUpdated);
        var panel = new Panel() { AutoSize = true };// { AutoSize = false }.SetClientDimensions(200, 200);
        var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 200, ScrollModes.Vertical);
        panel.AddControls(box);
        this.AddControlsVertically(
            this.LabelWorker,
            this.SliderMastery.ToPanelLabeled(()=>$"Minimum Mastery Allowed: {this.Order.MinMastery}"),//.InvalidateOn(order.Notifier),
            panel);
        this.InvalidateOn(order.Notifier);
    }

    private void OnOrderUpdated(CraftOrderUpdatedEvent e)
    {
        if (e.Order == this.Order)
            this.ListCollapsible.Invalidate(true);
    }
}
