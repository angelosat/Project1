namespace Start_a_Town_
{
    public record struct EntityKilledEvent(Entity Entity) : IEventPayload { }
}
