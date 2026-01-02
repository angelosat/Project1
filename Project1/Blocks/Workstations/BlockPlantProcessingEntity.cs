using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockPlantProcessingEntity : BlockEntity
    {
        public BlockPlantProcessingEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.PlantProcessing));
            //this.AddComp(new BlockEntityCompDeconstructible());
        }
    }
}
