#nullable enable

using Project1.Core.Blocks;

namespace Project1.Core.Crafting
{
    internal sealed class CraftingOrderModuleReq(int moduleCount) : CraftingOrderAvailabilityCondition
    {
        internal int ModuleCount = moduleCount;
        internal override string Label => $"Modules required: {this.ModuleCount}";

        internal override bool Predicate(BlockWorkstationComp comp)
            => comp.Modules.Count >= this.ModuleCount;
    }
}
