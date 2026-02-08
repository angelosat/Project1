using Project1.Core.Blocks;
using Project1.Framework.Math;

namespace Project1.Core
{
    partial class BlockPrefab : Block
    {
        class BlockPrefabEntity : BlockEntity
        {
            public BlockPrefabEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
            {

            }
        }
    }
}
