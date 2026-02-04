using Project1.Framework.Entities.Actors;
using Project1.Framework.Pathing;
using Start_a_Town_.AI;
using Start_a_Town_.Framework.AI.NodeTypes;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class BehaviorSetPath : Behavior
    {
        string Path;
        public BehaviorSetPath(string path)
        {
            this.Path = path;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            state.Path = state.Blackboard[this.Path] as Path;
            state.Blackboard.Remove(this.Path);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorSetPath(this.Path);
        }
    }
}
