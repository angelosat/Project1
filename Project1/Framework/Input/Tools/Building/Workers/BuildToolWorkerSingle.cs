using Project1.Framework.Math;
using System.Collections.Generic;

namespace Project1.Core.Input.Tools.Building.Workers
{
    class BuildToolWorkerSingle : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            yield return a;
        }
    }
}
