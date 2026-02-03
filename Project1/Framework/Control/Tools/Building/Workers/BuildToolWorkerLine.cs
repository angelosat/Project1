using Project1.Framework.Base;
using Project1.Framework.Input.Tools.Building;
using Start_a_Town_;
using System;
using System.Collections.Generic;

namespace Project1.Framework.Input.Tools.Building.Workers
{
    class BuildToolWorkerLine : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            IntVec3 axis;
            var end = b;
            var dx = end.X - a.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - a.Y;
            var ady = Math.Abs(dy);
            if (adx > ady)
                axis = IntVec3.UnitX + IntVec3.UnitZ;
            else
                axis = IntVec3.UnitY + IntVec3.UnitZ;

            var bb = a + new IntVec3(dx * axis.X, dy * axis.Y, 0);
            var box = a.GetBox(bb);
            return box;
        }
    }
}
