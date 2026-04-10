using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Magic;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Healing;

internal record struct HealingRequestUpdatedEvent(ServiceRequest_Spell Request) : IEventPayload;
internal record struct HealingRequestCreatedEvent(Actor Target, SpellDef Spell) : IEventPayload;
