using Project1.Framework.Base;
using Project1.Framework.Entities;

namespace Project1.Framework.Inventory
{
    public record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
