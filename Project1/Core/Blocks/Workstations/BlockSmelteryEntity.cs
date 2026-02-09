using Project1.Core.Blocks;
using Project1.Framework;

namespace Project1.Core
{
    class BlockSmelteryEntity : BlockEntity
    {
        public BlockSmelteryEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Smeltery));
        }
    }
}
