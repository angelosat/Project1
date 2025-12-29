namespace Start_a_Town_
{
    internal class InteractionStoppedEvent(Actor actor) : EventPayloadBase
    {
        public readonly Actor Actor = actor;
    }
}
