using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Framework.Events;

namespace Project1.Core.Loot
{
    internal record struct LootDropEvent(Entity[] Entities, MapBase Map, Vector3 Global, Vector3 Velocity  = default) : IEventPayload { }
}
