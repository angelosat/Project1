using Project1.Core.Towns;
using Project1.Core.Tools;
using System.Collections.Generic;

namespace Project1.Core.Legacy
{
    public class ItemToolDef
    {
        public ToolUse Ability;
        public readonly HashSet<JobDef> AssociatedJobs = new();
        
        public ItemToolDef(ToolUse ability)
        {
            this.Ability = ability;
        }
        public ItemToolDef AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }
    }
}
