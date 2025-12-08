using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class TaskBehaviorSleepingNew : BehaviorPerformTask
    {
        static public TargetIndex BedIndex = TargetIndex.A;
       
        public override string Name => "Sleeping";
     
        public TaskBehaviorSleepingNew()
        {

        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGetAtNewNew(TargetIndex.B, PathEndMode.Exact);//, 1);
            yield return new BehaviorCustom()
            {
                Mode = BehaviorCustom.Modes.Continuous,
                Init = (a, s) => this.Actor.Interact(new Blocks.Bed.InteractionSleepInBed(), this.Task.TargetA),
                SuccessCondition = a => IsEnergyFull()
            };
            yield return new BehaviorCustom() { Init = (a, t) => AIManager.EndInteraction(this.Actor, true) };
        }
      
        bool IsEnergyFull()
        {
            var needenergy = this.Actor.GetNeed(NeedDefOf.Energy);
            return needenergy.Percentage == 1;
        }

        protected override bool InitExtraReservations()
        {
            return
                this.Actor.Reserve(this.Task.TargetA) &&
                this.Actor.Reserve(this.Task.TargetB)
                ;
        }
    }
}
