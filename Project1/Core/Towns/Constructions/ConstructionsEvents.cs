using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Towns.Constructions
{
    public record struct PlayerCancelledConstructionEvent(List<IntVec3> Targets) : IEventPayload { }

}
