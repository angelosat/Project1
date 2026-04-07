using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Plants
{
    internal record struct PlantChoppedEvent(Actor Actor, InteractionTarget Target, int Intensity) : IEventPayload { }
    internal record struct PlantHarvestedEvent(Entity Entity) : IEventPayload { }
    internal record struct PlantHarvestableEvent(Entity Entity) : IEventPayload { }

}