using System.Collections.Generic;

namespace Start_a_Town_
{
    class BuildToolWorkerBoxFilled : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            return a.GetBox(b);
        }
    }
}
