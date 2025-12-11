using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorDigging : BehaviorPerformTask
    {
        public const TargetIndex MineInd = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(MineInd, DesignationDefOf.Mine);
            this.FailOnCellStandedOn(MineInd);
            yield return new BehaviorGrabTool();
            var gotomine = new BehaviorGetAtNewNew(MineInd);
            yield return gotomine;
            // TODO: check if another npc is standing on the target block to be digged
            //yield return new BehaviorInteractionNew(MineInd, () => new InteractionMiningDigging());// this.Actor.Map.GetBlockMaterial(this.Task.GetTarget(0).Global).Type.SkillToExtract.GetInteraction());
            yield return new BehaviorInteractionNew(MineInd, () => new InteractionBreakBlock());// this.Actor.Map.GetBlockMaterial(this.Task.GetTarget(0).Global).Type.SkillToExtract.GetInteraction());
            // no need to find next task here, just finish and let taskgiver give next one
        }

        protected override bool InitExtraReservations()
        {
            var global = this.Task.GetTarget(MineInd);
            //return this.Actor.Reserve(this.Task, global, 1);
            return this.Reserve(global, 1);
        }
    }
}
