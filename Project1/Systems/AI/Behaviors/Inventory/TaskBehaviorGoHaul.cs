using Start_a_Town_.AI.Behaviors;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Start_a_Town_
{
    class TaskBehaviorGoHaul : BehaviorExecutePlan
    {
        public override string Name { get; } = "Picking up item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnTargetDespawned();
            yield return new BehaviorResolvePath(PathEndMode.Any)
                .FailOnPreInteractionCheck(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(this.Plan.TargetA, this.Plan.AmountA);
        }
    }
}
