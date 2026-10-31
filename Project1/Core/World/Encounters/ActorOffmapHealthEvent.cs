using Project1.Core.Entities.Actors;
using Project1.Framework.Events;
using System;

namespace Project1.Core.World.Encounters
{
    record struct ActorOffmapHealthEvent(Actor Actor, int Delta) : IEventPayload;
}
