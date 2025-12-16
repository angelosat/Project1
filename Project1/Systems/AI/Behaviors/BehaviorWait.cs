using Start_a_Town_.AI;
using System;

namespace Start_a_Town_
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
            state.CurrentTask.TicksWaited++;
            this.TickAction();
            if (this.EndCondition())
                return BehaviorState.Success;
            return BehaviorState.Running;
        }
    }
}
