namespace Start_a_Town_
{
    internal class InhabitantPlacedInWorldEvent(Actor actor, WorldSpacePosition pos) : IEventPayload
    {
        public readonly Actor Actor = actor;
        public readonly WorldSpacePosition WorldPosition = pos;
    }
}
