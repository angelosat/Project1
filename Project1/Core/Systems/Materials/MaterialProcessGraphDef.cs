using Project1.Core.Legacy;

namespace Project1.Core.Systems.Materials
{
    public class MaterialProcessGraphDef(string name, params (RefinementPathDef source, RefinementPathDef[] targets)[] nodes) : Def(name)
    {
        public (RefinementPathDef source, RefinementPathDef[] targets)[] Nodes = nodes;
    }
}
