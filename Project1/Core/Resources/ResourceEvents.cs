using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.Resources
{
    internal record struct ResourceDeltaAppliedEvent(Entity Entity, ResourceDef Def, int Delta) : IEventPayload { }
    internal record struct ResourceRateDeltaAppliedEvent(Entity Entity, ResourceDef Def, float Delta) : IEventPayload { }
    //internal record struct ResourceChangedEvent(Entity Entity, IResourceView Resource) : IEventPayload { }
    internal record struct ResourceChangedEvent(Entity Entity, ResourceRuntime Resource) : IEventPayload { }
    internal record struct BlockResourceDeltaAppliedEvent(MapBase Map, IntVec3 Cell, ResourceDef Def, int Delta) : IEventPayload { }
    internal record struct BlockResourceValueSetEvent(MapBase Map, IntVec3 Cell, ResourceDef Def, int Value) : IEventPayload { }
}