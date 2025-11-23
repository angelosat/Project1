using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class BuildToolWorkerRoof : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 begin, IntVec3 end)
        {
            VectorHelper.GetMinMaxVector3(begin, end, out var min, out var max);
            var maxh = GetMaxHeight(min, max);
            var axis = GetAxis(min, max);
            var h = Math.Min(maxh, max.Z - min.Z) + 1;
            for (int i = 0; i < h; i++)
            {
                var a = new IntVec3(min.X + i * axis.Y, min.Y + i * axis.X, begin.Z + i);
                var b = new IntVec3(max.X - i * axis.Y, max.Y - i * axis.X, begin.Z + i);
                foreach (var pos in a.GetBox(b))
                    yield return pos;
            }
        }
        protected static int GetMaxHeight(IntVec3 min, IntVec3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var shortestside = Math.Min(dx, dy);
            return (int)(shortestside / 2);
        }
        protected static IntVec3 GetAxis(IntVec3 min, IntVec3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            if (dx < dy)
                return IntVec3.UnitY;
            else return IntVec3.UnitX;
        }
    }
}
