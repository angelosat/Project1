using Project1.Core.Base;
using Project1.Core.Blocks;

namespace Project1.Core
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
