using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class TaskBehaviorRefueling : BehaviorPerformTask
    {
        Vector3 RefualableGlobal => this.Task.GetTarget(DestinationIndex).Global;
        Entity Fuel => this.Task.GetTarget(SourceIndex).Object as Entity;
        public const TargetIndex DestinationIndex = TargetIndex.B, SourceIndex = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            bool failOnInvalidRefuelable()
            {
                return !this.Actor.Map.GetBlockEntity(this.RefualableGlobal)?.GetComp<BlockEntityCompRefuelable>()?.Accepts(this.Fuel) ?? true;
            };
            var extract = BehaviorHelper.ExtractNextTargetAmount(SourceIndex);
            yield return extract;
            yield return new BehaviorGetAtNewNew(SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            //yield return BehaviorHelper.StartCarrying(SourceIndex, SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            yield return BehaviorHaulHelper.StartCarrying(this, SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            yield return BehaviorHelper.JumpIfMoreTargets(extract, SourceIndex);
            //yield return new BehaviorGetAtNewNew(DestinationIndex).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
            yield return new BehaviorGetAtNewNew(DestinationIndex, PathEndMode.InteractionSpot).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
            yield return new BehaviorInteractionNew(DestinationIndex,  () => new UseHauledOnTarget()).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
        }
        protected override bool InitExtraReservations()
        {
            return
                this.ReserveAll(SourceIndex) &&
                this.Reserve(DestinationIndex);// &&
        }
    }
}
