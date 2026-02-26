using Project1.Core.UI;
using Project1.Core.Networking;
using Project1.Framework.UI;
using Project1.Core.Crafting;
using System;

namespace Project1.Core.Towns.Stockpiles
{
    internal class StockpileFiltersGui : GroupBox, ISelectionBound
    {
        Stockpile Stockpile;
        Action _unsub;
        ListCollapsibleNewNew ListCollapsible;
        //public StockpileFiltersGui(Stockpile stockpile)
        //{
        //    this.Stockpile = stockpile;
        //    var entries = IngredientGroupBuilder.Build(stockpile);
        //    this.ListCollapsible = new ListCollapsibleNewNew();
        //    this.ListCollapsible.BuildNew(entries);
        //    Client.Instance.Map.Events.ListenTo<StockpileUpdatedEvent>(OnStockpileUpdatedEvent);

        //    var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 400, ScrollModes.Vertical);
        //    this.AddControls(box);
        //}
        //public StockpileFiltersGui()
        //{
            
        //}
        public ISelectable CurrentSelection { get => this.Stockpile; set => this.Stockpile = value as Stockpile; }

        public void OnBind(ISelectable selectable)
        {
            if(this._unsub != null)
            {
                this._unsub();
                this._unsub = null;
            }
            this.ClearControls();
            var stockpile = selectable as Stockpile;
            var entries = IngredientGroupBuilder.Build(stockpile);
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
