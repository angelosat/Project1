using Project1.Core.AI.Behaviors.NodeTypes;
using System;

namespace Project1.Core.AI.Behaviors.Reserve
{
    static class BehaviorReserve
    { 
        static public Behavior Reserve(BehaviorExecutePlan source, TargetIndex targetInd)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (!source.Reserve(bhav.Actor.CurrentPlan.GetTarget(targetInd), -1))
                    throw new Exception();
            };
            return bhav;
        }
    }
}
