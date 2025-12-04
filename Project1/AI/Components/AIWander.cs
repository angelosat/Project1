using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class AIWander : Behavior
    {
        Vector3 Direction; 
        int Timer;
        
        const float MaxRange = 2;
        int Seed;

        public override string Name => "Wander";

        public override object Clone() => new AIWander();
        
        public AIWander()
        {
            this.Direction = Vector3.UnitX;
        }

        private void ChooseDirection(GameObject parent, AIState state)
        {
            var rand = parent.Map.Random;
            double radians = rand.NextDouble() * 2 * Math.PI;
            var choice = new Vector3((float)Math.Cos(radians), (float)Math.Sin(radians), 0);
            var dist = Math.Min(Vector3.Distance(parent.Global, state.Leash) / (float)MaxRange, 1);
            var towardsLeash = state.Leash - parent.Global;
            towardsLeash.Z = 0;
            if (towardsLeash != Vector3.Zero)
                towardsLeash.Normalize();
            var dir = choice + dist * (towardsLeash - choice);
            dir.Normalize();
            this.Direction = dir;
        }
        
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var net = parent.Net;

            /// dont handle this here. it isn't this behavior's job. if an interaction is active, the behavior sequence shouldn't arrive here
            /// OR NOT?
            //if (parent.CurrentInteraction is not null) // added this here because when cleaning up, an unequip interaction might be in progress. and we dont want to interrupt it by starting another task
            //    return BehaviorState.Running;

            if (this.Timer == 0)
            {
                Refresh(parent, state);
                if (net is Client)
                    state.InSync = false;

                parent.Direction = this.Direction;
                parent.MoveToggle(true);
                parent.WalkToggle(true);
            }

            Timer--;
            if (Timer > 0)
                return BehaviorState.Running;
            parent.MoveToggle(false);
            return BehaviorState.Success;
        }
        public Behavior Refresh(GameObject parent, AIState state)
        {
            ChooseDirection(parent, state);
            this.Timer = Ticks.PerSecond;
            return this;
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Seed);
            w.Write(this.Timer);
        }
        public override void Read(IDataReader r)
        {
            this.Seed = r.ReadInt32();
            this.Timer = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Timer.Save("Timer"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValue<int>("Timer", out this.Timer);
        }
    }
}
