using Project1.Core.Entities;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Towns.Designations
{
    public record struct PlayerDesignationEvent(DesignationDef Designation, IEnumerable<ISelectable> Targets, bool Removal) : IEventPayload { }
    public record struct PlayerDesignationCellsEvent(DesignationDef Designation, MapId MapId, IntVec3 Begin, IntVec3 End, bool IsRemoval) : IEventPayload { }
    public record struct PlayerDesignationEntitiesEvent(DesignationDef Designation, MapId MapId, IReadOnlyCollection<Entity> Entities, bool IsRemoval) : IEventPayload { }
    public record struct DesignationsChangedEvent(IEnumerable<ISelectable> Targets) : IEventPayload { }
}
