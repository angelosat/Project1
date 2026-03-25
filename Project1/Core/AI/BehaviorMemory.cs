using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities.Actors;

namespace Project1.Core.AI
{
    class BehaviorMemory : Behavior
    {
        float Timer { get; set; }
        float Period { get; set; }
        public BehaviorMemory()
        {
            this.Timer = 0;
            this.Period = Ticks.PerSecond;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (this.Timer < Period)
            {
                this.Timer++;
                // return fail so we don't block parent selector
                return BehaviorState.Fail;//.Running;
            }
            this.Timer = 0;
            // return fail so we don't block parent selector
            return BehaviorState.Fail;
        }

        public override object Clone()
        {
            return new BehaviorMemory();
        }
    }
}
