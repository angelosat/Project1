using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework.UI;

#nullable enable

namespace Project1.Core.Towns.Services.Shops;

internal class Gui_ItemForSale : SelectionBoundControl
{
    readonly CheckBoxFinalNew ChbxForSale;
    readonly LabelNew LblPrice;
    Entity? Item;
    public Gui_ItemForSale()
    {
        this.AutoSize = false;
        this.Width = 300;
        this.Height = 300;
        var manager = Ingame.Net.MainViewport.Map.Town.Shops;
        this.ChbxForSale = new("For Sale", this.MarkForSale, () => Ingame.Net.MainViewport.Map.Town.Shops.IsForSale(this.Item));
        this.ChbxForSale.InvalidateOn(manager.Notifier);

        this.LblPrice = new LabelNew(() => $"Price: {manager.GetPrice(this.Item)?.ToString() ?? "<unassigned>"}");
        this.LblPrice.InvalidateOn(manager.Notifier);

        this.AddControlsVertically(this.ChbxForSale, this.LblPrice);
    }

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is Entity item)
            this.Item = item;
    }

    void MarkForSale()
    {
        //Ingame.Net.MainViewport.Map.Town.Shops.ToggleForSale(this.Item);
        Ingame.Instance.Events.Post(new PlayerItemToggledForSaleEvent([this.Item]));
    }
}
