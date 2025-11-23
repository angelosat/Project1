using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public abstract class BuildToolWorker
    {
        public abstract IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b);
    }
}
