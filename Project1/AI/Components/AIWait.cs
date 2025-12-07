using System;

namespace Start_a_Town_.AI.Behaviors
{
    class AIWait : Behavior
    {
        public override string Name => "Idle";
            
        int TimerNew = 0;
        public AIWait() : this(new TimeSpan(0, 0, 1)) { }
        int BaseWaitTime = 5;
        public AIWait(TimeSpan timer)
        {
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            this.TimerNew--;
            if (this.TimerNew <= 0)
            {
                var composure = parent.GetTrait(TraitDefOf.Composure).Normalized;
                this.TimerNew = (int)((BaseWaitTime + (.5f * BaseWaitTime * composure)) * Ticks.PerSecond);
                return BehaviorState.Success;
            }
            return BehaviorState.Running;
        }
        public override object Clone()
        {
            return new AIWait();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(this.TimerNew);
        }
        public override void Read(IDataReader r)
        {
            base.Read(r);
            this.TimerNew = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.TimerNew.Save("Timer"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValueOrDefault<int>("Timer", out this.TimerNew);
        }
    }
}
