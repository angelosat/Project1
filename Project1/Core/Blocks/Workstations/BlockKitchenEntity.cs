using Project1.Core.Blocks;
using Project1.Framework;

namespace Project1.Core
{
    public class BlockKitchenEntity : BlockEntity
    {
        public BlockKitchenEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Baking, IsWorkstation.Types.PlantProcessing));
            this.AddComp(new BlockEntityCompRefuelable());
        }
    }
}
