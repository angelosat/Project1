namespace Start_a_Town_
{
    public record struct ItemAddedToInventoryEvent(Actor Actor, Entity Item) : IEventPayload { }
}
