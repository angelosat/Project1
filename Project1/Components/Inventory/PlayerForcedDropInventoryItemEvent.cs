namespace Start_a_Town_
{
    public record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
