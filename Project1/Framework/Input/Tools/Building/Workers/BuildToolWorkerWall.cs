using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Input.Tools.Building;
using System.Collections.Generic;

namespace Project1.Core.Input.Tools.Building.Workers
{
    class BuildToolWorkerWall : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            return a.GetBox(b);
        }
    }
}
