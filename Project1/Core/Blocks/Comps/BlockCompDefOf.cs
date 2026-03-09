using Project1.Core.Blocks.Construction;
using Project1.Core.Blocks.Doors;
using Project1.Framework;

namespace Project1.Core.Blocks.Comps
{
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
        internal static readonly BlockCompDef Building = new("Building", typeof(BlockBuildingComp));
        internal static readonly BlockCompDef Quality = new("Quality", typeof(BlockQualityComp));
        internal static readonly BlockCompDef Particles = new("Particles", typeof(BlockParticlesComp));
        internal static readonly BlockCompDef Switchable = new("Switchable", typeof(BlockSwitchableComp));

        static BlockCompDefOf()
        {
            Def.Register(typeof(BlockCompDefOf));
        }
    }
}
