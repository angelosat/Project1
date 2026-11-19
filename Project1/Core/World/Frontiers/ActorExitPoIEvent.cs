using Project1.Core.Entities.Actors;
using Project1.Framework.Events;
using System;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Frontiers
{
    internal record struct ActorExitPoIEvent(Actor Actor) : IEventPayload;
}
