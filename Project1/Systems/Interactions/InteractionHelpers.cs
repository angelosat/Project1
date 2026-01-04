using Start_a_Town_;
using System;

namespace Project1.Systems.Interactions
{
    public static class InteractionHelpers
    {
        /// <summary>
        /// Attempts to place the actor's carried entity using the standard placement policy:
        /// 1) Consume by BlockEntity at target cell
        /// 2) Consume by Block at target cell
        /// 3) Spawn entity into the world at target cell
        ///
        /// This method assumes:
        /// - Actor is hauling an Entity
        /// - Placement is allowed by interaction logic
        ///
        /// This method does NOT:
        /// - Validate interaction intent
        /// - Check cancelability
        /// - Enforce domain rules
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="global"></param>
        /// <param name="amount"></param>bal, int amount = -1)
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
            actor.Map.Spawn(carried, global, actor.Velocity);
        }
    }
}
