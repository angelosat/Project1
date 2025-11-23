using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    public class BlockKitchenEntity : BlockEntity
    {
        public BlockKitchenEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Baking, IsWorkstation.Types.PlantProcessing));
            //this.AddComp(new BlockEntityCompDeconstructible());
            this.AddComp(new BlockEntityCompRefuelable());
        }
    }
}
