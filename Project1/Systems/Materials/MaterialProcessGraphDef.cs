namespace Start_a_Town_
{
    public class MaterialProcessGraphDef(string name, params (RefinementPathDef source, RefinementPathDef[] targets)[] nodes) : Def(name)
    {
        public (RefinementPathDef source, RefinementPathDef[] targets)[] Nodes = nodes;
    }
}
