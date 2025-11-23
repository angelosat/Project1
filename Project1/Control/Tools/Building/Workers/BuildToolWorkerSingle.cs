using System.Collections.Generic;

namespace Start_a_Town_
{
    class BuildToolWorkerSingle : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            yield return a;
        }
    }
}
