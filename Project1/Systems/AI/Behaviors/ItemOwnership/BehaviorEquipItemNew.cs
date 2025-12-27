using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorEquipItemNew : BehaviorExecutePlan
    {
        public override string Name => "Equipping item";
        public BehaviorEquipItemNew()
        {

        }
        //public BehaviorEquipItemNew(AITask task)
        //{
        //    this.Task = task;
        //}
        
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorMoveTo(this.Task.TargetA, 1);
            //yield return new BehaviorInteractionNew(this.Task.TargetA, new InteractionEquip());
            yield return new BehaviorResolvePath(TargetIndex.A);
            //yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionEquip());
            yield return new BehaviorResolveInteraction(InteractionDefOf.Equip);
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(this.Plan.TargetA, 1);
        }
    }
}
