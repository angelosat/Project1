using Project1.Core.Entities;
using Project1.Framework.Events;

namespace Project1.Core.Loot
{
    internal record struct LootPopEvent(Entity[] Entities, Entity Source) : IEventPayload { }
}
