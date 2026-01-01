namespace Start_a_Town_
{
    internal class InteractionStoppedEvent(Actor actor) : IEventPayload
    {
        public readonly Actor Actor = actor;
    }
}
