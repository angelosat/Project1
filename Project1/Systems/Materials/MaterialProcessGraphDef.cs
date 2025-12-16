namespace Start_a_Town_
{
    public class MaterialProcessGraphDef(params (MaterialProcessDef source, MaterialProcessDef[] targets)[] nodes)
    {
        public (MaterialProcessDef source, MaterialProcessDef[] targets)[] Nodes = nodes;
    }
}
