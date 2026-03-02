using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.Resources
{
    internal record struct ResourceModifiedEvent(Entity Entity, ResourceDef Def, float Delta) : IEventPayload { }
    internal record struct BlockResourceModifiedEvent(MapBase Map, IntVec3 Cell, ResourceDef Def, float Delta) : IEventPayload { }
}