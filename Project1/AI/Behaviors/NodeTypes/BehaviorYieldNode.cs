using System;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    abstract class BehaviorYield : Behavior
    {
        IEnumerator<Behavior> Steps;
        int CurrentStepIndex;
        protected BehaviorYield()
        {

        }
        protected virtual IEnumerable<Behavior> GetSteps() { return null; }
        protected virtual IEnumerable<Behavior> GetCurrentStep(Actor parent, AIState state) { return null; }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if(this.Steps == null)
            {
                this.Steps = this.GetCurrentStep(parent, state).GetEnumerator();
                this.Steps.MoveNext();
                this.CurrentStepIndex++;
            }
            var current = this.Steps.Current;
            if (current != null)
            {
                var result = current.Tick(parent, state);
                switch (result)
                {
                    case BehaviorState.Running:
                        return BehaviorState.Running;

                    case BehaviorState.Success:
                        var hasNext = this.Steps.MoveNext();
                        this.CurrentStepIndex++;
                        if (!hasNext)
                            return BehaviorState.Success;
                        return BehaviorState.Running;

                    case BehaviorState.Fail:
                        return BehaviorState.Fail;
                }
            }
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        
        public override void Write(IDataWriter w)
        {
            base.Write(w);
            w.Write(this.CurrentStepIndex);
            this.Steps.Current.Write(w);
        }
        public override void Read(IDataReader r)
        {
            base.Read(r);
            this.CurrentStepIndex = r.ReadInt32();
            
            if(this.Steps == null)
            {
                this.Steps = this.GetSteps().GetEnumerator();
                this.Steps.MoveNext();
                for (int i = 0; i < this.CurrentStepIndex; i++)
                    this.Steps.MoveNext();
            }
            this.Steps.Current.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.CurrentStepIndex.Save("CurrentStep"));
            tag.Add(this.Steps.Current.Save("Behavior"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);

            var currentStep = tag.GetValue<int>("CurrentStep");
            this.CurrentStepIndex = currentStep;
            this.Steps = this.GetSteps().GetEnumerator();
            this.Steps.MoveNext();
            for (int i = 0; i < currentStep; i++)
            {
                this.Steps.MoveNext();
            }
            this.Steps.Current.Load(tag["Behavior"]);
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.Steps.Current.ObjectLoaded(parent);
        }
    }
}
