using Project1.Core.World.WorldAreas;
using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.World
{
    internal record struct InhabitantPlacedInWorldEvent(Actor Actor, WorldSpacePosition WorldPosition) : IEventPayload { }
}
