namespace Start_a_Town_
{
    public class BlockWorkbenchEntity : BlockEntity
    {
        public BlockWorkbenchEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            //this.AddComp(new BlockEntityCompWorkstationOld(IsWorkstation.Types.Workbench));
            this.AddComp(new BlockEntityCompWorkstation(WorkstationDefOf.Smeltery));
        }
    }
}
