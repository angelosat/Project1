using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Crafting;

internal record Contract(Actor Author, BlockWorkstationComp Workstation, CraftingOrder Order, IEnumerable<Entity> Ingredients)
{
    public bool IsValid => !this.Order.IsDisposed;
}
internal sealed class CraftingCommitment(Actor actor, CraftingOrder order)
{
    internal readonly Actor Actor = actor;
    internal readonly CraftingOrder Order = order;
    internal Entity? Product;
    internal readonly SimulationTick TickCommitted = actor.World.CurrentTick;
}
