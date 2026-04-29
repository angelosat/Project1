using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.AI.Personality;

record struct PlayerChangeTraitValueEvent(Actor Actor, TraitDef Trait, float Value) : IEventPayload;
record struct TraitValueChangedEvent(Actor Actor, TraitDef Trait, float Value) : IEventPayload;
