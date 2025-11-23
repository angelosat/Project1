using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class BuildToolWorkerPyramid : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 begin, IntVec3 end)
        {
            VectorHelper.GetMinMaxVector3(begin, end, out IntVec3 min, out IntVec3 max);

            var h = Math.Min(GetMaxHeight(min, max), max.Z - min.Z) + 1;
            for (int i = 0; i < h; i++)
            {
                var a = new IntVec3(min.X + i, min.Y + i, begin.Z + i);
                var b = new IntVec3(max.X - i, max.Y - i, begin.Z + i);
                foreach (var pos in a.GetBox(b))
                    yield return pos;
            }
        }
        static int GetMaxHeight(IntVec3 min, IntVec3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var shortestside = Math.Min(dx, dy);
            return (int)(shortestside / 2);
        }
    }
}
