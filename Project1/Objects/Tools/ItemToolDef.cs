using System.Collections.Generic;

namespace Start_a_Town_
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
