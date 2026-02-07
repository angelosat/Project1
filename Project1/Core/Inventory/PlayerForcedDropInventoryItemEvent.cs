using Project1.Core.Entities;
using Project1.Core.Base;

namespace Project1.Core.Inventory
{
    public record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
