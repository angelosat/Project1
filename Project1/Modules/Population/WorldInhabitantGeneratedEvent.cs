namespace Start_a_Town_
{
    internal class WorldInhabitantGeneratedEvent(Actor actor, WorldSpacePosition pos) : EventPayloadBase
    {
        public readonly Actor Actor = actor;
        public readonly WorldSpacePosition WorldPosition = pos;
    }
}
