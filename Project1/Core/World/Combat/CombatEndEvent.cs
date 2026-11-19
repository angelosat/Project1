using Project1.Framework.Events;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Combat
{
    internal record struct CombatEndEvent(int Id) : IEventPayload { }
}
