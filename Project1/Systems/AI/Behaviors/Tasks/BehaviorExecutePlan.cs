using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    abstract public class BehaviorExecutePlan : Behavior
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
        readonly List<Action> FinishActions = new();
        public Plan Plan;

        List<Behavior> _CachedBehaviors;
        List<Behavior> CachedBehaviors
        {
            get
            {
                if (this._CachedBehaviors is null)
                {
                    this._CachedBehaviors = new List<Behavior>();
                    foreach (var bhav in this.GetSteps())
                    {
                        bhav.Actor = this.Actor;
                        this._CachedBehaviors.Add(bhav);
                    }
                }
                return this._CachedBehaviors;
            }
        }
        //public override string Status => $"{this.CurrentBehavior.Status}";

        Behavior CurrentBehavior => this.CachedBehaviors[this.CurrentStepIndex];
        public BehaviorExecutePlan()
        {

        }
        public (BehaviorState result, Behavior source) TickNew(Actor parent, AIState state)
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
            if (current != null)
            {
                // MOVING THIS TO BEHAVIOR'S EXECUTE FUNCTION
                // MOVING THIS AFTER BEHAVIOR'S EXECUTE FUNCTION because the behavior might update values that will make it not fail, and this should happen before the fail check for this tick
                // why did i actually do this again???
                //var failedorended = current.HasFailedOrEnded();
                //if (failedorended)
                //    parent.Net.Log.Write(current.ToString() + " failed or ended");

                current.PreTick();
                if (current != this.CachedBehaviors[this.CurrentStepIndex]) // if the pretick action caused a jump, return
                    return BehaviorState.Running;
                FromJump = false;

                // IF I CALL THIS HERE
                // when an actor adds an item to his existing carried item stack, the target item gets absorbed to the carried stack and stops existing
                // since the target item no longer exists, calling this here for some reason fails the 'target existing' check
                // WORKCOMPONENT is ticked after AICOMPONENT, so the interaction finishes and changes the game state before the behavior that handles the interaction is called
                // the behavior that handles the interaction doesn't get the chance to return success and advance the parent behavior
                //if (current.HasFailedOrEnded())
                //    return BehaviorState.Fail;
                //var result = failedorended ? BehaviorState.Fail : current.Execute(parent, state);

                var result = current.Tick(parent, state);
                this.Plan.TicksCounter++;
                /// added the success check because interactioncrafting in behaviorcrafting fails even after the interaction successfuly completes because the ingredients are disposed, and it fails on disposed ingredients
                /// move the whole if block inside the switch block below?
                //if (result != BehaviorState.Success && current.HasFailedOrEnded())
                //    return BehaviorState.Fail;

                switch (result)
                {
                    case BehaviorState.Running:
                        FromJump = false;
                        if (current.HasFailedOrEnded())   /// have this here or before the switch block?
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

        
        public override void Write(IDataWriter w)
        {
        }
        public override void Read(IDataReader r)
        {
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
        public bool ReserveBase()
        {
            //if (this.Task.Tool.HasObject)
            //    if (!this.Actor.Reserve(this.Task.Tool, 1))
            //        return false;

            return this.ReserveExtra();
        }

        protected virtual bool ReserveExtra()
        {
            return true;
        }
        public virtual void CleanUp() 
        {
            for (int i = 0; i < this.FinishActions.Count; i++)
                this.FinishActions[i]();
            //this.Actor.Net.Report($"{this} cleaned up");
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
            // TODO: interperet amount by target type:
            // for entities do if -1 then amount = entity.stacksize
            // for intvec3 and blockentities, do amount  = 1
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

        //internal void SyncToClients(IDataWriter w)
        //{
        //    this.Task.SyncToClients(w);
        //}
        //internal void SyncFromServer(NetEndpoint provider, IDataReader r)
        //{
        //    var plan = new Plan();
        //    plan.SyncFromServer(provider, r);
        //    this.Task = plan;
        //}
    }
}
