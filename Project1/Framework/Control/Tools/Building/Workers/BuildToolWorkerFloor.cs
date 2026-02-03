using Project1.Framework.Base;
using Project1.Framework.Input.Tools.Building;
using Start_a_Town_;
using System;
using System.Collections.Generic;

namespace Project1.Framework.Input.Tools.Building.Workers
{
    class BuildToolWorkerFloor : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 begin, IntVec3 end)
        {
            var dx = end.X - begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - begin.Y;
            var ady = Math.Abs(dy);
            var bb = begin + new IntVec3(adx, ady, 0);
            return begin.GetBox(bb);
        }
    }
}
