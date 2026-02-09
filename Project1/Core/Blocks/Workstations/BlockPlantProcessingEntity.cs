using Project1.Framework;
using Project1.Core.Legacy;

namespace Project1.Core.Blocks
{
    class BlockPlantProcessingEntity : BlockEntity
    {
        public BlockPlantProcessingEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.PlantProcessing));
        }
    }
}
