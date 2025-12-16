namespace Start_a_Town_
{
    public class MaterialProcessGraphDef(params (MaterialFormDef source, MaterialFormDef[] targets)[] nodes)
    {
        public (MaterialFormDef source, MaterialFormDef[] targets)[] Nodes = nodes;
    }
}
