using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Interactions
{
    internal record struct PlantChoppedEvent(Actor Actor, TargetArgs Target, int Intensity) : IEventPayload { }
}