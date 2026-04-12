using Project1.Core.Blocks.Construction;
using Project1.Core.Blocks.Doors;
using Project1.Core.Systems.Quests;
using Project1.Core.Towns.Services;
using Project1.Core.Towns.Services.Shops;
using Project1.Core.Towns.Storage;
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
        internal static readonly BlockCompDef Light = new("Light", typeof(BlockLightComp));
        internal static readonly BlockCompDef Shop = new("Shop", typeof(BlockShopComp));
        internal static readonly BlockCompDef Shelf = new("Shop", typeof(BlockShelfComp));
        internal static readonly BlockCompDef Inventory = new("Inventory", typeof(BlockInventoryComp));
        internal static readonly BlockCompDef Quests = new("Quests", typeof(BlockQuestsComp));

        static BlockCompDefOf()
        {
            Def.Register(typeof(BlockCompDefOf));
        }
    }
}
