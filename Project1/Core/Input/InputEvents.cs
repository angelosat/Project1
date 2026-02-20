using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Input
{
    public enum SelectionOp { Clear = 0, Add, Remove }
    internal record struct PlayerSelectionSingleEvent(TargetArgs Single = null, TargetArgs Add = null, List<TargetArgs> Multiple = null) : IEventPayload { }
    internal record struct PlayerSelectionEventNew(SelectionIntent Selection) : IEventPayload { }
    internal record struct PlayerSelectionCubeEvent(IntVec3 Begin, IntVec3 End) : IEventPayload { }
    internal record struct PlayerSelectionRectangleEvent(IEnumerable<Entity> Entities, SelectionOp SelectionOp = default) : IEventPayload { }
    internal record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
    internal record struct PlayerForbiddingItemsEvent(IReadOnlyCollection<Entity> Entities) : IEventPayload { }
    internal record struct PlayerTogglingTownMemberEvent(IReadOnlyCollection<Actor> Entities) : IEventPayload { }
    internal record struct PlayerForcedDropInventoryItemEvent(Entity Owner, Entity Item, int Count) : IEventPayload { }
}
