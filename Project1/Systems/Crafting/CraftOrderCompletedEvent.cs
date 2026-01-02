namespace Start_a_Town_
{
    public sealed record CraftOrderCompletedEvent(OrderSettings Order, Actor Actor) : IEventPayload { }
}
