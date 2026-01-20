using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class JobDef : Def
    {
        readonly PlannerDef[] Planners;
        public ToolUseDef ToolUse;
        public Icon Icon => Icon.Replace;

        public JobDef(string name, params PlannerDef[] planners) : base(name)
        {
            this.Planners = planners;
        }
        public IEnumerable<Planner> GetPlanners()
        {
            for (int i = 0; i < this.Planners.Length; i++)
            {
                yield return this.Planners[i].Worker;
            }
        }
        public override string ToString()
        {
            return this.Name;
        }

        public JobDef SetTool(ToolUseDef toolUse)
        {
            this.ToolUse = toolUse;
            return this;
        }
    }
}
