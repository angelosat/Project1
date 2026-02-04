using System;
using System.Collections.Generic;

namespace Project1.Core.Construction.Tools
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
