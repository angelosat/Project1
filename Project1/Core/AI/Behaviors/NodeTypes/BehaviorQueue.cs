using System.Linq;
using Project1.Core.Entities.Actors;

namespace Project1.Core.AI.Behaviors.NodeTypes
{
    class BehaviorQueue : BehaviorComposite
    {
        public BehaviorQueue(params Behavior[] behavs)
        {
            this.Children = [.. behavs];
        }
        public Behavior Current;
        public override string ToString()
        {
            return this.Current is not null ? this.Current.ToString() : "<none>";
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            foreach (var child in this.Children)
            {
                var result = child.Tick(parent, state);
                
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
            return new BehaviorQueue([.. from child in this.Children select child.Clone() as Behavior]);
        }
        
        internal override void MapLoaded(Actor parent)
        {
            foreach (var ch in this.Children)
                ch.MapLoaded(parent);
        }
    }
}
