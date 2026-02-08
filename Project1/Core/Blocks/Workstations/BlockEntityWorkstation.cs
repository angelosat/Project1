using Project1.Core.Blocks;
using Project1.Framework.Math;

namespace Project1.Core
{
    public abstract class BlockEntityWorkstation : BlockEntity
    {
        protected BlockEntityWorkstation(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
        }
    }
}
