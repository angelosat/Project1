#nullable enable

using Project1.Core.Blocks;

namespace Project1.Core.Crafting
{
    abstract class CraftingOrderAvailabilityCondition 
    {
        internal abstract string Label { get; }
        internal abstract bool Predicate(BlockWorkstationComp comp);
    }
}
