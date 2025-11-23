using System.Collections.Generic;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class TaskBehaviorSleepOnGround : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorCustom()
            {
                Mode = BehaviorCustom.Modes.Continuous,
                Init = (a, s) => this.Actor.Interact(new InteractionSleepOnGround()),
                SuccessCondition = a => this.Actor.GetNeed(NeedDef.Energy).Percentage == 1
            };
            yield return new BehaviorCustom() { Init = (a, t) => AIManager.EndInteraction(this.Actor, true) };
        }
    }
}
