using Start_a_Town_.AI;
using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    sealed class BehaviorHandlePlans : Behavior
    {
        static readonly int TimerMax = Ticks.PerSecond / 20;
        Planner CurrentPlanner;
        int Timer = TimerMax;
        readonly Timer IdleTimer = new(TimerMax);

        private void CleanUp(Actor parent)
        {
            this.CleanUp(parent, parent.GetState());
        }
        private void CleanUp(Actor parent, AIState state)
        {
            if (parent.Hauled is not null)
                parent.Interact(new InteractionThrow(true));

            if (parent.GetEquipmentSlot(GearType.Mainhand) is Entity item)
            {
                if (parent.ItemPreferences.IsPreference(item))
                    parent.Interact(new InteractionEquip(), new TargetArgs(item)); // equip() currently toggles gear. if target is currently equipped, it unequips it
                else
                    parent.Interact(new InteractionDropEquipped(GearType.Mainhand));
            }

            parent.Unreserve();

            state.Reset();
            this.CurrentPlanner = null;
        }
        static IEnumerable<Planner> GetPlanners(Actor actor)
        {
            var givers = actor.GetComponent<NeedsComponent>().NeedsNew.Values.Select(n => n.Planner);
            givers = givers.Concat(Planner.EssentialPlanners);
            var jobs = actor.AI.State.GetJobs().Where(j => j.Enabled);
            jobs.OrderBy(j => j.Priority);
            var jobPlanners = jobs.SelectMany(j => j.Def.GetPlanners());

            // replace this when meta-roles are fully implemented
            givers = actor.IsTownMember ? givers.Concat(jobPlanners) : givers.Concat(Planner.VisitorPlanners);

            givers = givers.Append(Planner.Idle);
            return givers;
        }
        Plan FindNewPlan(Actor parent, AIState state)
        {

            var givers = GetPlanners(parent);

            foreach (var giver in givers)
            {
                if (giver == null)
                    continue;
                var giverResult = giver.FindPlan(parent);
                var task = giverResult.Plan;
                if (task == null)
                    continue;
                var bhav = task.CreateBehavior(parent);
                if (!bhav.InitBaseReservations())
                {
                    parent.Unreserve();
                    continue;
                }

                state.Assign(bhav);
                this.CurrentPlanner = giver;
                return task;
            }

            return null;
        }

        bool TryForcePlan(Actor parent, Plan task, AIState state)
        {
            var bhav = task.CreateBehavior(parent);
            if (!bhav.InitBaseReservations())
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

            if (this.CurrentPlanner is not null)
                tag.Add(this.CurrentPlanner.GetType().FullName.Save("CurrentTaskGiver")); ;
        }

        internal void EndCurrentPlan(Actor actor)
        {
            this.CleanUp(actor);
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValueOrDefault("Timer", out this.Timer);
            tag.TryGetTagValue<string>("CurrentTaskGiver", t => this.CurrentPlanner = Activator.CreateInstance(Type.GetType(t)) as Planner);
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
                var plannerEnum = Planner.UrgentPlanners.GetEnumerator();
                while 
                    (
                    plannerEnum.MoveNext() && 
                    plannerEnum.Current.FindPlanNew(parent) is var task && 
                    task is not null
                    )
                    if (state.TryAssign(task))
                        break;
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

                        parent.CancelInteraction();

                        // TODO: unreserve here?
                        parent.Unreserve();
              
                        state.LastBehavior = currentBhav;

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

                //if (this.CurrentPlanner != null && (!state.Behavior?.Task.Def.Idle ?? false)) 
                if (HasIntent && !IsIdle)
                {
                    if (tired)
                    {
                        this.CleanUp(parent, state);
                        return BehaviorState.Fail;
                    }
                    var next = this.CurrentPlanner.FindPlan(parent);

                    if (next.Plan is not null)
                    {
                        var bhav = next.Plan.CreateBehavior(parent);
                        if (bhav.InitBaseReservations())
                        {
                            $"found followup task from same planner {this.CurrentPlanner}".ToConsole();
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
        bool HasIntent => this.CurrentPlanner is not null;
        bool IsIdle => this.Actor.AI.State.Behavior?.Plan.Def.Idle ?? true;
        public override void Write(IDataWriter w)
        {
            w.Write(this.Timer);
        }
    }
}
