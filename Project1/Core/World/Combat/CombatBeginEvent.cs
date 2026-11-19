using Project1.Core.Entities.Actors;
using Project1.Framework.Events;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Combat
{
    internal record struct CombatBeginEvent(int Id, Actor Attacker, Actor Defender) : IEventPayload { }
}
