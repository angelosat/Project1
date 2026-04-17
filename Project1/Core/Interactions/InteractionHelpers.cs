using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Crafting;
using Project1.Framework;
using System;

namespace Project1.Core.Interactions
{ 
    public static class InteractionHelpers
    {
        public static void TryDepositCarriedItemInsideBlockOrSpawn(Interaction i)
        {
            var ctx = i.Context;
            var actor = ctx.Actor;
            if (actor.Net.IsClient)
                return;
            var global = ctx.Target.Global;
            var amount = ctx.Count;
            var carried = actor.Hauled as Entity;
            ArgumentNullException.ThrowIfNull(carried);
            if (amount > carried.StackSize)
                throw new Exception();
            if (actor.Map.GetBlockEntity(global)?.TryConsume(carried) ?? false)
                return;
            if (actor.Map.GetBlock(global).TryConsume(actor, carried, global, amount == -1 ? carried.StackSize : amount))
                return;

            var finalItem = carried.Take(amount == -1 ? null : amount);
            actor.Map.Spawn(finalItem, global, actor.Velocity);
        }

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
        public static void TrySwapHauledItem(
           Actor actor,
           Entity target,
           int amount)
        {
            var carried = actor.Hauled;
            ArgumentNullException.ThrowIfNull(carried);
            actor.Map.Spawn(carried, target.Global, target.Velocity);
            actor.Inventory.HaulNew(target, amount);
        }
        public static void DepositResource(Interaction i)
        {
            var ctx = i.Context;
            var actor = ctx.Actor;
            if (actor.Net.IsClient)
                return;
            var global = ctx.Target.Global;
            var amount = ctx.Count;
            var carried = actor.Hauled;
            ArgumentNullException.ThrowIfNull(carried);
            var comp = actor.Map.GetBlockEntity(global).GetComp<BlockResourcesComp>();
            var fuel = CraftingSystem.GetFuelValue(carried) * carried.StackSize;
            comp.ApplyDelta(ResourceDefOf.Fuel, fuel);
            carried.Consume(amount == -1 ? carried.StackSize : amount);
        }
    }
}
