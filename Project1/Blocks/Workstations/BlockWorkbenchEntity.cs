namespace Start_a_Town_
{
    public class BlockWorkbenchEntity : BlockEntity
    {
        public BlockWorkbenchEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {
            this.AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Workbench));
        }
    }
}
