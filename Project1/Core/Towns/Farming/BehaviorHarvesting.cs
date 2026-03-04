using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Plants;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Farming
{
    class BehaviorHarvesting : BehaviorExecutePlan
    {
        public const TargetIndex PlantIndex = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.Touching).FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }

        protected IEnumerable<Behavior> GetStepsOld()
        {
            this.FailOn(() =>
            {
                var target = this.Plan.GetTarget(PlantIndex);
                var plant = target.Object as Plant;
                if (plant == null)
                    return true;
                if (!plant.Exists)
                    return true;
                if (!plant.IsHarvestable)
                    return true;
                return false;
            });
            this.FailOnForbidden(PlantIndex);
            yield return new BehaviorResolvePath(PlantIndex);
            throw new NotImplementedException();
            //yield return new BehaviorResolveInteraction(PlantIndex, () => new InteractionHarvest());
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.GetTarget(PlantIndex));
        }
    }
}
