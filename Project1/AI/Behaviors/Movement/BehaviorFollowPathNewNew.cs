using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFollowPathNewNew : Behavior
    {
        float Range;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="range">Stop moving when within this range of the final path step</param>
        public BehaviorFollowPathNewNew(bool urgent = true, float range = .1f)
        {
            this.Range = range;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            state.Leash = parent.Global;
            var path = state.Path;

            // check if path invalidated
            if (path == null)
                return BehaviorState.Fail;

            var next = path.Current;

            // todo: also check if inbetween blocks have changed? keep a list of blocks that the actor will walk upon when generating the path, and check them when handling blockchanged events?
            // todo: reserve path steps???
            if (!PathingSync.IsPathable(parent.Map, next))
                return BehaviorState.Fail;

            /// i check if path valid inside aicomponent by handling blockchanged event
            //if (!path.IsValid(parent))
            //    return BehaviorState.Fail;

            var acc = parent.Acceleration;

            if (acc > 0) //if moving
            {
                if (parent.IsFootprintWithinBlock(next))
                {
                    if (IsLastStep(state)) // is last step, so make sure we are at the destination and not falling, before popping path step and stopping movement
                    {
                        parent.MoveToggle(false);
                        return BehaviorState.Running;
                    }
                    else
                        path.MoveNext();
                }
            }
            else // if not moving, and not at goal, start moving
            {
                if (parent.IsFootprintWithinBlock(next))
                {
                    if (IsLastStep(state))
                    {
                        if (parent.Velocity == Vector3.Zero)
                        {
                            state.Path = null;
                            return BehaviorState.Success;
                        }
                        else
                            return BehaviorState.Running;
                    }
                }
                parent.MoveToggle(true);
                parent.WalkToggle(!parent.CurrentTask.Urgent);
            }
            Vector3 distanceVector = next - parent.Global;
            Vector2 directionNormal = distanceVector.XY();
            directionNormal.Normalize();
            parent.Direction = new Vector3(directionNormal, 0);
            return BehaviorState.Running;
        }

        private static bool IsLastStep(AIState state)
        {
            return state.Path.IsLastStep;
        }
        
        public override object Clone()
        {
            throw new Exception();
        }
    }
}
