using Start_a_Town_;

namespace Project1.Framework.Inventory
{
    public record struct InventoryItemAddedEvent(Actor Actor, Entity Item) : IEventPayload { }
    public record struct InventoryItemRemovedEvent(Actor Actor, Entity Item) : IEventPayload { }
}
