using System;

namespace Start_a_Town_.AI.Behaviors
{
    static class BehaviorReserve
    { 
        static public Behavior Reserve(TargetIndex targetInd)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (!bhav.Actor.Reserve(bhav.Actor.CurrentTask, bhav.Actor.CurrentTask.GetTarget(targetInd), -1))
                    throw new Exception();
            };
            return bhav;
        }
    }
}
