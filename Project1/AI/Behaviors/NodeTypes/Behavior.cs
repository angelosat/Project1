using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public enum BehaviorState { Running, Success, Fail }
    public abstract class Behavior : ICloneable
    {
        public virtual string Name { get; } = string.Empty;
        public string Label;
        /// <summary>
        /// This action is performed before any end conditions are checked
        /// </summary>
        public Action PreInitAction = () => { };

        readonly List<Func<BehaviorState>> EndConditions = new();
        readonly List<Action> PreTickActions = new();

        public void PreTick()
        {
            for (int i = 0; i < this.PreTickActions.Count; i++)
            {
                this.PreTickActions[i]();
            }
        }

        public void AddEndCondition(Func<BehaviorState> cond)
        {
            this.EndConditions.Add(cond);
        }
        public void AddPreTickAction(Action act)
        {
            this.PreTickActions.Add(act);
        }
        
        public virtual bool HasFailedOrEnded()
        {
            if (!this.EndConditions.Any())
                return false;
            foreach (var cond in this.EndConditions)
            {
                var result = cond();
                if (result == BehaviorState.Success || result == BehaviorState.Fail)
                    return true;
            }
            return false;
        }
        public Behavior FailOn(Func<bool> cond)
        {
            this.AddEndCondition(() =>
            {
                if (cond())
                    return BehaviorState.Fail;
                return BehaviorState.Running;
            });
            return this;
        }
        public Behavior JumpIf(Func<bool> cond, Behavior gotoBhav)
        {
            this.AddPreTickAction(() =>
            {
                if (cond())
                    //this.Actor.CurrentTaskBehavior.JumpTo(gotoBhav);
                    this.Actor.AI.State.Behavior.JumpTo(gotoBhav);

            });
            return this;
        }
        public Actor Actor;

        public abstract BehaviorState Tick(Actor parent, AIState state);

        public virtual void Write(IDataWriter w)
        {
            
        }
        public virtual void Read(IDataReader r)
        {
            
        }

        public abstract object Clone();
        
        internal SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.String, "Type", this.GetType().FullName));
            this.AddSaveData(tag);
            return tag;
        }
        protected virtual void AddSaveData(SaveTag tag) { }
        internal virtual void Load(SaveTag tag)
        {
        }

        internal virtual void ObjectLoaded(GameObject parent)
        {
            
        }
        internal virtual void WriteBlackboard(BinaryWriter w, Dictionary<string, object> blackboard) { }
        internal virtual void ReadBlackboard(BinaryReader r, Dictionary<string, object> blackboard) { }
        internal virtual SaveTag SaveBlackboard(string name, Dictionary<string, object> blackboard) { return null; }
        internal virtual void LoadBlackboard(SaveTag tag, Dictionary<string, object> blackboard) { }

        internal virtual void MapLoaded(Actor parent)
        {
            
        }
    }
}

