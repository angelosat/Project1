using Project1.Core.Entities;
using Project1.Framework.Events;

namespace Project1.Core.Resources
{
    internal record struct ResourceAdjustedEvent(Entity Owner, ResourceDef Def, float Value) : IEventPayload { }
}