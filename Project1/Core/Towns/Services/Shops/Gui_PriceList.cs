using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;

internal class Gui_PriceList : GroupBox
{
    readonly Table<Entity> Table;
    public Gui_PriceList()
    {
        var shops = Ingame.Net.MainViewport.Map.Town.Shops;
        
        this.Table = new Table<Entity>()
                    .AddColumn("item", 256, a => new LabelNew(a))
                    .AddColumn("price", 48, a => new LabelNew(() => shops.GetPrice(a).ToString()))
                    .AddColumn("camera", 16, a => new IconButtonSmall(Icon.Replace, () => CenterCamera(a)) { HoverText = "Center camera" }.ShowOnParentFocus(true))
                    .AddColumn("delete", 16, a => new IconButtonSmall(Icon.Cross, () => Delete(a)) { HoverText = "Delete" }.ShowOnParentFocus(true));
        this.Table.AddItems(shops.GetPriceList().Select(p=>p.entity));
        shops.ItemsForSaleToggled += Shops_ItemsForSaleToggled;

        var scrollbox = ScrollableBoxNewNewNew.FromWidth(this.Table, this.Table.RowWidth, Label.DefaultHeight * 16);
        this.Controls.Add(scrollbox.ToPanelLabeled("Price list"));
    }

    private void Shops_ItemsForSaleToggled((IEnumerable<Entity> added, IEnumerable<Entity> removed) e)
    {
        this.Table.AddItems(e.added);
        this.Table.RemoveItems(e.removed);
    }

    private static void Delete(Entity entity)
    {
        Ingame.Instance.Events.Post(new PlayerItemToggledForSaleEvent([entity]));
    }

    private static void CenterCamera(Entity entity)
    {
        Ingame.Instance.Events.Post(new PlayerSelectionRectangleEvent([entity]));
        Ingame.Net.MainViewport.Camera.CenterOn(entity.Global);
    }
}
