using Project1.Core.Towns;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.AI.Labors
{
    static class ExtensionsJobs
    {
        static public void ToggleJob(this Actor actor, JobDef jobDef)
        {
            actor.GetState().ToggleJob(jobDef);
        }
        static public bool HasJob(this Actor actor, JobDef jobDef)
        {
            return jobDef is null ? true : actor.GetState().HasJob(jobDef);
        }
        static public Job GetJob(this Actor actor, JobDef jobDef)
        {
            return actor.GetState().GetJob(jobDef);
        }
        static public IEnumerable<Job> GetJobs(this Actor actor)
        {
            return actor.GetState().GetJobs();
        }
    }
}
