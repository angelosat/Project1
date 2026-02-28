using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Towns.Duties
{
    static class DutiesExtensions
    {
        static public bool HasDuty(this Actor actor, DutyDef dutyDef)
            => actor.Town?.DutiesManager.HasDuty(actor, dutyDef) ?? false;

        static public Duty GetDuty(this Actor actor, DutyDef dutyDef)
            => actor.Town?.DutiesManager.GetDuty(actor, dutyDef);
        
        static public IEnumerable<Duty> GetDuties(this Actor actor)
            => actor.Town?.DutiesManager.GetDuties(actor);
    }
}
