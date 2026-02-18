using Project1.Core.UI;
using System.Collections.Generic;

namespace Project1.Core.Towns.Designations
{
    public record struct PlayerDesignationEvent(DesignationDef Designation, IEnumerable<ISelectable> Targets, bool Removal) : Framework.Events.IEventPayload { }
}
