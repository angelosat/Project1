using System.Collections.Generic;
using Project1.Core.Towns;
using Project1.Core.Entities.Actors;

namespace Project1.Core.AI.Labors
{
    static class ExtensionsJobs
    {
        static public void ToggleJob(this Actor actor, JobDef jobDef)
        {
            actor.AI.State.ToggleJob(jobDef);
        }
        static public bool HasJob(this Actor actor, JobDef jobDef)
        {
            return jobDef is null ? true : actor.AI.State.HasJob(jobDef);
        }
        static public Job GetJob(this Actor actor, JobDef jobDef)
        {
            return actor.AI.State.GetJob(jobDef);
        }
        static public IEnumerable<Job> GetJobs(this Actor actor)
        {
            return actor.AI.State.GetJobs();
        }
    }
}
