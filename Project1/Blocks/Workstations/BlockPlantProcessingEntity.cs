using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockPlantProcessingEntity : BlockEntity
    {
        public BlockPlantProcessingEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.PlantProcessing));
            //this.AddComp(new BlockEntityCompDeconstructible());
        }
    }
}
