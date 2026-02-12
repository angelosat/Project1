using System;
using Project1.Framework;
using Project1.Core.Blocks.Doors;
using Project1.Framework.Helpers;

namespace Project1.Core.Blocks.Comps
{
    public class BlockCompDef : Def
    {
        readonly Type BlockCompType;
        public BlockCompDef(string name, Type compType) : base(name)
        {
            this.BlockCompType = compType;
        }
        public BlockComp Create() => ActivatorSafe<BlockComp>.CreateInstance(this.BlockCompType);
    }
    [EnsureStaticCtorCall]
    internal static class BlockCompDefOf
    {
        internal static readonly BlockCompDef Bed = new("Bed", typeof(BlockBedComp));
        internal static readonly BlockCompDef Ownership = new("Ownership", typeof(BlockOwnershipComp));
        internal static readonly BlockCompDef Fuel = new("Fuel", typeof(BlockFuelComp));
        internal static readonly BlockCompDef Construction = new("Construction", typeof(BlockConstructionComp));
        internal static readonly BlockCompDef Door = new("Door", typeof(BlockDoorComp));
        internal static readonly BlockCompDef Workstation = new("Workstation", typeof(BlockWorkstationComp));
        internal static readonly BlockCompDef Resources = new("Resources", typeof(BlockResourcesComp));

        static BlockCompDefOf()
        {
            Def.Register(typeof(BlockCompDefOf));
        }
    }
}
