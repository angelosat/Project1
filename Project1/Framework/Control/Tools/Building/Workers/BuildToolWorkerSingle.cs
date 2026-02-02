using Project1.Framework.Input.Tools.Building;
using Start_a_Town_;
using System.Collections.Generic;

namespace Project1.Framework.Input.Tools.Building.Workers
{
    class BuildToolWorkerSingle : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            yield return a;
        }
    }
}
