using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Designations;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors
{
    abstract public class BehaviorExecutePlan : PlanExecutor
    {
        /// <summary>
        /// Attaches a fail condition to the behavior that checks interaction feasibility while the actor is moving toward the target.
        /// Once the interaction is instantiated, the interaction itself performs the authoritative validity checks.
        /// This ensures mid-transit failures are caught early without duplicating the interaction logic.
        /// </summary>
        protected Behavior FailOnNoDesignation(DesignationDef def) => this.FailOn(() => !this.Actor.Town.DesignationManager.IsDesignation(this.Plan.TargetA, def));
        protected Behavior FailOnNoDesignation() => this.FailOn(() => !this.Actor.Map.Town.DesignationManager.IsDesignation(this.Plan.TargetA, this.Plan.Designation));
        protected Behavior FailOnNoConstructionDesignation() => this.FailOn(() => !this.Actor.Map.Town.ConstructionsManager.IsDesignatedConstruction(this.Plan.TargetA.Global));
        protected abstract IEnumerable<Behavior> GetSteps();
        int CurrentStepIndex;
        public bool Finished;
        readonly List<Action> FinishActions = [];
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
        public BehaviorExecutePlan()
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
            if(this.Plan.IsCancelled)
                return BehaviorState.Fail;
            if (this.HasFailedOrEnded())
                return BehaviorState.Fail;
            var current = this.CachedBehaviors[this.CurrentStepIndex];
            if (current is not null)
            {
                current.PreTick();
                if (current != this.CachedBehaviors[this.CurrentStepIndex]) // if the pretick action caused a jump, return
                    return BehaviorState.Running;
                this.FromJump = false;

                var result = current.Tick(parent, state);
                this.Plan.TicksCounter++;

                switch (result)
                {
                    case BehaviorState.Running:
                        FromJump = false;
                        if (current.HasFailedOrEnded())   // have this here or before the switch block?
                            return BehaviorState.Fail;
                        return BehaviorState.Running;

                    case BehaviorState.Success:
                        if(!FromJump) // workaround
                        {
                            NextBehavior();
                            var hasNext = this.CachedBehaviors.Count > this.CurrentStepIndex;

                            if (!hasNext)
                                return BehaviorState.Success;
                        }

                        this.CachedBehaviors[this.CurrentStepIndex].PreInitAction();
                        this.FromJump = false;
                        return BehaviorState.Running;

                    case BehaviorState.Fail:
                        FromJump = false;
                        return BehaviorState.Fail;
                }
            }
            FromJump = false;
            return BehaviorState.Success;
        }

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
            return this.ReserveExtra();
        }
        protected virtual bool ReserveExtra()
        {
            return true;
        }
        public override void CleanUp() 
        {
            for (int i = 0; i < this.FinishActions.Count; i++)
                this.FinishActions[i]();
        }
        internal override void MapLoaded(Actor parent)
        {
            this.Actor = parent;
            this.Plan.MapLoaded(parent);
        }
        bool FromJump = false;
        public void JumpTo(Behavior bhav)
        {
            FromJump = true;
            this.CurrentStepIndex = this.CachedBehaviors.IndexOf(bhav); //because it's increased by one 
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.Actor = parent as Actor;
            this.CurrentBehavior.ObjectLoaded(parent);
        }
        protected void AddFinishAction(Action a)
        {
            this.FinishActions.Add(a);
        }
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
            if (this.Plan.GetTarget(sourceIndex) is InteractionTarget singleTarget && singleTarget != InteractionTarget.Null)
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
        internal bool Reserve(InteractionTarget target, int amount = -1)
        {
            return this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, target, amount);
        }
        internal bool Reserve(IntVec3 global)
        {
            return this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this.Plan, new InteractionTarget(this.Actor.Map, global), 1);
        }

        internal bool ReserveAsManyAsPossible(InteractionTarget item, int desiredAmount)
        {
            return this.Actor.Map.Town.ReservationManager.ReserveAsManyAsPossible(this.Actor, this.Plan, item, desiredAmount);
        }
        internal bool ReserveAsManyAsPossible(TargetIndex index, int desiredAmount)
        {
            return this.Actor.Map.Town.ReservationManager.ReserveAsManyAsPossible(this.Actor, this.Plan, this.Plan.GetTarget(index), desiredAmount);
        }
    }
}
