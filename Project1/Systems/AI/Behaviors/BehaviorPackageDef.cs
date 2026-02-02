using Start_a_Town_.Framework.AI.NodeTypes;

namespace Start_a_Town_
{
    public class BehaviorPackageDef(string name, Behavior bhav) : Def(name)
    {
        public Behavior Root = bhav;
    }
}
