using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class BuildToolWorkerBox : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            VectorHelper.GetMinMaxVector3(a, b, out IntVec3 min, out IntVec3 max);
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var dz = max.Z - min.Z;
            if (dx <= 1 || dy <= 1 || dz <= 1)
                return min.GetBox(max);
            else
                return min.GetBox(max).Except((min + IntVec3.One).GetBox(max - IntVec3.One)).ToList();
        }
    }
}
