using Project1.Framework.Events;

namespace Project1.Core.Systems.Relationships
{
    internal record struct RelationshipDeltaAppliedEvent(EntityRefId Owner, EntityRefId Target, int Delta) : IEventPayload { }
}
