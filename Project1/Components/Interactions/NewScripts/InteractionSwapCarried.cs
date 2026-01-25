using Microsoft.Xna.Framework;
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
        protected void OnStart()
        {
            var a = this.Actor;
            //this.cachedAnimation = new Animation(AnimationDef.TouchItem);
            //this.AnimationDef = AnimationDef.TouchItem;

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
            //prevCarried.Spawn(a.Map, global);
            a.Map.Spawn(prevCarried as Entity, global, Vector3.Zero);
            actor.Inventory.Haul(item);
        }
    }
}
