using Project1.Core.Legacy.Storage;
using Project1.Core.Screens;
using Project1.Core.Systems.Crafting;
using Project1.Core.UI;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Towns.Stockpiles;

internal sealed class StockpileSettingsGui : SelectionBoundControl
{
    readonly StockpileFiltersGui storage;
    readonly CheckBoxFinalNew forSale;
    readonly ComboBoxFinal<StoragePriority> priority;

    Stockpile Stockpile => this.CurrentSelection as Stockpile;
    public StockpileSettingsGui()
    {
        this.storage = new();
        this.forSale = new("For Sale",
            toggleForSale,
            () => this.Stockpile?.ForSale ?? false);
        this.priority = new(
            Enum.GetValues<StoragePriority>(), 
            100, 
            s => s.ToString(), 
            setPriority,
            () => this.Stockpile?.Settings.Priority ?? StoragePriority.None);
        this.AddControlsVertically(this.priority, this.forSale, this.storage);
    }
    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Stockpile stockpile)
            return;
        this.storage.Bind(stockpile);
        this.forSale.InvalidateOn(stockpile);
        this.priority.InvalidateOn(stockpile);
    }

    void toggleForSale()
        => Ingame.Instance.Events.Post(new PlayerModifiedStockpileSettingsEvent(this.Stockpile, !this.Stockpile.ForSale, this.Stockpile.Priority));

    void setPriority(StoragePriority priority)
        => Ingame.Instance.Events.Post(new PlayerModifiedStockpileSettingsEvent(this.Stockpile, this.Stockpile.ForSale, priority));
}
