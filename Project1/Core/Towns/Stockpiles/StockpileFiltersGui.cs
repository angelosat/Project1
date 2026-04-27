using Project1.Core.Systems.Crafting;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Stockpiles;
internal sealed class StockpileFiltersGui : SelectionBoundControl
{
    //Action _unsub;
    //ListCollapsibleNewNew ListCollapsible;
    ListCollapsible<Def> ListCollapsible;


    protected internal override void OnBind(ISelectable selectable)
    {
        //if(this._unsub != null)
        //{
        //    this._unsub();
        //    this._unsub = null;
        //}
        if (selectable is not Stockpile stockpile)
            return;
        this.ClearControls();
        var entries = IngredientGroupBuilder.BuildNew(stockpile);
        this.ListCollapsible = new();
        this.ListCollapsible.Build(entries);
        selectable.Map.Events.ListenTo<StockpileUpdatedEvent>(OnStockpileUpdatedEvent);

        var box = new ScrollableBoxNewNewNew(this.ListCollapsible, 200, 400, ScrollModes.Vertical);
        //var box = ScrollableBoxNewNewNew.FromWidth(this.ListCollapsible, 200, 400);
        this.AddControls(box);
    }

    private void OnStockpileUpdatedEvent(StockpileUpdatedEvent e)
    {
        if (e.Stockpile == this.CurrentSelection)
            this.ListCollapsible.Invalidate(true);
    }
}
