using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSequence : BehaviorComposite
    {
        Behavior Current;
        public override string ToString()
        {
            return "Sequence: " + this.Current != null ? this.Current.ToString() : "<none>";
        }
      
        public int Position;
        public BehaviorSequence(params Behavior[] behavs)
        {
            this.Children = new List<Behavior>(behavs);
            this.Position = 0;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            for (int i = this.Position; i < this.Children.Count; i++)
            {
                var child = this.Children[i];
                this.Position = i;
                var result = child.Execute(parent, state);
                switch (result)
                {
                    case BehaviorState.Fail:
                        this.Position = 0;
                        return result;

                    case BehaviorState.Running:
                        this.Current = child;
                        return result;

                    case BehaviorState.Success:
                        break;
                }
            }
            this.Position = 0;
            return BehaviorState.Success;
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Position);
            base.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Position = r.ReadInt32();
            base.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorSequence((from child in this.Children select child.Clone() as Behavior).ToArray());
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Position.Save("Position"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValue<int>("Position", out this.Position);
        }
    }
}
