using System.Collections.Generic;

namespace Project1.Core.Towns.Designations
{
    public record struct PlayerDesignationEvent(DesignationDef Designation, IEnumerable<TargetArgs> Targets, bool Removal) : Framework.Events.IEventPayload { }
}
