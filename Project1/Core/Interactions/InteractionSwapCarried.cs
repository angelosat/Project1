using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;

namespace Project1.Core.Interactions
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
