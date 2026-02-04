using Project1.Core.World.WorldAreas;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;

namespace Start_a_Town_
{
    internal class InhabitantPlacedInWorldEvent(Actor actor, WorldSpacePosition pos) : IEventPayload
    {
        public readonly Actor Actor = actor;
        public readonly WorldSpacePosition WorldPosition = pos;
    }
}
