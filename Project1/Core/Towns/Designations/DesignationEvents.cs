using Project1.Core.UI;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Towns.Designations
{
    public record struct PlayerDesignationEvent(DesignationDef Designation, IEnumerable<ISelectable> Targets, bool Removal) : IEventPayload { }
    public record struct DesignationsChangedEvent(IEnumerable<ISelectable> Targets) : IEventPayload { }
}
