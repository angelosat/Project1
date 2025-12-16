using System;

namespace Start_a_Town_.AI.Behaviors
{
    static class BehaviorReserve
    { 
        static public Behavior Reserve(BehaviorPerformTask source, TargetIndex targetInd)
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
