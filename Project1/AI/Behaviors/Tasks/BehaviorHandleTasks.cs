using Start_a_Town_.AI;
using System;

namespace Start_a_Town_
{
    sealed class BehaviorHandleTasks : Behavior
    {
        static readonly int TimerMax = Ticks.PerSecond;
        int Timer = TimerMax;

        TaskGiver CurrentTaskGiver;

        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (parent.Velocity.Z != 0)
                return BehaviorState.Running;

            if (state.ForcedTask != null)
            {
                var task = state.ForcedTask;
                state.ForcedTask = null;
                this.CleanUp(parent);
                this.TryForceTask(parent, task, state);
            }

            if (state.CurrentTaskBehavior != null)
            {
                var (result, source) = state.CurrentTaskBehavior.TickNew(parent, state);

                if (parent.Resources[ResourceDefOf.Stamina].Value == 0)
                    result = BehaviorState.Fail;

                switch (result)
                {
                    case BehaviorState.Running:
                        return BehaviorState.Success;

                    case BehaviorState.Fail:
                    case BehaviorState.Success:
                        parent.MoveToggle(false);

                        /// LATEST FINDINGS:
                        /// the problem ended up not being that this call was canceling the interaction at the client, 
                        /// but that the interaction wasn't being serialized properly. its state wasnt synced to the client, and was left as unstarted
                        /// which resulted in the the intearction starting again and re-adding its animation to the entity
                        /// after fixing that, the cancelinteraction now seem to work even after a success

                        parent.CancelInteraction(); // (the following is not true anymore, see above comment) THIS CANNOT BE HERE BECAUSE IT WILL CANCEL THE CLIENT ENTITY'S INTERACTION WHILE THE ANIMATION IS ON THE LAST FRAME

                        //if (result == BehaviorState.Fail) // ONLY CANCEL INTERACTION ON FAILURE?
                        //parent.CancelInteraction(); 
                        // DO I ACTUALLY NEED IT? i dont remember why i added this here
                        // i think i was only cancelling the interaction server-side and the problem appeared after sync-cancelling to the clients

                        // TODO: unreserve here?
                        parent.Unreserve();
                        state.LastBehavior = state.CurrentTaskBehavior;
                        state.CurrentTaskBehavior.CleanUp();
                        state.CurrentTaskBehavior = null;

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

                if (this.CurrentTaskGiver != null && !parent.CurrentTask.Def.Idle)
                {
                    if (tired)
                    {
                        this.CleanUp(parent, state);
                        return BehaviorState.Fail;
                    }
                    var next = this.CurrentTaskGiver.FindTask(parent);

                    if (next.Task != null)
                    {
                        var bhav = next.Task.CreateBehavior(parent);
                        if (bhav.InitBaseReservations())
                        {
                            $"found followup task from same taskgiver {this.CurrentTaskGiver}".ToConsole();
                            state.CurrentTaskBehavior = bhav;
                            state.CurrentTask = next.Task;
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
                    if (this.Timer < TimerMax)
                        this.Timer++;
                    else
                    {
                        this.Timer = 0;
                        var task = this.FindNewTaskNew(parent, state); // TODO: needs optimization
                        if (task is not null)
                            return BehaviorState.Success;
                    }
                }
            }
            if (parent.CurrentInteraction is not null) // added this here because when cleaning up, an unequip interaction might be in progress. and we dont want to interrupt it by starting another task
                return BehaviorState.Running; // returning running until clean up interaction finishes, otherwise it might get interrupted by the next behaviors, like BehaviorIdle
            return BehaviorState.Fail;
        }

        bool TryForceTask(Actor parent, AITask task, AIState state)
        {
            var bhav = task.CreateBehavior(parent);
            if (!bhav.InitBaseReservations())
                return false;
            state.CurrentTaskBehavior = bhav;
            state.CurrentTask = task;
            return true;
        }

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
                    parent.Interact(new Equip(), new TargetArgs(item)); // equip() currently toggles gear. if target is currently equipped, it unequips it
                else
                    parent.Interact(new InteractionDropEquipped(GearType.Mainhand));
            }

            parent.Unreserve();

            state.Reset();
            this.CurrentTaskGiver = null;
        }

        AITask FindNewTaskNew(Actor parent, AIState state)
        {
            
            var givers = parent.GetTaskGivers();

            foreach (var giver in givers)
            {
                if (giver == null)
                    continue;
                var giverResult = giver.FindTask(parent);
                var task = giverResult.Task;
                if (task == null)
                    continue;
                var bhav = task.CreateBehavior(parent);
                if (!bhav.InitBaseReservations())
                {
                    parent.Unreserve();
                    continue;
                }

                state.CurrentTaskBehavior = bhav;
                this.CurrentTaskGiver = giver;
                state.CurrentTask = task;
                return task;
            }

            return null;
        }

        public override object Clone()
        {
            return new BehaviorHandleTasks();
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.Timer);
        }
        public override void Read(IDataReader r)
        {
            this.Timer = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Timer.Save("Timer"));

            if (this.CurrentTaskGiver is not null)
                tag.Add(this.CurrentTaskGiver.GetType().FullName.Save("CurrentTaskGiver")); ;
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValueOrDefault("Timer", out this.Timer);
            tag.TryGetTagValue<string>("CurrentTaskGiver", t => this.CurrentTaskGiver = Activator.CreateInstance(Type.GetType(t)) as TaskGiver);
        }
        internal override void MapLoaded(Actor parent)
        {
            this.Actor = parent;
        }
        
        internal void EndCurrentTask(Actor actor)
        {
            this.CleanUp(actor);
        }
    }
}
