using Project1.Framework.Base;
using Project1.Framework.Blocks;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    public class BlockKitchenEntity : BlockEntity
    {
        public BlockKitchenEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Baking, IsWorkstation.Types.PlantProcessing));
            //this.AddComp(new BlockEntityCompDeconstructible());
            this.AddComp(new BlockEntityCompRefuelable());
        }
    }
}
