using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
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
            //var panel = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            //var box = this.ListCollapsible.ToScrollableBox(200, 400);
            var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 400, ScrollModes.Vertical);
            //panel.AddControls(box);
            //this.AddControls(panel);
            this.AddControls(box);
        }
    }
}
