using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Consumables;

internal class Planner_Consumables : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.Hauled is not Entity carried)
            return null;
        if (carried.Consumable is not ConsumableComp comp)
            return null;
        // TODO :
        // 1) do i want to activate a consumable only if it's one of the actor's item preferences?
        // 2) do i want to activate if it's not but it has a situational score > 0?
        // 3) do i want to activate directly without score, if no same beneficial effect is currently active?
        // let's do 2 for now.
        var score = actor.AI.State.ItemPreferences.GetTotalSituationalScoreFor(carried);
        if (score <= 0)
            return null;
        return new Plan(ConsumableDefOf.PlanActivate) { Continuation = PlanContinuationPolicy.Yield };
    }
}
