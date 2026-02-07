using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using System;
using System.Linq;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class BehaviorJumpOnBlock : Behavior
    {
        const float MaxClimbableHeight = .5f;
        const float VelocityFactorForNextStep = 4;
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (!parent.Mobile.CanJump)
                return BehaviorState.Success;

            var v = parent.Velocity;
            if (v.Z != 0)
                return BehaviorState.Success;
            if (parent.Acceleration == 0)
                return BehaviorState.Success;

            var parentGlobal = parent.Global;
            var nextStep = parentGlobal + parent.Velocity * VelocityFactorForNextStep;
            var footprintCorners = parent.GetBoundingBox(nextStep).GetCorners().Where(c => c.Z == parentGlobal.Z);
            foreach(var corner in footprintCorners)
            {
                var cell = parent.Map.GetCell(corner);
                if (cell == null)
                    continue;
                var isNextBlockSolid = cell.IsSolid();
                var isAboveNextBlockSolid = parent.Map.IsSolid(corner.Above());
                float nextBlockHeight = Block.GetBlockHeight(parent.Map, corner);
                var heightDifference = (cell.Z + nextBlockHeight) - parentGlobal.Z;
                if (isNextBlockSolid && 
                    !isAboveNextBlockSolid &&
                    heightDifference > MaxClimbableHeight)
                {
                    parent.Jump();
                    return BehaviorState.Success;
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
