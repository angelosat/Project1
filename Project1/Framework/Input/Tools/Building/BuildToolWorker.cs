using Project1.Framework.Math;
using System.Collections.Generic;
namespace Project1.Core.Input.Tools.Building
{
    public abstract class BuildToolWorker
    {
        public abstract IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b);
    }
}
