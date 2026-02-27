using Project1.Core.Tools;
using System.Collections.Generic;
using Project1.Core.Towns.Duties;

namespace Project1.Core.Legacy
{
    public class ItemToolDef
    {
        public ToolUse Ability;
        public readonly HashSet<DutyDef> AssociatedJobs = new();
        
        public ItemToolDef(ToolUse ability)
        {
            this.Ability = ability;
        }
        public ItemToolDef AssociateJob(params DutyDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }
    }
}
