using Start_a_Town_;

namespace Project1.Core.Interactions
{
    internal record struct PlantChoppedEvent(Actor Actor, TargetArgs Target, int Intensity) : IEventPayload { }
}