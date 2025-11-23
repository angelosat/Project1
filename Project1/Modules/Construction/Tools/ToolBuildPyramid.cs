using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class ToolBuildPyramid : ToolBuildWithHeight
    {
        public ToolBuildPyramid()
        {

        }
        public ToolBuildPyramid(Action<Args> callback)
            : base(callback)
        {

        }
      
        protected static int GetMaxHeight(IntVec3 min, IntVec3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var shortestside = Math.Min(dx, dy);
            return (int)(shortestside / 2);
        }
        protected static float GetShortestSideInt(IntVec3 min, IntVec3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            return Math.Min(dx, dy);
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
