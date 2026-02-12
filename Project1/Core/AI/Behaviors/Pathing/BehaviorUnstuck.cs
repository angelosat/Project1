using System;
using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class BehaviorUnstuck : Behavior
    {
        int Timer;
        readonly int TimerMax = Ticks.PerSecond;
        Vector3 LastPosition;
        public override BehaviorState Tick(Actor parent, AIState state)
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
