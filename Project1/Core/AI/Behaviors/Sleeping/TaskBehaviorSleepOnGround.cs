using System.Collections.Generic;
using Project1.Core.Needs;
using Project1.Core;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors.Sleeping
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
