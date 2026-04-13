using Project1.Core.Crafting;
using Project1.Core.UI;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Towns.Stockpiles;
internal sealed class StockpileFiltersGui : SelectionBoundControl
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
        if (selectable is not Stockpile stockpile)
            return;
        var entries = IngredientGroupBuilder.Build(stockpile);
        this.ListCollapsible = new ListCollapsibleNewNew();
        this.ListCollapsible.BuildNew(entries);
        selectable.Map.Events.ListenTo<StockpileUpdatedEvent>(OnStockpileUpdatedEvent);

        var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 400, ScrollModes.Vertical);
        this.AddControls(box);
    }

    private void OnStockpileUpdatedEvent(StockpileUpdatedEvent e)
    {
        if (e.Stockpile == this.CurrentSelection)
            this.ListCollapsible.Invalidate(true);
    }
}
