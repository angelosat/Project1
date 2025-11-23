using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class ToolBuildRoof : ToolBuildPyramid
    {
        public ToolBuildRoof()
        {

        }
        public ToolBuildRoof(Action<Args> callback)
            : base(callback)
        {

        }
    }
}
