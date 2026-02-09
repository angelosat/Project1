using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors
{
    public class BehaviorPackageDef(string name, Behavior bhav) : Def(name)
    {
        public Behavior Root = bhav;
    }
}
