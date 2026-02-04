using Project1.Framework.Base;

namespace Start_a_Town_
{
    public record struct WorkstationUpdatedEvent(BlockWorkstationComp Comp) : IEventPayload { }
}
