using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorQueue : BehaviorComposite
    {
        public BehaviorQueue(params Behavior[] behavs)
        {
            this.Children = new List<Behavior>(behavs);
        }
        public Behavior Current;
        public override string ToString()
        {
            return this.Current != null ? this.Current.ToString() : "<none>";
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            foreach (var child in this.Children)
            {
                var result = child.Execute(parent, state);
                
                if (result != BehaviorState.Fail)
                {
                    this.Current = child;
                    return result;
                }
            }
            this.Current = null;
            return BehaviorState.Fail;
        }
        
        public override object Clone()
        {
            return new BehaviorQueue((from child in this.Children select child.Clone() as Behavior).ToArray());
        }
        
        internal override void MapLoaded(Actor parent)
        {
            foreach (var ch in this.Children)
                ch.MapLoaded(parent);
        }
    }
}
