using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class InteractionSwapCarried : Interaction
    {
        public InteractionSwapCarried()
             : base(
            "SwapCarried",
            .4f
            )
        {
        }
        public override object Clone()
        {
            return new InteractionSwapCarried();
        }
        protected override void Start()
        {
            var a = this.Actor;
            this.Animation = new Animation(AnimationDef.TouchItem);
        }
        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target; 
            var item = t.Object as Entity;
            var global = item.Global;
            var actor = a as Actor;
            var prevCarried = actor.Hauled;
            prevCarried.Slot.Clear();
            prevCarried.Spawn(a.Map, global);
            actor.Inventory.Haul(item);
        }
    }
}
