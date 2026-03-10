using Project1.Core.Blocks.Comps;
using Project1.Core.Blocks.Construction;
using Project1.Core.Blocks.Doors;
using Project1.Core.Construction;
using Project1.Core.Crafting;
using Project1.Core.Graphics.Particles;
using Project1.Core.Materials;
using Project1.Core.Resources;
using Project1.Framework;

namespace Project1.Core.Blocks
{
    [EnsureStaticCtorCall]
    class BlockDefOf
    {
        static public readonly BlockDef Air = new("Air", typeof(BlockAir)) { DefaultMaterial = MaterialDefOf.Air };
        static public readonly BlockDef Grass = new("Grass", typeof(BlockGrass))
        {
            BreakProduct = MaterialRefinementDefOf.Bag,
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Structural, [MaterialRefinementDefOf.Bag])
            {
                Materials = [MaterialDefOf.Soil],
                IsDeconstructible = false
            },
        };
        static public readonly BlockDef Stone = new("Stone", typeof(BlockBedrock))
        {
            BreakProduct = MaterialRefinementDefOf.Chunk
        };
        static public readonly BlockDef Farmland = new("Farmland", typeof(BlockFarmland))
        {
            BreakProduct = MaterialRefinementDefOf.Bag
        };
        static public readonly BlockDef Cobblestone = new("Cobblestone", typeof(BlockStone))
        {
            BreakProduct = MaterialRefinementDefOf.Chunk
        };
        static public readonly BlockDef Mineral = new("Mineral", typeof(BlockMineral))
        {
            BreakProduct = MaterialRefinementDefOf.Chunk
        };
        static public readonly BlockDef Sand = new("Sand", typeof(BlockSand))
        {
            BreakProduct = MaterialRefinementDefOf.Bag,
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Structural, [MaterialRefinementDefOf.Bag])
            {
                Materials = [MaterialDefOf.Sand],
                IsDeconstructible = false
            },
        };
        static public readonly BlockDef WoodenDeck = new("WoodenDeck", typeof(BlockWoodenDeck));
        static public readonly BlockDef Soil = new("Soil", typeof(BlockSoil))
        {
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Structural, [MaterialRefinementDefOf.Bag])
            {
                Materials = [MaterialDefOf.Soil],
                IsDeconstructible = false
            },
            BreakProduct = MaterialRefinementDefOf.Bag
        };
        static public readonly BlockDef Door = new("Door", typeof(BlockDoor))
        {
            BlockEntityCompSpecs = [new BlockDoorComp.Spec()],
            ConstructionProfile = new(ConstructionCategoryDefOf.Doors, [MaterialRefinementDefOf.Planks])
        }; // TODO: different door materials???
        static public readonly BlockDef Bed = new("Bed", typeof(BlockBed))
        {
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Furniture, [MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk]),
            BlockEntityCompSpecs = [
                new BlockBuildingComp.Spec(),
                new BlockBedComp.Spec(), 
                new BlockOwnershipComp.Spec()]
        };
        static public readonly BlockDef SleepingSpot = new("SleepingSpot", typeof(BlockSleepingSpot));
        static public readonly BlockDef WoodPaneling = new("WoodPaneling", typeof(BlockWoodPaneling));
        static public readonly BlockDef Chest = new("Chest", typeof(BlockChest));
        static public readonly BlockDef Bin = new("Bin", typeof(BlockStorage));
        static public readonly BlockDef Fluid = new("Fluid", typeof(BlockFluid));
        static public readonly BlockDef Stool = new("Stool", typeof(BlockStool));
        static public readonly BlockDef Chair = new("Chair", typeof(BlockChair));
        static public readonly BlockDef Bricks = new("Bricks", typeof(BlockBricks))
        {
            //BlockEntityCompSpecs = [new BlockBuildingComp.Spec()],
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Structural, [MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Campfire = new("Campfire", typeof(BlockCampfire))
        { 
            BlockEntityCompSpecs = [
                new BlockLightComp.Spec(),
                new BlockParticlesComp.Spec(ParticleEmitter.Fire),
                new BlockSwitchableComp.Spec(ResourceDefOf.Fuel),
                new BlockResourcesComp.Spec([ResourceDefOf.Fuel])
            ]
        };
        static public readonly BlockDef Window = new("Window", typeof(BlockWindow));
        static public readonly BlockDef Roof = new("Roof", typeof(BlockRoof));
        static public readonly BlockDef Stairs = new("Stairs", typeof(BlockStairs));
        static public readonly BlockDef Counter = new("Counter", typeof(BlockCounter));
        static public readonly BlockDef Designation = new("Designation", typeof(BlockDesignation)) { BlockEntityCompSpecs = [new BlockConstructionComp.Spec()] };
        static public readonly BlockDef Slab = new("Slab", typeof(BlockSlab));
        static public readonly BlockDef Conveyor = new("Conveyor", typeof(BlockConveyor));
        static public readonly BlockDef Construction = new("Construction", typeof(BlockConstruction));
        static public readonly BlockDef ShopCounter = new("ShopCounter", typeof(Towns.Shops.Blocks.BlockShopCounter));
        static public readonly BlockDef Workbench = new("Workbench", typeof(BlockWorkstation))
        {
            Profile = WorkstationDefOf.Workbench,
            BlockEntityCompSpecs = [
                new BlockBuildingComp.Spec(),
                new BlockWorkstationComp.Spec(WorkstationDefOf.Workbench),
                new BlockResourcesComp.Spec([ResourceDefOf.RepairCharges])
                ],
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Production, [MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Smeltery = new("Smeltery", typeof(BlockWorkstation))
        {
            BlockEntityCompSpecs = [
                new BlockBuildingComp.Spec(),
                new BlockWorkstationComp.Spec(WorkstationDefOf.Smeltery),
                new BlockResourcesComp.Spec([ResourceDefOf.Fuel])
                ],
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Production, [MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Carpenter = new("Carpenter", typeof(BlockWorkstation))
        {
            BlockEntityCompSpecs = [
                new BlockBuildingComp.Spec(),
                new BlockWorkstationComp.Spec(WorkstationDefOf.Carpentry)
                ],
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Production, [MaterialRefinementDefOf.Logs, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Kitchen = new("Kitchen", typeof(BlockWorkstation))
        {
            BlockEntityCompSpecs = [
                new BlockBuildingComp.Spec(),
                new BlockWorkstationComp.Spec(WorkstationDefOf.Kitchen)
                ],
            ConstructionProfile = new ConstructionProfile(ConstructionCategoryDefOf.Production, [MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        static BlockDefOf()
        {
            Def.Register(typeof(BlockDefOf));
        }
    }
}