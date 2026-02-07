using Project1.Core.Base;

namespace Project1.Core
{
    public record struct ConstructionReadyEvent(BlockConstructionComp Source) : IEventPayload { }
    public record struct ConstructionFinishedEvent(BlockConstructionComp Source) : IEventPayload { }
    public record struct ConstructionUpdatedEvent(BlockConstructionComp Source) : IEventPayload { }
}
