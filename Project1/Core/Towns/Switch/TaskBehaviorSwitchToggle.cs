using System.Collections.Generic;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.Designations;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;
using System;

namespace Project1.Core.Towns.Switch
{
    class TaskBehaviorSwitchToggle : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(TargetIndex.A, DesignationDefOf.Switch);
            yield return new BehaviorResolvePath(TargetIndex.A);
            throw new NotImplementedException();
            //yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionFlipSwitch());
        }
    }
}
