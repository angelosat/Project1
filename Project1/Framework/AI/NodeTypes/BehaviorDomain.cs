using Project1.Framework.AI.Behaviors.Conditionals;
using Project1.Framework.Entities.Actors;
using Start_a_Town_.AI;

namespace Start_a_Town_.Framework.AI.NodeTypes
{
    public class BehaviorDomain : BehaviorDecorator
    {
        BehaviorCondition Condition;
        public BehaviorDomain(BehaviorCondition condition, Behavior child)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public BehaviorDomain(Behavior child, BehaviorCondition condition)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var cond = this.Condition.Tick(parent, state);
            if (cond == BehaviorState.Fail)
                return BehaviorState.Fail;
            else
                return this.Child.Tick(parent, state);
        }
       
        public override object Clone()
        {
            return new BehaviorDomain(this.Condition, this.Child.Clone() as Behavior);
        }
    }
}
