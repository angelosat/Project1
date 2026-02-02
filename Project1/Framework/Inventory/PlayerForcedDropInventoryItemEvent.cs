using Start_a_Town_;

namespace Project1.Framework.Inventory
{
    public record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
