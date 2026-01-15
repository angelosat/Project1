using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorResolvePath : BehaviorQueue
    {
        public BehaviorResolvePath(TargetIndex targetInd)
            : this((int)targetInd, PathEndMode.Touching)
        {

        }

        public BehaviorResolvePath(TargetArgs target)
            : this(target, PathEndMode.Touching)
        {
        }
        public BehaviorResolvePath(TargetArgs target, PathEndMode mode)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorResolveDoors(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(target, mode, "path")), // TODO: completely fail behavior if no path found
                        new BehaviorFollowPathNewNew()) // TODO: if path is invalidated while following, return to the find path behavior to find a new path
            };
        }
        public BehaviorResolvePath(TargetIndex targetInd, PathEndMode mode)
            :this((int)targetInd, mode)
        {

        }
        public BehaviorResolvePath(PathEndMode mode)
            : this((int)TargetIndex.A, mode)
        {

        }
        public BehaviorResolvePath(int targetInd, PathEndMode mode)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorResolveDoors(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(targetInd, mode, "path")),
                        new BehaviorFollowPathNewNew())
            };
        }
        public BehaviorResolvePath(string target)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorSequence(
                        new BehaviorFindPath(target, "path"),
                        new BehaviorSetPath("path"),
                        new BehaviorStartMoving(),
                        new BehaviorFollowPathNewNew())
            };
        }
        
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            return base.Tick(parent, state);
        }
    }
}
