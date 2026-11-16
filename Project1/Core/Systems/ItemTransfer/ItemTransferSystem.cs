using Project1.Core.Entities;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.ItemTransfer;

public static class ItemTransferSystem
{
    public static void Loot(Actor looter, Entity item)
    {
        item.SetOwnerNew(looter);
        looter.Inventory.Insert(item);
    }
}
