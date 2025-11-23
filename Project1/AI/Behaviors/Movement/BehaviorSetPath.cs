using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSetPath : Behavior
    {
        string Path;
        public BehaviorSetPath(string path)
        {
            this.Path = path;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
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
