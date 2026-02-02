using Project1.Framework.Blocks;

namespace Start_a_Town_
{
        class BlockCarpentryEntity : BlockEntityWorkstation// BlockEntity
        {
            public BlockCarpentryEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
            {
                this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Carpentry));
                //this.AddComp(new BlockEntityCompDeconstructible());
            }
        }
}
