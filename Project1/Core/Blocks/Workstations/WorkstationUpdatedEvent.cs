using Project1.Core.Base;

namespace Project1.Core
{
    public record struct WorkstationUpdatedEvent(BlockWorkstationComp Comp) : IEventPayload { }
}
