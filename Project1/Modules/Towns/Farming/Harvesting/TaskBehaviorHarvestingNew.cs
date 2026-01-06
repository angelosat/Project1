using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class TaskBehaviorHarvestingNew : BehaviorExecutePlan
    {
        public const TargetIndex PlantIndex = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
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
            yield return new BehaviorResolveInteraction(PlantIndex, () => new InteractionHarvest());
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.GetTarget(PlantIndex));
        }
    }
}
