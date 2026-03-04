using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Interactions
{
    sealed class InteractionSwapCarried : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var a = i.Actor;
            var t = i.Target; 
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
