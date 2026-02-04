using Project1.Framework.Base;
using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;
using System;

namespace Project1.Framework.Interactions
{ 
    public static class InteractionHelpers
    {
        public static void TryDepositCarriedItemInsideBlockOrSpawn(
           Actor actor,
           IntVec3 global,
           int amount = -1)
        {
            var carried = actor.Hauled as Entity;
            ArgumentNullException.ThrowIfNull(carried);
            if (actor.Map.GetBlockEntity(global)?.TryConsume(carried) ?? false)
                return;
            if (actor.Map.GetBlock(global).TryConsume(actor, carried, global, amount == -1 ? carried.StackSize : amount))
                return;

            var finalItem = carried.Take(amount == -1 ? null : amount);
            actor.Map.Spawn(finalItem, global, actor.Velocity);
        }
    }
}
