using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Interactions
{
    internal record struct InteractionProgressEvent(Actor Actor, int WorkAmount) : IEventPayload { }
    internal record struct InteractionNextSwingSpeedEvent(Actor Actor, float Speed) : IEventPayload { }
    internal record struct InteractionStartedEvent(Actor Actor, InteractionDef InteractionDef, TargetArgs Target) : IEventPayload { }
    internal record struct InteractionStoppedEvent(Actor Actor) : IEventPayload { }
    internal record struct InteractionFinishedEvent(Actor Actor) : IEventPayload { }
}
