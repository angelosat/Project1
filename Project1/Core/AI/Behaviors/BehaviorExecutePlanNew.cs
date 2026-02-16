using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors
{
    public abstract class PlanExecutor : Behavior
    {
        public abstract bool CommitReservations();
    }
    public class BehaviorExecutePlanNew : PlanExecutor
    {
        IEnumerable<Behavior> GetSteps()
        {
            var endMode = this.Plan.Def.Interaction.Range switch
            {
                InteractionRange.Touching => PathEndMode.Touching,
                InteractionRange.Exact => PathEndMode.Exact,
                InteractionRange.Any => PathEndMode.Any,
                InteractionRange.InteractionSpot => PathEndMode.InteractionSpot,
                _ => throw new NotImplementedException(),
            };
            yield return new BehaviorResolvePath(endMode).FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        int CurrentStepIndex;
        public bool Finished;
        //readonly List<Action> FinishActions = [];
        //public Plan Plan; //putting this to the base behavior class temporarily to migrate to a new plan executor behavior

        List<Behavior> _CachedBehaviors;
        List<Behavior> CachedBehaviors
        {
            get
            {
                if (this._CachedBehaviors is null)
                {
                    this._CachedBehaviors = [];
                    foreach (var bhav in this.GetSteps())
                    {
                        bhav.Actor = this.Actor;
                        this._CachedBehaviors.Add(bhav);
                    }
                }
                return this._CachedBehaviors;
            }
        }
        
        Behavior CurrentBehavior => this.CachedBehaviors[this.CurrentStepIndex];
        public BehaviorExecutePlanNew()
        {

        }
        public override (BehaviorState result, Behavior source) TickNew(Actor parent, AIState state)
        {
            var current = this.CachedBehaviors[this.CurrentStepIndex];
            var result = this.Tick(parent, state);
            return (result, current);
        }
        public sealed override BehaviorState Tick(Actor parent, AIState state)
        {
            if (this.Plan.IsCancelled)
                return BehaviorState.Fail;
            if (this.HasFailedOrEnded())
                return BehaviorState.Fail;
            var current = this.CachedBehaviors[this.CurrentStepIndex];
            if (current is not null)
            {
                var result = current.Tick(parent, state);
                this.Plan.TicksCounter++;

                switch (result)
                {
                    case BehaviorState.Running:
                        if (current.HasFailedOrEnded() || this.ShouldAbort())   // have this here or before the switch block?
                            return BehaviorState.Fail;
                        return BehaviorState.Running;

                    case BehaviorState.Success:
                        this.NextBehavior();
                        var hasNext = this.CachedBehaviors.Count > this.CurrentStepIndex;

                        if (!hasNext)
                            return BehaviorState.Success;

                        this.CachedBehaviors[this.CurrentStepIndex].PreInitAction();
                        return BehaviorState.Running;

                    case BehaviorState.Fail:
                        return BehaviorState.Fail;
                }
            }
            return BehaviorState.Success;
        }
        protected sealed override bool ShouldAbort()
        {
            if (!this.Plan.IsStillValid())
                return true;
            return this.ShouldAbortCore();
        }
        protected virtual bool ShouldAbortCore() => false;
        private void NextBehavior()
        {
            this.CurrentStepIndex++;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.CurrentStepIndex.Save("CurrentStep"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            var currentStep = tag.GetValue<int>("CurrentStep");
            this.CurrentStepIndex = currentStep;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        public override bool CommitReservations()
        {
            return this.ReserveAll();
            //return this.ReserveExtra();
        }
        //protected virtual bool ReserveExtra()
        //{
        //    return true;
        //}
        //public override void CleanUp()
        //{
        //    for (int i = 0; i < this.FinishActions.Count; i++)
        //        this.FinishActions[i]();
        //}
        internal override void MapLoaded(Actor parent)
        {
            this.Actor = parent;
            this.Plan.MapLoaded(parent);
        }
        public void JumpTo(Behavior bhav)
        {
            this.CurrentStepIndex = this.CachedBehaviors.IndexOf(bhav); //because it's increased by one 
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.Actor = parent as Actor;
            this.CurrentBehavior.ObjectLoaded(parent);
        }
        //protected void AddFinishAction(Action a)
        //{
        //    this.FinishActions.Add(a);
        //}
        internal bool ReserveAll()
        {
            return
                this.ReserveAll(TargetIndex.A) &&
                this.ReserveAll(TargetIndex.B) &&
                this.ReserveAll(TargetIndex.C);
        }
        internal bool ReserveAll(TargetIndex sourceIndex)
        {
            /// TODO: interperet amount by target type:
            /// for entities do if -1 then amount = entity.stacksize
            /// for intvec3 and blockentities, do amount  = 1
            if (this.Plan.GetTarget(sourceIndex) is TargetArgs singleTarget && singleTarget != TargetArgs.Null)
            {
                var amountSpecified = this.Plan.GetAmount(sourceIndex);
                var amountToReserve = singleTarget.Type switch
                {
                    TargetType.Entity => amountSpecified > 0 ? amountSpecified : singleTarget.Object.StackSize,
                    _ => 1
                };
                this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, singleTarget, amountToReserve);
            }
            var targets = this.Plan.GetTargetQueue(sourceIndex);
            var amounts = this.Plan.GetAmountQueue(sourceIndex);
            var count = targets.Count;
            if (count != amounts.Count)
                throw new Exception();
            for (int i = 0; i < count; i++)
            {
                var target = targets[i];
                var amount = amounts[i];
                if (!this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, target, amount))
                    return false;
            }
            return true;
        }
        internal bool Reserve(TargetIndex index)
        {
            return this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, this.Plan.GetTarget(index), this.Plan.GetAmount(index));
        }

        internal bool Reserve(TargetIndex index, int amount)
        {
            return this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, this.Plan.GetTarget(index), amount);
        }
        internal bool Reserve(TargetArgs target, int amount = -1)
        {
            return this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, target, amount);
        }
        internal bool Reserve(IntVec3 global)
        {
            return this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, new TargetArgs(this.Actor.Map, global), 1);
        }

        internal bool ReserveAsManyAsPossible(TargetArgs item, int desiredAmount)
        {
            return this.Actor.Map.Town.ReservationManager.ReserveAsManyAsPossible(this.Actor, this.Plan, item, desiredAmount);
        }
        internal bool ReserveAsManyAsPossible(TargetIndex index, int desiredAmount)
        {
            return this.Actor.Map.Town.ReservationManager.ReserveAsManyAsPossible(this.Actor, this.Plan, this.Plan.GetTarget(index), desiredAmount);
        }
    }
}
