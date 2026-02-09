using Project1.Core.UI;
using Project1.Core.Net;
using Project1.Framework.UI;
using Project1.Core.Crafting.Gui;

namespace Project1.Core.Towns.Stockpiles
{
    internal class StockpileFiltersGui : GroupBox
    {
        readonly Stockpile Stockpile;
        readonly ListCollapsibleNewNew ListCollapsible;
        public StockpileFiltersGui(Stockpile stockpile)
        {
            this.Stockpile = stockpile;
            var entries = CraftingGuiBuilder.Build(stockpile);
            this.ListCollapsible = new ListCollapsibleNewNew();
            this.ListCollapsible.BuildNew(entries);
            Client.Instance.Map.Events.ListenTo<StockpileUpdatedEvent>(OnStockpileUpdatedEvent);

            var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 400, ScrollModes.Vertical);
            this.AddControls(box);
        }

        private void OnStockpileUpdatedEvent(StockpileUpdatedEvent e)
        {
            if (e.Stockpile == this.Stockpile)
                this.ListCollapsible.Invalidate(true);
        }
    }
}
