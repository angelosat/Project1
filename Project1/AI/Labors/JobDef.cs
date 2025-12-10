using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class JobDef : Def
    {
        readonly TaskGiver[] TaskGivers;
        public ToolUseDef ToolUse;
        public Icon Icon => Icon.Replace;

        public JobDef(string name, params TaskGiver[] taskGivers) : base(name)
        {
            this.TaskGivers = taskGivers;
        }
        public IEnumerable<TaskGiver> GetTaskGivers()
        {
            for (int i = 0; i < this.TaskGivers.Length; i++)
            {
                yield return this.TaskGivers[i];
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
