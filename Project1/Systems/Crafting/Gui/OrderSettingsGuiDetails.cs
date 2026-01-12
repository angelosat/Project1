using Start_a_Town_.UI;

namespace Start_a_Town_
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
            Net.Client.Instance.Map.Events.ListenTo<CraftOrderUpdatedEvent>(OnOrderUpdated);
            var panel = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            //var box = this.ListCollapsible.ToScrollableBox(200, 400);
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
