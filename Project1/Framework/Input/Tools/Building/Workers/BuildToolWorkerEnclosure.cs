using Project1.Core.Helpers;
using Project1.Framework.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Tools.Building.Workers
{
    class BuildToolWorkerEnclosure : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            var box = a.GetBox(b);
            if (Math.Abs(b.X - a.X) > 1 && Math.Abs(b.Y - a.Y) > 1)
            {
                VectorHelper.GetMinMaxVector3(a, b, out a, out b);
                var boxInner = (a + new IntVec3(1, 1, 0)).GetBox(b - new IntVec3(1, 1, 0));
                box = box.Except(boxInner).ToList();
            }
            return box;
        }
    }
}
