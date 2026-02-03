using Project1.Framework.Base;
using Project1.Framework.Blocks;

namespace Start_a_Town_
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
