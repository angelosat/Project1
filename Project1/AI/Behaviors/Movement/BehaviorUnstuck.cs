using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorUnstuck : Behavior
    {
        int Timer;
        readonly int TimerMax = Ticks.PerSecond;
        Vector3 LastPosition;
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var a = parent.Acceleration;
            if (this.Timer == this.TimerMax)
            {
                var distanceVector = parent.Global.ToRounded() - parent.Global;
                distanceVector.Z = 0;
                var l = distanceVector.Length();
                if (l < .1f)
                {
                    // arrived
                    this.Timer = 0;
                    return BehaviorState.Success;
                }
                var dir = distanceVector;
                dir.Normalize();
                parent.Direction = dir;
                return BehaviorState.Running;
            }
            else if (a > 0 && parent.Global == this.LastPosition)
                    this.Timer++;
            this.LastPosition = parent.Global;
            return BehaviorState.Success;
        }
        
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
