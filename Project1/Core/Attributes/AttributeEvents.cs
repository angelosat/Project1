using Project1.Framework.Events;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Attributes
{
    internal record struct AttributeAdjustedEvent(Actor Owner, AttributeDef Def, float Value) : IEventPayload { }
}