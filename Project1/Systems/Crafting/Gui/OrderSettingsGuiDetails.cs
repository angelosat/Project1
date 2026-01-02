using Start_a_Town_.UI;

namespace Start_a_Town_
{
    internal class OrderSettingsGuiDetails : GroupBox
    {
        readonly ListCollapsibleNewNew ListCollapsible;
        public OrderSettingsGuiDetails(OrderSettings order)
        {
            var entries = CraftingGuiBuilder.Build(order);
            this.ListCollapsible = new ListCollapsibleNewNew();
            this.ListCollapsible.Build(entries);
            var panel = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            var box = this.ListCollapsible.ToScrollableBox(200, 400);
            panel.AddControls(box);
            this.AddControls(panel);
        }
    }
}
