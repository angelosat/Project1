using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public struct TaskGiverResult
    {
        public readonly static TaskGiverResult Empty = new(null, null);

        public Plan Task;
        public Planner Source;

        public TaskGiverResult(Plan task, Planner source)
        {
            this.Task = task;
            this.Source = source;
        }
    }
}
