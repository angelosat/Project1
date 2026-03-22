using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Planners;
using Project1.Core.AI.Reservations;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.Towns.Duties;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Behaviors
{
    sealed class BehaviorHandlePlans : Behavior
    {
        static readonly int TimerMax = Ticks.PerSecond / 20;
        int Timer = TimerMax;
        readonly Timer IdleTimer = new(TimerMax);

        private void CleanUp(Actor parent)
        {
            this.CleanUp(parent, parent.AI.State);
        }
        private void CleanUp(Actor parent, AIState state)
        {
            parent.Unreserve();
            state.Reset();
            parent.AI.State.CurrentPlanner = null;
        }
        static IEnumerable<PlannerDef> GetPlanners(Actor actor)
        {
            var planners = actor.GetComponent<NeedsComponent>().NeedsNew.Values.Select(n => n.Planner);
            planners = planners.Concat(Planner.EssentialPlanners);
            //var jobs = actor.Town.DutiesManager.GetDuties(actor);
            var jobs = actor.ActiveDuties;
            jobs = jobs.OrderBy(j => j.Priority);
            var jobPlanners = jobs.SelectMany(j => j.Def.Planners);

            // replace this when meta-roles are fully implemented
            planners = actor.IsTownMember ? planners.Concat(jobPlanners) : planners.Concat(Planner.VisitorPlanners);
            //planners = planners.Concat(jobPlanners);
            planners = planners.Append(PlannerDefOf.Idle);
            return planners;
        }
        Plan FindNewPlan(Actor parent, AIState state)
        {
            var planners = GetPlanners(parent);

            foreach (var planner in planners)
            {
                if (planner is null)
                    continue;
                var giverResult = planner.Worker.FindPlan(parent);
                var plan = giverResult.Plan;
                if (plan is null)
                    continue;
                giverResult.SourceDef = planner;
                var bhav = plan.CreateBehavior(parent);

                if (!bhav.CommitReservations())
                //if (!plan.ReserveAll())
                {
                    parent.Unreserve();
                    continue;
                }
                //var bhav = plan.CreateBehavior(parent);
                state.Assign(bhav, planner);
                $"{this.Actor} assigned {plan} from {planner}".ToConsole();
                //parent.AI.State.CurrentPlanner = plan.Continuation == PlanContinuationPolicy.Continue ? planner : null;
                return plan;
            }

            return null;
        }

        bool TryForcePlan(Actor parent, Plan plan, AIState state)
        {
            //if (!bhav.CommitReservations())
            if (!plan.ReserveAll())
                return false;
            var bhav = plan.CreateBehavior(parent);
            plan.IsImmediate = true;
            throw new NotImplementedException();
            //state.Assign(bhav);
            return true;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Timer.Save("Timer"));
        }
        internal void EndCurrentPlan(Actor actor)
        {
            this.CleanUp(actor);
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValueOrDefault("Timer", out this.Timer);
        }
        internal override void MapLoaded(Actor parent)
        {
            this.Actor = parent;
        }
        public override object Clone()
        {
            return new BehaviorHandlePlans();
        }
        public override void Read(IDataReader r)
        {
            this.Timer = r.ReadInt32();
        }

        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (parent.Velocity.Z != 0)
                return BehaviorState.Running;

            //if (parent.CurrentInteraction is not null) 
                /// added this here because plans returned fromurgent plannets (smart equip in particular)
                /// didn't correctly abort/remove any current interaction, which caused
                /// workcomponent to have a null interaction but behaviorresolveinteraction to store a stale reference of an interaction
                /// specifically, the interaction stayed in a finishing state and was never ticked by the workcomponent in order to switch
                /// from finishing to finished, so behaviorresolveinteraction was stuck looking that the interaction never finished
                /// but urgent planners should be able to interrupt/abort running interactions
                /// maybe only tick urgent planners if current interaction is null or current interaction state is running
                /// maybe dont tick urgent planners if current interaction is not null and its state is finishing, so that it can finish gracefully
                /// maybe when an urrgent plan is returned, dont assign it immediately, but store it as pending (or enqueue it)
                /// and first make the current interaction/bhav abort gracefully before assinging it
                //return BehaviorState.Running;

            if (state.ForcedTask != null)
            {
                var task = state.ForcedTask;
                state.ForcedTask = null;
                this.CleanUp(parent);
                this.TryForcePlan(parent, task, state);
            }
            else if (!state.Behavior?.Plan.IsUrgent ?? true)
            {
                if (parent.CurrentInteraction is not Interaction i || i.State == Interaction.States.Running)
                    foreach (var planner in Planner.UrgentPlanners)
                    {
                        var task = planner.Worker.FindPlanNew(parent);
                        if (task is null)
                            continue;
                        task.IsUrgent = true;
                        state.TryAssign(task, planner);
                        break;
                    }
            }
         
            if (state.Behavior is not null)
            {
                var currentBhav = state.Behavior;
                var (result, source) = currentBhav.TickNew(parent, state);

                if (parent.Resources.View(ResourceDefOf.Stamina).Value == 0)
                    result = BehaviorState.Fail;

                switch (result)
                {
                    case BehaviorState.Running:
                        return BehaviorState.Success;

                    case BehaviorState.Fail:
                    case BehaviorState.Success:
                        parent.MoveToggle(false);
                        //parent.EndInteraction();
                        parent.Unreserve();
                        state.NextTask();
                        state.Path = null;
                        if (parent.CurrentInteraction is not null) 
                            return BehaviorState.Running; 
                        return BehaviorState.Fail;

                    default:
                        break;
                }
            }
            else
            {
               
                if (parent.CurrentInteraction is not null) 
                    return BehaviorState.Running; 

                var stamina = parent.GetResource(ResourceDefOf.Stamina);
                var staminaTaskThreshold = 20;
                var tired = stamina.Value <= staminaTaskThreshold;

                var currentPlanner = parent.AI.State.CurrentPlanner;
                if(currentPlanner is not null && currentPlanner != PlannerDefOf.Idle)
                {
                    if (tired)
                    {
                        this.CleanUp(parent, state);
                        return BehaviorState.Fail;
                    }
                    var next = currentPlanner.Worker.FindPlan(parent);

                    if (next.Plan is not null)
                    {
                        var bhav = next.Plan.CreateBehavior(parent);
                        if (bhav.CommitReservations())
                        {
                            $"found followup {next.Plan} from same planner {currentPlanner}".ToConsole();
                         
                            state.Assign(bhav, currentPlanner);
                            return BehaviorState.Success;
                        }
                        else
                            this.CleanUp(parent, state);
                    }
                    else
                    {
                        this.CleanUp(parent, state);
                        return BehaviorState.Running;
                    }
                }

                if (!tired)
                {
                    this.IdleTimer.Tick();
                    if (this.IdleTimer.Fired)
                    {
                        var task = this.FindNewPlan(parent, state);
                        if (task is not null)
                        {
                            this.IdleTimer.Reset();
                            return BehaviorState.Success;
                        }
                    }
                }
            }
            if (parent.CurrentInteraction is not null)
                return BehaviorState.Running;
            return BehaviorState.Fail;
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.Timer);
        }
    }
}
