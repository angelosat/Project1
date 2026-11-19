using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.World.Combat;

record struct WorldCombatEvent(CombatRuntime Combat, Actor Actor, AttackResult Result) : IEventPayload { }
