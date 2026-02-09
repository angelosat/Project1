using Project1.Core.Blocks;
using Project1.Framework;

namespace Project1.Core
{
    public class BlockWorkbenchEntity : BlockEntity
    {
        public BlockWorkbenchEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {
            throw new System.Exception();
            //this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Workbench));
            this.AddComp(new BlockWorkstationComp());// WorkstationDefOf.Smeltery));
        }
    }
}
