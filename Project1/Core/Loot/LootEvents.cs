using Project1.Framework.Events;
using Project1.Core.Entities;

namespace Project1.Core.Loot
{
    internal record struct LootPopEvent(Entity[] Entities, Entity Source) : IEventPayload { }
}
