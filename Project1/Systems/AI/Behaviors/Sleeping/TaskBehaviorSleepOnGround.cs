using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI;
using Project1.Core.Needs;

namespace Start_a_Town_
{
    class TaskBehaviorSleepOnGround : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolveInteraction();
        }
        protected IEnumerable<Behavior> GetStepsOld()
        {
            yield return new BehaviorCustom()
            {
                Mode = BehaviorCustom.Modes.Continuous,
                Init = (a, s) => this.Actor.Interact(new InteractionSleepOnGround()),
                SuccessCondition = a => this.Actor.GetNeed(NeedDefOf.Energy).Percentage == 1
            };
            yield return new BehaviorCustom() { Init = (a, t) => AIManager.EndInteraction(this.Actor, true) };
        }
    }
}
