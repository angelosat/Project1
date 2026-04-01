using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;

namespace Project1.Core.Towns;

sealed class PlannerWithdrawCashOverflow : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsHauling)
            return null;
        var map = actor.Map;
        //var cashRegisters = map.GetBlockEntityComps<BlockShopComp>();
        var candidates = map.GetBlockEntityComps<BlockResourcesComp>();
        foreach(var comp in candidates)
        {
            var cashdef = ResourceDefOf.Cash;
            if (!comp.HasResource(cashdef))
                continue;
            var overflow = comp.GetOverflow(cashdef);
            if (overflow <= 0)
                continue;
            if (!actor.CanReachAndReserve(comp.Parent))
                continue;
            return new Plan(PlanDefOf.Withdraw, new TargetArgs(map, comp.Parent.OriginGlobal));
        }
        return null;
    }
}
