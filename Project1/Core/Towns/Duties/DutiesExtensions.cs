using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Towns.Duties
{
    static class DutiesExtensions
    {
        static public void ToggleJob(this Actor actor, DutyDef jobDef)
        {
            actor.AI.State.ToggleJob(jobDef);
        }
        static public bool HasJob(this Actor actor, DutyDef jobDef)
        {
            return jobDef is null ? true : actor.AI.State.HasJob(jobDef);
        }
        static public Duty GetJob(this Actor actor, DutyDef jobDef)
        {
            return actor.AI.State.GetJob(jobDef);
        }
        static public IEnumerable<Duty> GetJobs(this Actor actor)
        {
            return actor.AI.State.GetJobs();
        }
    }
}
