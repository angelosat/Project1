using Microsoft.Xna.Framework;
using Project1.Core.Systems.Presentation;
using Project1.Core.UI;

namespace Project1.Core.Systems.Inventory;

internal sealed class PresentationInventory : IPresentationWorker
{
    public void Register()
    {
        Registry.MapEventHooksClient.Register<InventoryItemAddedEvent>(OnItemGot);
        Registry.MapEventHooksClient.Register<InventoryItemRemovedEvent>(OnItemLost);
    }

    private static void OnItemGot(InventoryItemAddedEvent e)
    {
        var parent = e.Actor;
        var item = e.Item;
        var floating = new FloatingTextEx(parent)
           .AddSegment("Received ", Color.Lime)
           .AddSegment(item.Name, item.GetInfo().GetQualityColor());
        floating.Show();
    }
    private static void OnItemLost(InventoryItemRemovedEvent e)
    {
        var parent = e.Actor;
        var item = e.Item;
        var floating = new FloatingTextEx(parent)
            .AddSegment(item.Name, item.GetInfo().GetQualityColor());
        floating.Show();
    }
}
