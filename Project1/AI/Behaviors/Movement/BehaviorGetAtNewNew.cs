using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorGetAtNewNew : BehaviorQueue
    {
        public BehaviorGetAtNewNew(TargetIndex targetInd)
            : this((int)targetInd, PathEndMode.Touching)
        {

        }
        public BehaviorGetAtNewNew(TargetArgs target)
            : this(target, PathEndMode.Touching)
        {

        }
        public BehaviorGetAtNewNew(TargetArgs target, PathEndMode mode)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorOpenDoor(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(target, mode, "path")), // TODO: completely fail behavior if no path found
                        new BehaviorFollowPathNewNew()) // TODO: if path is invalidated while following, return to the find path behavior to find a new path
            };
        }
        public BehaviorGetAtNewNew(TargetIndex targetInd, PathEndMode mode)
            :this((int)targetInd, mode)
        {

        }
        public BehaviorGetAtNewNew(int targetInd, PathEndMode mode)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorOpenDoor(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(targetInd, mode, "path")),
                        new BehaviorFollowPathNewNew())
            };
        }
        public BehaviorGetAtNewNew(string target)
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
        
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
    }
}
