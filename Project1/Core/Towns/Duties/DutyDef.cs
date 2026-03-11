using Project1.Core.AI.Planners;
using Project1.Core.Systems.Tools;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Duties
{
    public sealed class DutyDef : Def
    {
        public readonly PlannerDef[] Planners;
        public ToolUseDef ToolUse;
        public Icon Icon => Icon.Replace;

        public DutyDef(string name, params PlannerDef[] planners) : base(name)
        {
            this.Planners = planners;
        }
        //public IEnumerable<PlannerDef> GetPlanners()
        //{
        //    foreach (var p in this.Planners) yield return p;
        //}
        //public override string ToString()
        //{
        //    return this.Name;
        //}

        public DutyDef SetTool(ToolUseDef toolUse)
        {
            this.ToolUse = toolUse;
            return this;
        }
    }
}
