namespace Start_a_Town_
{
    static class BlockDefOf
    {
        static public readonly Block Air = BlockDefOfNew.Air.Worker;// new BlockAir();
        static public readonly Block Grass = BlockDefOfNew.Grass.Worker;// new BlockGrass();
        static public readonly Block Stone = BlockDefOfNew.Stone.Worker;// new BlockBedrock();
        static public readonly Block Cobblestone = BlockDefOfNew.Cobblestone.Worker;// new BlockStone();
        static public readonly Block Mineral = BlockDefOfNew.Mineral.Worker;// new BlockMineral();
        static public readonly Block Sand = BlockDefOfNew.Sand.Worker;// new BlockSand();
        static public readonly Block Fluid = BlockDefOfNew.Fluid.Worker;// new BlockFluid();
        static public readonly Block Soil = BlockDefOfNew.Soil.Worker;// new BlockSoil();
        static public readonly Block Door = BlockDefOfNew.Door.Worker;// new BlockDoor(); // TODO: different door materials???
        static public readonly Block Bed = BlockDefOfNew.Bed.Worker;// new BlockBed();
        static public readonly Block SleepingSpot = BlockDefOfNew.SleepingSpot.Worker;// new BlockSleepingSpot();
        static public readonly Block WoodPaneling = BlockDefOfNew.WoodPaneling.Worker;//  new BlockWoodPaneling();
        static public readonly Block Chest = BlockDefOfNew.Chest.Worker;// new BlockChest();
        static public readonly Block Bin = BlockDefOfNew.Bin.Worker;// new BlockStorage();
        static public readonly Block Bricks = BlockDefOfNew.Bricks.Worker;// new BlockBricks() { ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk])};
        static public readonly Block Campfire = BlockDefOfNew.Campfire.Worker;//new BlockCampfire();
        static public readonly Block Window = BlockDefOfNew.Window.Worker;//new BlockWindow();
        static public readonly Block Roof = BlockDefOfNew.Roof.Worker;//new BlockRoof();
        static public readonly Block Stairs = BlockDefOfNew.Stairs.Worker;//new BlockStairs();
        static public readonly Block Counter = BlockDefOfNew.Counter.Worker;//new BlockCounter();
        static public readonly Block Designation = BlockDefOfNew.Designation.Worker;//new BlockDesignation() { BlockEntityCompSpecs = [new BlockConstructionComp.Spec()] };
        static public readonly Block Slab = BlockDefOfNew.Slab.Worker;//new BlockSlab();
        static public readonly Block Conveyor = BlockDefOfNew.Conveyor.Worker;// new BlockConveyor();
        static public readonly Block Construction = BlockDefOfNew.Construction.Worker;//new BlockConstruction();
        static public readonly Block ShopCounter = BlockDefOfNew.ShopCounter.Worker;//new BlockShopCounter();
        static public readonly Block Workbench = new BlockWorkstation("Workbench") { BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Smeltery)] };
        static public readonly Block Kitchen = new BlockWorkstation("Kitchen");
        //static public readonly Block PlantProcessingBench = new BlockWorkstation("PlantProcessing");
        //static public readonly Block CarpentryBench = new BlockWorkstation("CarpenterBench");
        //static public readonly Block Smeltery = new BlockWorkstation("Smeltery");
        //static public readonly Block Workbench = new BlockWorkstation("Workbench", typeof(BlockWorkbenchEntity)) { BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Smeltery)] };
        //static public readonly Block Kitchen = new BlockWorkstation("Kitchen", typeof(BlockKitchenEntity));
        //static public readonly Block PlantProcessingBench = new BlockWorkstation("PlantProcessing", typeof(BlockPlantProcessingEntity));
        //static public readonly Block CarpentryBench = new BlockWorkstation("CarpenterBench", typeof(BlockCarpentryEntity));
        //static public readonly Block Smeltery = new BlockWorkstation("Smeltery", typeof(BlockSmelteryEntity));
        static BlockDefOf() { }
        internal static void Init() { }

    }
}
