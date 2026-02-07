using Project1.Core.Entities;
using Project1.Core.Base;

namespace Project1.Core.Components.Plants
{
    internal record struct PlantHarvestableEvent(Entity Entity) : IEventPayload { }
  
}
