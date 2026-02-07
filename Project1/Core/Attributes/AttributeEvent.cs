using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Attributes
{
    internal record struct AttributeAdjustedEvent(Actor Owner, AttributeDef Def, float Value) : IEventPayload { }
}