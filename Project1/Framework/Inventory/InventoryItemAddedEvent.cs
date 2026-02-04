using Project1.Framework.Base;
using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;

namespace Project1.Framework.Inventory
{
    public record struct InventoryItemAddedEvent(Actor Actor, Entity Item) : IEventPayload { }
    public record struct InventoryItemRemovedEvent(Actor Actor, Entity Item) : IEventPayload { }
}
