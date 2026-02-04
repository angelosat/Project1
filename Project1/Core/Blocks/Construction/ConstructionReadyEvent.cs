using Project1.Framework.Base;

namespace Start_a_Town_
{
    public record struct ConstructionReadyEvent(BlockConstructionComp Source) : IEventPayload { }
    public record struct ConstructionFinishedEvent(BlockConstructionComp Source) : IEventPayload { }
    public record struct ConstructionUpdatedEvent(BlockConstructionComp Source) : IEventPayload { }
}
