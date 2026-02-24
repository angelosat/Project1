using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI;

namespace Project1.Core
{
    class BehaviorGoCraftUnfinishedBegin : BehaviorExecutePlanNew
    {
        //readonly static TargetIndex CellTarget = TargetIndex.A;
        //readonly static TargetIndex WorkstationTarget = TargetIndex.B;
        public override string Name { get; } = "CraftingUnfinishedBegin";

        public override bool CommitReservations()
        {
            var map = this.Actor.Map;
            var contract = map.Town.CraftingManagerNew.GetContract(this.Actor);
            var ingredients = contract.Ingredients;
            bool ingredientSuccess = true;
            foreach (var i in ingredients)
                ingredientSuccess &= map.Town.ReservationManager.Reserve(this.Actor, this.Plan, new TargetArgs(i));
            if (ingredientSuccess)
                return base.CommitReservations();
            return ingredientSuccess;
        }
    }
    //class TaskBehaviorGoCraftUnfinishedAdvance : BehaviorExecutePlanNew
    //{
    //    readonly static TargetIndex CellTarget = TargetIndex.A;
    //    readonly static TargetIndex WorkstationTarget = TargetIndex.B;
    //    public override string Name { get; } = "CraftingUnfinishedAdvance";

    //    //public override bool CommitReservations()
    //    //{
    //    //    return this.Reserve(CellTarget) && this.Reserve(WorkstationTarget);
    //    //}
    //}
    class TaskBehaviorGoCraft : BehaviorExecutePlan
    {
        public override string Name { get; } = "Crafting";

        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.InteractionSpot);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
        
    }
}
