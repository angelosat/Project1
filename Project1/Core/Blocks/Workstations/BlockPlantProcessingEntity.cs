using Project1.Core.Blocks;
using Project1.Framework.Math;

namespace Project1.Core
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
