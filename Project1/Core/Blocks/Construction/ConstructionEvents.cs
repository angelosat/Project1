using Project1.Framework.Events;

namespace Project1.Core.Blocks
{
    public record struct ConstructionReadyEvent(BlockConstructionComp Source) : IEventPayload { }
    public record struct ConstructionFinishedEvent(BlockConstructionComp Source) : IEventPayload { }
    public record struct ConstructionUpdatedEvent(BlockConstructionComp Source) : IEventPayload { }
}
