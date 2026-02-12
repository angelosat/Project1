using Project1.Core.Helpers;
using Project1.Core.Input.Building;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Input.Building.Workers
{
    class BuildToolWorkerBoxFilled : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            return a.GetBox(b);
        }
    }
}
