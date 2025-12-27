using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorGoConstruct : BehaviorExecutePlan
    {
        public override string Name { get; } = "Finishing Construction";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            var cache = this.Plan.Def.Interaction.CreateCache(this.Actor, this.Plan.GetTarget(index));
            yield return new BehaviorResolvePath(index, PathEndMode.Any).FailOn(() => this.Plan.Def.Interaction.Worker.CanPerform(cache));
            yield return new BehaviorBeginInteraction(index);
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
