using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors
{
    class BehaviorIdle : BehaviorSequence
    {
        public BehaviorIdle()
        {
            this.Children = new List<Behavior>()
            {
                new AIWait(),
                new AIWander()
            };
        }
        public override object Clone()
        {
            return new BehaviorIdle();
        }
    }
}
