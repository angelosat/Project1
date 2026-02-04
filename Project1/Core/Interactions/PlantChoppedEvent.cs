using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;

namespace Project1.Core.Interactions
{
    internal record struct PlantChoppedEvent(Actor Actor, TargetArgs Target, int Intensity) : IEventPayload { }
}