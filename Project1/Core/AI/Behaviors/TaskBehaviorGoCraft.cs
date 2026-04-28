using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using System;
using System.Collections.Generic;

namespace Project1.Core
{
    class BehaviorCraftUnfinishedAdvance : BehaviorExecutePlanNew
    {
        //public override bool CommitReservations()
        //{
        //    var map = this.Actor.Map;
        //    var plan = this.Plan;
        //    var order = map.Town.Crafting.Get(plan.Order);
        //    var item = order.UnfinishedItem;
        //    ArgumentNullException.ThrowIfNull(item);
        //    if (!map.Town.ReservationManager.Reserve(this.Actor, plan, new InteractionTarget(item)))
        //        return false;
        //    return base.CommitReservations();
        //}
    }
    class BehaviorGoCraftUnfinishedBegin : BehaviorExecutePlanNew
    {
        public override string Name { get; } = "CraftingUnfinishedBegin";

        //public override bool CommitReservations()
        //{
        //    var map = this.Actor.Map;
        //    var contract = map.Town.Crafting.GetContract(this.Actor);
        //    var ingredients = contract.Ingredients;
        //    bool ingredientSuccess = true;
        //    foreach (var i in ingredients)
        //        ingredientSuccess &= map.Town.ReservationManager.Reserve(this.Actor, this.Plan, new InteractionTarget(i));
        //    if (ingredientSuccess)
        //        return base.CommitReservations();
        //    return ingredientSuccess;
        //}
    }
   
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
