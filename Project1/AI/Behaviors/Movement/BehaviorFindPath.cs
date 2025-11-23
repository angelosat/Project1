using System;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindPath : Behavior
    {
        readonly string TargetName, PathName = "path";
        readonly int TargetInd;
        TargetArgs Target { get => this.Actor.CurrentTask.GetTarget(this.TargetInd); set { } }

        readonly float Range;
        readonly PathEndMode FinishMode = PathEndMode.Touching;
        public BehaviorFindPath(string variableName, string pathName, float range = 0)
        {
            this.TargetName = variableName;
            this.Range = range;
            this.PathName = pathName;
        }
        public BehaviorFindPath(TargetArgs target, PathEndMode mode, string pathname)
        {
            this.Target = target;
            this.PathName = pathname;
            this.FinishMode = mode;
        }
        public BehaviorFindPath(int targetInd, PathEndMode mode, string pathname)
        {
            this.TargetInd = targetInd;
            this.PathName = pathname;
            this.FinishMode = mode;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            // only execute if there's no current path
            if (state.Path != null)
                return BehaviorState.Success;

            this.Actor = parent;

            switch (state.PathFinder.State)
            {
                case PathingSync.States.Stopped:
                    if (!this.Actor.Physics.MidAir) // DONT START PATHING if actor is mid air, because then the starting node will be null
                    {
                        state.PathFinder.Begin(parent, parent.GetCellStandingOn().Above(), this.Target.Global, this.Range);
                        state.PathFinder.WorkMode(this.FinishMode);
                    }
                    return BehaviorState.Running;

                case PathingSync.States.Working:
                    state.PathFinder.WorkMode(this.FinishMode);
                    return BehaviorState.Running;

                case PathingSync.States.Finished:
                    var path = state.PathFinder.GetPath();
                    state.Blackboard[this.PathName] = path;
                    state.Path = path;
                    if (path is not null)
                    {
                        parent.MoveToggle(true);
                        parent.WalkToggle(!state.CurrentTask.Urgent);
                        return BehaviorState.Success;
                    }
                    else
                    {
                        parent.Net.SyncReport($"{parent.Name} failed to find path from {parent.GetCellStandingOn().Above()} to {this.Target.Global}! {state.PathFinder.Ticks}");
                        return BehaviorState.Fail;
                    }

                default:
                    break;
            }
            throw new Exception();
        }

        public override object Clone()
        {
            return new BehaviorFindPath(this.TargetName, this.PathName, this.Range);
        }
    }
}
