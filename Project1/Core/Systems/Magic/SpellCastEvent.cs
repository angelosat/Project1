using Project1.Core.Entities;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Magic;

internal record struct SpellCastEvent(Entity Caster, InteractionTarget Target, SpellDef Spell) : IEventPayload;