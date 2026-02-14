using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Project1.Core.AI.Behaviors.NodeTypes
{
    public enum BehaviorState { Running, Success, Fail }
    public abstract class Behavior : ICloneable
    {
        public Behavior FailOnInvalidInteraction(Actor actor, Plan plan) //FailOnInvalidInteraction()
        {
            var ctx = plan.Def.Interaction.CreateContext(actor, plan.TargetA, plan.AmountA);
            return this.FailOn(() => !plan.Def.Interaction.Logic.CanPerform(ctx));
        }
        public Plan Plan; //putting this to the base behavior class temporarily to migrate to a new plan executor behavior
        public virtual void CleanUp() { }

        //public virtual string Status => $"{this}";
        public virtual string Name { get; } = string.Empty;
        public string Label;
        /// <summary>
        /// This action is performed before any end conditions are checked
        /// </summary>
        public Action PreInitAction = () => { };

        readonly List<Func<BehaviorState>> EndConditions = [];
        readonly List<Action> PreTickActions = [];

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
        protected virtual bool ShouldAbort() => false;
        protected virtual bool ShouldFinish() => true;
        public virtual bool HasFailedOrEnded()
        {
            if (this.EndConditions.Count == 0)
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
                //if (cond())
                //    return BehaviorState.Fail;
                //return BehaviorState.Running;
                return cond() ? BehaviorState.Fail : BehaviorState.Running;
            });
            return this;
        }
        public Actor Actor;

        public abstract BehaviorState Tick(Actor parent, AIState state);
        public virtual (BehaviorState result, Behavior source) TickNew(Actor parent, AIState state) => throw new NotImplementedException();

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

        internal virtual void AttachTo(Actor owner) => this.Actor = owner;
    
    }
}

