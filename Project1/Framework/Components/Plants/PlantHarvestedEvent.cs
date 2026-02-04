using Project1.Framework.Base;
using Project1.Framework.Entities;

namespace Project1.Framework.Components.Plants
{
    internal record struct PlantHarvestedEvent(Entity Entity) : IEventPayload { }
  
}
