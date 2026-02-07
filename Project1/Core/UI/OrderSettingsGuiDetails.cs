using Project1.Core.Net;
using Project1.Core.Towns.Crafting;
using Project1.Core.Towns.Crafting.Gui;
using Project1.Core.UI;
using Project1.Core.UI;

namespace Project1.Core.UI
{
    internal class OrderSettingsGuiDetails : GroupBox
    {
        readonly ListCollapsibleNewNew ListCollapsible;
        readonly OrderSettings Order;
        public OrderSettingsGuiDetails(OrderSettings order)
        {
            this.Order = order;
            var entries = CraftingGuiBuilder.Build(order);
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
