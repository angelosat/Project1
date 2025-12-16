namespace Start_a_Town_
{
    public class MaterialProcessGraphDef(params (MaterialStageDef source, MaterialStageDef[] targets)[] nodes)
    {
        public (MaterialStageDef source, MaterialStageDef[] targets)[] Nodes = nodes;
    }
}
