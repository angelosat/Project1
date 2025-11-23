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

        public AITask Task;
        public TaskGiver Source;
        public TaskGiverResult(AITask task, TaskGiver source)
        {
            this.Task = task;
            this.Source = source;
        }
    }
}
