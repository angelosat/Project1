using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Systems.Magic;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Services.Spells;

internal record struct HealingRequestUpdatedEvent(ServiceRequest_Spell Request) : IEventPayload;
internal record struct HealingRequestCreatedEvent(Actor Target, SpellDef Spell) : IEventPayload;

internal record struct TownSpellToggledEvent(MapBase Map, SpellDef Spell) : IEventPayload;
internal record struct PlayerTownSpellToggledEvent(MapBase Map, SpellDef Spell) : IEventPayload;

