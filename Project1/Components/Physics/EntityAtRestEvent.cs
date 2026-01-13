namespace Start_a_Town_
{
    public record struct EntityAtRestEvent(Entity Entity, bool AtRest) : IEventPayload { }
}
