using Project1.Core.Blocks;
using Project1.Framework.Math;

namespace Project1.Core
{
        class BlockCarpentryEntity : BlockEntityWorkstation
        {
            public BlockCarpentryEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
            {
                this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Carpentry));
            }
        }
}
