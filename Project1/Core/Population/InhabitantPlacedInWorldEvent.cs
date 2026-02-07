using Project1.Core.World.WorldAreas;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Population
{
    internal class InhabitantPlacedInWorldEvent(Actor actor, WorldSpacePosition pos) : IEventPayload
    {
        public readonly Actor Actor = actor;
        public readonly WorldSpacePosition WorldPosition = pos;
    }
}
