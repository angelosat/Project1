using Project1.Core.Base;
using Project1.Core.Entities;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Input
{
    internal record struct PlayerSelectionEvent(TargetArgs Single = null, TargetArgs Add = null, List<TargetArgs> Multiple = null) : IEventPayload { }
    internal record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
    public record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }

}
