using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Digging.AI
{
    class TaskBehaviorDigging : BehaviorExecutePlan
    {
        public const TargetIndex MineInd = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation();
            this.FailOnCellStandedOn(TargetIndex.A);
            yield return new BehaviorResolvePath(PathEndMode.Touching)
                .FailOnInvalidInteraction(this.Actor, this.Plan);
            // TODO: check if another npc is standing on the target block to be digged
            yield return new BehaviorResolveInteraction();// this.Actor.Map.GetBlockMaterial(this.Task.GetTarget(0).Global).Type.SkillToExtract.GetInteraction());
            // no need to find next task here, just finish and let taskgiver give next one
        }

        protected override bool ReserveExtra()
        {
            var global = this.Plan.GetTarget(MineInd);
            //return this.Actor.Reserve(this.Task, global, 1);
            return this.Reserve(global, 1);
        }
    }
}
