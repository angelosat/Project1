namespace Start_a_Town_
{
    public class BlockWorkbenchEntity : BlockEntity
    {
        public BlockWorkbenchEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            throw new System.Exception();
            //this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Workbench));
            this.AddComp(new BlockWorkstationComp());// WorkstationDefOf.Smeltery));
        }
    }
}
