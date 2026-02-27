using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Crafting
{
    internal record Contract(Actor Author, BlockWorkstationComp Workstation, CraftingOrder Order, IEnumerable<Entity> Ingredients)
    {
        public bool IsValid => !this.Order.IsDisposed;
    }
}
