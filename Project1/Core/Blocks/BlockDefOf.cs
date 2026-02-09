using Project1.Core.Blocks;
using Project1.Core.Blocks.Doors;
using Project1.Core.Materials;
using Project1.Core.Base;
using Project1.Core.Crafting;

namespace Project1.Core
{
    [EnsureStaticCtorCall]
    class BlockDefOf
    {
        static public readonly BlockDef Air = new("Air", typeof(BlockAir)) { DefaultMaterial = MaterialDefOf.Air };
        static public readonly BlockDef Grass = new("Grass", typeof(BlockGrass));
        static public readonly BlockDef Stone = new("Stone", typeof(BlockBedrock));
        static public readonly BlockDef Farmland = new("Farmland", typeof(BlockFarmland));
        static public readonly BlockDef Cobblestone = new("Cobblestone", typeof(BlockStone));
        static public readonly BlockDef Mineral = new("Mineral", typeof(BlockMineral));
        static public readonly BlockDef Sand = new("Sand", typeof(BlockSand));
        static public readonly BlockDef WoodenDeck = new("WoodenDeck", typeof(BlockWoodenDeck));
        static public readonly BlockDef Soil = new("Soil", typeof(BlockSoil));
        static public readonly BlockDef Door = new("Door", typeof(BlockDoor))
        {
            BlockEntityCompSpecs = [new BlockDoorComp.Spec()]
        }; // TODO: different door materials???
        static public readonly BlockDef Bed = new("Bed", typeof(BlockBed))
        {
            ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk]),
            BlockEntityCompSpecs = [new BlockBedComp.Spec(), new BlockOwnershipComp.Spec()]
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
            ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Campfire = new("Campfire", typeof(BlockCampfire));
        static public readonly BlockDef Window = new("Window", typeof(BlockWindow));
        static public readonly BlockDef Roof = new("Roof", typeof(BlockRoof));
        static public readonly BlockDef Stairs = new("Stairs", typeof(BlockStairs));
        static public readonly BlockDef Counter = new("Counter", typeof(BlockCounter));
        static public readonly BlockDef Designation = new("Designation", typeof(BlockDesignation)) { BlockEntityCompSpecs = [new BlockConstructionComp.Spec()] };
        static public readonly BlockDef Slab = new("Slab", typeof(BlockSlab));
        static public readonly BlockDef Conveyor = new("Conveyor", typeof(BlockConveyor));
        static public readonly BlockDef Construction = new("Construction", typeof(BlockConstruction));
        static public readonly BlockDef ShopCounter = new("ShopCounter", typeof(Project1.Core.Towns.Shops.Blocks.BlockShopCounter));
        static public readonly BlockDef Workbench = new("Workbench", typeof(BlockWorkstation))
        {
            Profile = WorkstationDefOf.Workbench,
            BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Workbench)],
            ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Smeltery = new("Smeltery", typeof(BlockWorkstation))
        {
            BlockEntityCompSpecs = [
                new BlockWorkstationComp.Spec(WorkstationDefOf.Smeltery),
                new BlockFuelComp.Spec()],
            ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Chunk])
        };
        static public readonly BlockDef Kitchen = new("Kitchen", typeof(BlockWorkstation))
        {
            BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Kitchen)],
            ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])
        };
        //static public readonly BlockDef Kitchen = new BlockDef("Kitchen", typeof(BlockWorkstation("Kitchen", typeof(BlockKitchenEntity));
        //static public readonly BlockDef PlantProcessingBench = new BlockDef("PlantProcessingBench", typeof(BlockWorkstation("PlantProcessing", typeof(BlockPlantProcessingEntity));
        //static public readonly BlockDef CarpentryBench = new BlockDef("CarpentryBench", typeof(BlockWorkstation("CarpenterBench", typeof(BlockCarpentryEntity));
        //static public readonly BlockDef Smeltery = new BlockDef("Smeltery", typeof(BlockWorkstation("Smeltery", typeof(BlockSmelteryEntity));
        static BlockDefOf()
        {
            Def.Register(typeof(BlockDefOf));
        }
    }
}
