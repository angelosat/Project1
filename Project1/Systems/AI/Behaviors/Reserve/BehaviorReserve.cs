using Start_a_Town_.Framework.AI.NodeTypes;
using System;

namespace Start_a_Town_.AI.Behaviors
{
    static class BehaviorReserve
    { 
        static public Behavior Reserve(BehaviorExecutePlan source, TargetIndex targetInd)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (!source.Reserve(bhav.Actor.CurrentTask.GetTarget(targetInd), -1))
                    throw new Exception();
            };
            return bhav;
        }
    }
}
