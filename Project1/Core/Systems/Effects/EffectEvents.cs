using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Effects
{
    //public record struct EffectEvents(Actor Actor, EffectDef EffectDef, Def Target, int? Budget, int Rate) : IEventPayload { }
    public record struct ActorEffectAppliedEvent(Actor Actor, EntityEffectWrapper Effect) : IEventPayload { }
    public record struct ActorEffectAbortedEvent(Actor Actor, EntityEffectWrapper Effect) : IEventPayload { }
}
