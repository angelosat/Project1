using Project1.Core.Crafting;
using Project1.Core.Networking;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    internal class OrderSettingsGuiDetails : GroupBox
    {
        readonly ListCollapsibleNewNew ListCollapsible;
        readonly CraftingOrder Order;
        public OrderSettingsGuiDetails(CraftingOrder order)
        {
            this.Order = order;
            var entries = IngredientGroupBuilder.Build(order);
            this.ListCollapsible = new ListCollapsibleNewNew();
            this.ListCollapsible.Build(entries);
            Client.Instance.Map.Events.ListenTo<CraftOrderUpdatedEvent>(OnOrderUpdated);
            var panel = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 400, ScrollModes.Vertical);
            panel.AddControls(box);
            this.AddControls(panel);
        }

        private void OnOrderUpdated(CraftOrderUpdatedEvent e)
        {
            if (e.Order == this.Order)
                this.ListCollapsible.Invalidate(true);
        }
    }
}
