using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Core.Needs;
using Project1.Framework.Pathing;

namespace Start_a_Town_
{
    class TaskBehaviorSleepingNew : BehaviorExecutePlan
    {
        static public TargetIndex BedIndex = TargetIndex.A;
       
        public override string Name => "Sleeping";
     
        public TaskBehaviorSleepingNew()
        {

        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorResolvePath(PathEndMode.InteractionSpot);
            yield return new BehaviorResolvePath(PathEndMode.InteractionSpot);
            yield return new BehaviorResolveInteraction();
        }
        protected IEnumerable<Behavior> GetStepsOld()
        {
            yield return new BehaviorResolvePath(TargetIndex.B, PathEndMode.Exact);//, 1);
            yield return new BehaviorCustom()
            {
                Mode = BehaviorCustom.Modes.Continuous,
                Init = (a, s) => this.Actor.Interact(new InteractionSleepInBed(), this.Plan.TargetA),
                SuccessCondition = a => IsEnergyFull()
            };
            yield return new BehaviorCustom() { Init = (a, t) => AIManager.EndInteraction(this.Actor, true) };
        }
      
        bool IsEnergyFull()
        {
            var needenergy = this.Actor.GetNeed(NeedDefOf.Energy);
            return needenergy.Percentage == 1;
        }

        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
            return
                //this.Actor.Reserve(this.Task, this.Task.TargetA) &&
                //this.Actor.Reserve(this.Task, this.Task.TargetB)
                this.Reserve(this.Plan.TargetA) &&
                this.Reserve(this.Plan.TargetB)
                ;
        }
    }
}
