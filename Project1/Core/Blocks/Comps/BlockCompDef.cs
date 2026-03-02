using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Blocks.Comps
{
    public class BlockCompDef : Def
    {
        readonly Type BlockCompType;
        public BlockCompDef(string name, Type compType) : base(name)
        {
            this.BlockCompType = compType;
        }
        public BlockComp Create() => ActivatorSafe<BlockComp>.CreateInstance(this.BlockCompType);
    }
}
