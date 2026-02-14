using System;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors
{
    class BehaviorWait : Behavior
    {
        public Func<bool> EndCondition = () => false;
        public Action TickAction = () => { };
        public BehaviorWait()
        {

        }
        public BehaviorWait(Func<bool> endCondition)
        {
            this.EndCondition = endCondition;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override BehaviorState Tick(Actor parent, AIState state)
        {
            state.CurrentPlan.TicksWaited++;
            this.TickAction();
            if (this.EndCondition())
                return BehaviorState.Success;
            return BehaviorState.Running;
        }
    }
}
