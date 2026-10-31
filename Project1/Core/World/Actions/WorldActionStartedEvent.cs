using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.World.Actions;

internal record struct WorldActionStartedEvent(Actor Actor, WorldActionRuntime Action) : IEventPayload { }
