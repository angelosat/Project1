using Project1.Core.UI;
using Project1.Core.Networking;
using Project1.Framework.UI;
using Project1.Core.Crafting;
using System;
using Project1.Core.Screens;

namespace Project1.Core.Towns.Stockpiles
{
    internal sealed class StockpileSettingsGui : SelectionBoundControl
    {
        readonly StockpileFiltersGui storage;
        readonly CheckBoxFinalNew forSale;
        Stockpile Stockpile => this.CurrentSelection as Stockpile;
        public StockpileSettingsGui()
        {
            this.storage = new();
            this.forSale = new("For Sale", 
                () => Ingame.Instance.Events.Post(new PlayerModifiedStockpileSettingsEvent(this.Stockpile, !this.Stockpile.ForSale)), 
                () => this.Stockpile?.ForSale ?? false);
            //this.AddControlsVertically(this.storage, this.forSale);
            this.AddControlsVertically(this.forSale, this.storage);
        }
        protected internal override void OnBind(ISelectable selectable)
        {
            if (selectable is not Stockpile stockpile)
                return;
            this.storage.Bind(stockpile);
            this.forSale.Bind(stockpile);
        }
    }
    internal sealed class StockpileFiltersGui : SelectionBoundControl// GroupBox, ISelectionBound
    {
        Action _unsub;
        ListCollapsibleNewNew ListCollapsible;

        protected internal override void OnBind(ISelectable selectable)
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
            if (e.Stockpile == this.CurrentSelection)
                this.ListCollapsible.Invalidate(true);
        }
    }
}
