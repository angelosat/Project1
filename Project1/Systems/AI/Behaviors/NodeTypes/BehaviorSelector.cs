using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSelector : BehaviorComposite
    {
        public BehaviorSelector(params Behavior[] behavs)
        {
            this.Children = new List<Behavior>(behavs);
        }
        Behavior Current;
        int Position;
        public override string ToString()
        {
            return this.Current != null ? this.Current.ToString() : "<none>";
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            for (int i = this.Position; i < this.Children.Count; i++)
            {
                var child = this.Children[i];
                this.Position = i;
                var result = child.Tick(parent, state);
                switch (result)
                {
                    case BehaviorState.Fail:
                        break;

                    case BehaviorState.Running:
                        this.Current = child;
                        return result;

                    case BehaviorState.Success:
                        this.Position = 0;
                        return result;
                }
            }
            this.Position = 0;
            return BehaviorState.Fail;
        }
        
        public override void Write(IDataWriter w)
        {
            w.Write(this.Position);
            base.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Position = r.ReadInt32();
            base.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorSelector((from child in this.Children select child.Clone() as Behavior).ToArray());
        }
    }
}
