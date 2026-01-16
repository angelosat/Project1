namespace Start_a_Town_
{
    public record struct InventoryItemAddedEvent(Actor Actor, Entity Item) : IEventPayload { }
}
