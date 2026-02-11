using System.Collections.Generic;
using Project1.Framework.Events;
using Project1.Core.Entities;

namespace Project1.Core.Input
{
    internal record struct PlayerSelectionEvent(TargetArgs Single = null, TargetArgs Add = null, List<TargetArgs> Multiple = null) : IEventPayload { }
    internal record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
    internal record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
