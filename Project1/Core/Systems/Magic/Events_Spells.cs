using Project1.Core.Entities;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Magic;

internal record struct Events_Spells(Entity Entity, SpellDef Spell) : IEventPayload;