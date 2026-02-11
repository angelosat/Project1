using Project1.Framework.Events;
using Project1.Core.Entities;

namespace Project1.Core.Resources
{
    internal record struct ResourceAdjustedEvent(Entity Owner, ResourceDef Def, float Value) : IEventPayload { }
}