using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockSmelteryEntity : BlockEntity
    {
        public BlockSmelteryEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Smeltery));
            //this.AddComp(new BlockEntityCompRefuelable());
        }
    }
}
