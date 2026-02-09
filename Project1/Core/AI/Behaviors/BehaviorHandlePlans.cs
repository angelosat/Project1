using System.Collections.Generic;
using System.Linq;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Planners;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Entities;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Framework.Serialization;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors
{
    sealed class BehaviorHandlePlans : Behavior
    {
        static readonly int TimerMax = Ticks.PerSecond / 20;
        int Timer = TimerMax;
        readonly Timer IdleTimer = new(TimerMax);

        private void CleanUp(Actor parent)
        {
            this.CleanUp(parent, parent.GetState());
        }
        private void CleanUp(Actor parent, AIState state)
        {
            // dont drop carried item here, let the last cleanup behavior (idle) handle it?
            //Unequip(parent);

            parent.Unreserve();

            state.Reset();
            //this.CurrentPlanner = null;
            parent.AI.State.CurrentPlanner = null;
        }

        //private static void Unequip(Actor parent)
        //{
        //    //if (parent.Hauled is not null)
        //    //    parent.Interact(new InteractionThrow(true));

        //    if (parent.GetEquipmentSlot(GearTypeDefOf.Mainhand) is Entity item)
        //    {
        //        if (parent.ItemPreferences.IsPreference(item))
        //        {
        //            throw new NotImplementedException();
        //            //parent.Interact(new InteractionEquip(), new TargetArgs(item)); // equip() currently toggles gear. if target is currently equipped, it unequips it
        //        }
        //        else
        //            parent.Interact(new InteractionDropEquipped(GearTypeDefOf.Mainhand));
        //    }
        //}

        static IEnumerable<PlannerDef> GetPlanners(Actor actor)
        {
            var planners = actor.GetComponent<NeedsComponent>().NeedsNew.Values.Select(n => n.Planner);//.OfType<Planner>();
            planners = planners.Concat(Planner.EssentialPlanners);
            var jobs = actor.AI.State.GetJobs().Where(j => j.Enabled);
            jobs = jobs.OrderBy(j => j.Priority);
            var jobPlanners = jobs.SelectMany(j => j.Def.GetPlanners());

            // replace this when meta-roles are fully implemented
            //givers = actor.IsTownMember ? givers.Concat(jobPlanners) : givers.Concat(Planner.VisitorPlanners);
            planners = planners.Concat(jobPlanners);
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
                var task = giverResult.Plan;
                if (task is null)
                    continue;
                var bhav = task.CreateBehavior(parent);
                if (!bhav.ReserveBase())
                {
                    parent.Unreserve();
                    continue;
                }

                state.Assign(bhav);
                //this.CurrentPlanner = task.Continuation == PlannerContinuation.Continue ? planner : null;
                parent.AI.State.CurrentPlanner = task.Continuation == PlannerContinuation.Continue ? planner : null;
                return task;
            }

            return null;
        }

        bool TryForcePlan(Actor parent, Plan task, AIState state)
        {
            var bhav = task.CreateBehavior(parent);
            if (!bhav.ReserveBase())
                return false;
            //state.CurrentTaskBehavior = bhav;
            //state.CurrentTask = task;
            task.IsImmediate = true;
            state.Assign(bhav);
            return true;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Timer.Save("Timer"));

            //if (this.CurrentPlanner is not null)
            //    tag.Save("CurrentPlanner", this.CurrentPlanner);
        }

        internal void EndCurrentPlan(Actor actor)
        {
            this.CleanUp(actor);
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValueOrDefault("Timer", out this.Timer);
            //tag.TryLoadDefOut("CurrentPlanner", out this.CurrentPlanner);
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

            if (state.ForcedTask != null)
            {
                var task = state.ForcedTask;
                state.ForcedTask = null;
                this.CleanUp(parent);
                this.TryForcePlan(parent, task, state);
            }
            else if(!state.Behavior?.Plan.IsUrgent ?? true)
            {
                foreach(var giver in Planner.UrgentPlanners)
                {
                    var task = giver.FindPlanNew(parent);
                    if (task is null)
                        continue;
                    task.IsUrgent = true;
                    state.TryAssign(task);
                    break;
                }
                //var plannerEnum = Planner.UrgentPlanners.GetEnumerator();
                //while 
                //    (
                //    plannerEnum.MoveNext() && 
                //    plannerEnum.Current.FindPlanNew(parent) is var task && 
                //    task is not null
                //    )
                //    if (state.TryAssign(task))
                //        break;
            }

            if (state.Behavior is not null)
            {
                var currentBhav = state.Behavior;
                var (result, source) = currentBhav.TickNew(parent, state);

                if (parent.Resources[ResourceDefOf.Stamina].Value == 0)
                    result = BehaviorState.Fail;

                switch (result)
                {
                    case BehaviorState.Running:
                        return BehaviorState.Success;

                    case BehaviorState.Fail:
                    case BehaviorState.Success:
                        parent.MoveToggle(false);

                        parent.EndInteraction();

                        // TODO: unreserve here?
                        parent.Unreserve();
              
                        //state.LastBehavior = currentBhav;

                        state.NextTask();

                        // ADDED THIS HERE because when immediately getting a new task from the same taskgiver,
                        // the pathfinding behavior saw that the path wasn't null and didn't calculate a new path for the new behavior/targets
                        state.Path = null;

                        if (parent.CurrentInteraction is not null) // added this here because when cleaning up, an unequip interaction might be in progress. and we dont want to interrupt it by starting another task
                            return BehaviorState.Running; // returning running until clean up interaction finishes, otherwise it might get interrupted by the next behaviors, like BehaviorIdle
                        /// OTHER SOLUTION: make a new behavior that cleans up before behaviorhandletask is ticked?

                        // I MOVED THIS FROM HERE SO THAT THE FALLBACK BEHAVIOR, IF ANY, STARTS IN THE NEXT FRAME
                        //this.CleanUp(parent, state);
                        return BehaviorState.Fail;

                    default:
                        break;
                }
            }
            else
            {
                if (parent.CurrentInteraction is not null) // added this here because when cleaning up, an unequip interaction might be in progress. and we dont want to interrupt it by starting another task
                    return BehaviorState.Running; // returning running until clean up interaction finishes, otherwise it might get interrupted by the next behaviors, like BehaviorIdle
                /// OTHER SOLUTION: make a new behavior that cleans up before behaviorhandletask is ticked?
                var stamina = parent.GetResource(ResourceDefOf.Stamina);
                var staminaTaskThreshold = 20;
                var tired = stamina.Value <= staminaTaskThreshold;

                //if (this.CurrentPlanner != null && (!state.Behavior?.Plan.Def.Idle ?? false))
                //if (this.HasIntent && !this.IsIdle)
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
                        if (bhav.ReserveBase())
                        {
                            $"found followup task from same planner {currentPlanner}".ToConsole();
                            state.Assign(bhav);
                            return BehaviorState.Success;
                        }
                        else
                            this.CleanUp(parent, state);
                    }
                    else
                    {
                        this.CleanUp(parent, state);
                        //return BehaviorState.Fail;
                        return BehaviorState.Running; // RETURN RUNNING INSTEAD because cleaning up starts an interaction
                    }
                }

                if (!tired)
                {
                    this.IdleTimer.Tick();
                    if (this.IdleTimer.Fired)
                    {
                        var task = this.FindNewPlan(parent, state); // TODO: needs optimization
                        if (task is not null)
                        {
                            this.IdleTimer.Reset();
                            return BehaviorState.Success;
                        }
                    }
                    //if (this.Timer < TimerMax)
                    //    this.Timer++;
                    //else
                    //{
                    //    this.Timer = 0;
                    //    var task = this.FindNewPlan(parent, state); // TODO: needs optimization
                    //    if (task is not null)
                    //        return BehaviorState.Success;
                    //}
                }
            }
            if (parent.CurrentInteraction is not null) // added this here because when cleaning up, an unequip interaction might be in progress. and we dont want to interrupt it by starting another task
                return BehaviorState.Running; // returning running until clean up interaction finishes, otherwise it might get interrupted by the next behaviors, like BehaviorIdle
            return BehaviorState.Fail;
        }
        //void TickIdleTimer()
        //{
        //    this.Timer++;
        //}
        //bool IdleTimerFired => this.Timer >= TimerMax;
        //bool HasIntent => this.CurrentPlanner is not null;
        bool IsIdle => this.Actor.AI.State.Behavior?.Plan.Def.Idle ?? true;
        public override void Write(IDataWriter w)
        {
            w.Write(this.Timer);
        }
    }
}
