using Project1.Core.Base;
using System;
using System.Collections.Generic;
namespace Project1.Core.Input.Tools.Building
{
    public abstract class BuildToolWorker
    {
        public abstract IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b);
    }
}
