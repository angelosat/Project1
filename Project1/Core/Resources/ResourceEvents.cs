using Project1.Framework.Events;
using Project1.Core.Entities;

namespace Project1.Core.Resources
{
    internal record struct ResourceModifiedEvent(Entity Entity, ResourceDef Def, float Delta) : IEventPayload { }
}