namespace Start_a_Town_
{
    public class MaterialProcessGraphDef(string name, params (MaterialFormDef source, MaterialFormDef[] targets)[] nodes) : Def(name)
    {
        public (MaterialFormDef source, MaterialFormDef[] targets)[] Nodes = nodes;
    }
}
