using System;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorCrouch : Behavior
    {
        const float MaxClimbableHeight = .5f;
        const float VelocityFactorForNextStep = 4;
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var v = parent.Velocity;
            if (v.Z != 0)
                return BehaviorState.Success;
            if (parent.Acceleration == 0)
                return BehaviorState.Success;

            var map = parent.Map;
            var parentGlobal = parent.Global;
            if (parent.Mobile.Crouching)
            {
                var currentfootprintCorners = parent.GetBoundingBox(parentGlobal).GetCorners().Where(c => c.Z == parentGlobal.Z);
                if (currentfootprintCorners.All(c => !map.IsSolid(c.Above())))
                {
                    parent.CrouchToggle(false);
                    return BehaviorState.Success;
                }
            }
            else
            {
                var nextStep = parentGlobal + parent.Velocity * VelocityFactorForNextStep;
                var footprintCorners = parent.GetBoundingBox(nextStep).GetCorners().Where(c => c.Z == parentGlobal.Z);
                foreach (var corner in footprintCorners)
                {
                    var cell = parent.Map.GetCell(corner);
                    if (cell == null)
                        continue;
                    var isNextBlockSolid = cell.IsSolid();
                    var isAboveNextBlockSolid = parent.Map.IsSolid(corner.Above());
                    float nextBlockHeight = Block.GetBlockHeight(map, corner);
                    var heightDifference = (cell.Z + nextBlockHeight) - parentGlobal.Z;
                    if (!isNextBlockSolid && isAboveNextBlockSolid)
                    {
                            parent.CrouchToggle(true);
                        return BehaviorState.Success;
                    }
                    else if (!isNextBlockSolid && !isAboveNextBlockSolid)
                    {

                        return BehaviorState.Success;
                    }
                }
            }
            return BehaviorState.Success;
        }
        
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
