using Project1.Core.Entities;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Input
{
    internal record struct PlayerSelectionEvent(TargetArgs Single = null, TargetArgs Add = null, List<TargetArgs> Multiple = null) : IEventPayload { }
    internal record struct PlayerSelectionEventNew(SelectionIntent Selection) : IEventPayload { }
    internal record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
    internal record struct PlayerForbidItemsEvent(Entity[] Entities) : IEventPayload { }
    internal record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
