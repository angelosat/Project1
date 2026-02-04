using Project1.Framework.Base;
using Project1.Framework.Skills;
using Start_a_Town_;
using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Project1.Core.Towns
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
        public IEnumerable<PlannerDef> GetPlanners()
        {
            foreach (var p in this.Planners) yield return p;
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
