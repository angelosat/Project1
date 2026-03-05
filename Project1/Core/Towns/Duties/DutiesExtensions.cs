using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Towns.Duties
{
    static class DutiesExtensions
    {
        extension(Actor actor)
        {
            public IEnumerable<Duty> ActiveDuties => actor.Town?.DutiesManager.GetDuties(actor) ?? [];
            public Duty GetDuty(DutyDef dutyDef) => actor.Town?.DutiesManager.GetDuty(actor, dutyDef);
            public bool HasDuty(DutyDef dutyDef) => actor.Town?.DutiesManager.HasDuty(actor, dutyDef) ?? false;
        }
    }
}
