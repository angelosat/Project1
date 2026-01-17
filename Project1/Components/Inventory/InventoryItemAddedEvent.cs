namespace Start_a_Town_
{
    public record struct InventoryItemAddedEvent(Actor Actor, Entity Item) : IEventPayload { }
    public record struct InventoryItemRemovedEvent(Actor Actor, Entity Item) : IEventPayload { }
}
