using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors;
using Project1.Core;
using Project1.Core.Entities;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Refueling
{
    class TaskBehaviorRefueling : BehaviorExecutePlan
    {
        Vector3 RefualableGlobal => this.Plan.GetTarget(DestinationIndex).Global;
        Entity Fuel => this.Plan.GetTarget(SourceIndex).Object as Entity;
        public const TargetIndex DestinationIndex = TargetIndex.B, SourceIndex = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            bool failOnInvalidRefuelable()
            {
                return !this.Actor.Map.GetBlockEntity(this.RefualableGlobal)?.GetComp<BlockEntityCompRefuelable>()?.Accepts(this.Fuel) ?? true;
            };
            var extract = BehaviorHelper.ExtractNextTargetAmount(SourceIndex);
            yield return extract;
            yield return new BehaviorResolvePath(SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            yield return BehaviorHaulHelper.StartCarrying(this, SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            yield return BehaviorHelper.JumpIfMoreTargets(extract, SourceIndex);
            yield return new BehaviorResolvePath(DestinationIndex, PathEndMode.InteractionSpot).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
            yield return new BehaviorResolveInteraction(DestinationIndex,  () => new UseHauledOnTarget()).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
        }
        protected override bool ReserveExtra()
        {
            return
                this.ReserveAll(SourceIndex) &&
                this.Reserve(DestinationIndex);// &&
        }
    }
}
