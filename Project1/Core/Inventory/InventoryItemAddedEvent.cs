using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Inventory
{
    public record struct InventoryItemAddedEvent(Actor Actor, Entity Item) : IEventPayload { }
    public record struct InventoryItemRemovedEvent(Actor Actor, Entity Item) : IEventPayload { }
}
