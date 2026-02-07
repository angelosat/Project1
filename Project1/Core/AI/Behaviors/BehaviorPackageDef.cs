using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Base;

namespace Project1.Core.AI.Behaviors
{
    public class BehaviorPackageDef(string name, Behavior bhav) : Def(name)
    {
        public Behavior Root = bhav;
    }
}
