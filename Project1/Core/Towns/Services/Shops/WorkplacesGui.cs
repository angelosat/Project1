using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Towns.Services.Shops;

class WorkplacesGui : GroupBox
{
    public WorkplacesGui()
    {
        var boxList = new GroupBox();
        var town = Ingame.MainViewportMap.Town;
        var shopUI = new Lazy<(Control control, Action<Workplace> refresh)>(Workplace.CreateUI);
        var win = new Lazy<Window>(() => shopUI.Value.control.ToWindow("Shop"));

        var shoplist = new TableCompact<Workplace>()
            .AddColumn(new(), "name", 200, sh => new Label(() => sh.Name, () =>
            {
                shopUI.Value.refresh(sh);
            }), 0)
            .AddColumn(new(), "delete", Icon.Cross.SourceRect.Width,
                w => IconButton.CreateSmall(Icon.Cross,
                    () => MessageBox.CreateDialogue("Warning!", $"{w.Name} will be deleted. Are you sure?",
                        //() => Packets.SendPlayerDeleteShop(this.Town.Net, this.Town.Net.GetPlayer(), w.ID)
                        () => town.Map.Events.Post(new PlayerDeleteShopEvent(w))
                        )));

        var shoplistcontainer = shoplist.MakeScrollable(-1, 200);

        var btnNew = new Button("New", () => town.Map.Events.Post(new PlayerCreateShopEvent()));
        boxList.AddControlsVertically(shoplistcontainer, btnNew);
        this.AddControlsHorizontally(boxList);
    }
}
