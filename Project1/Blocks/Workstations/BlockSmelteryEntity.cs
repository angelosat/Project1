using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockSmelteryEntity : BlockEntity
    {
        public BlockSmelteryEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Smeltery));
            //this.AddComp(new BlockEntityCompRefuelable());
        }
    }
}
