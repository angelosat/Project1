namespace Start_a_Town_
{
    static class BlockDefOf
    {
        static public readonly Block Air = new BlockAir();
        static public readonly Block Grass = new BlockGrass();
        static public readonly Block Stone = new BlockBedrock();
        static public readonly Block Farmland = new BlockFarmland();
        static public readonly Block Cobblestone = new BlockStone();
        static public readonly Block Mineral = new BlockMineral();
        static public readonly Block Sand = new BlockSand();
        static public readonly Block WoodenDeck = new BlockWoodenDeck();
        static public readonly Block Soil = new BlockSoil();
        static public readonly Block Door = new BlockDoor(); // TODO: different door materials???
        static public readonly Block Bed = new BlockBed();
        static public readonly Block SleepingSpot = new BlockSleepingSpot();
        static public readonly Block WoodPaneling = new BlockWoodPaneling();
        static public readonly Block Chest = new BlockChest();
        static public readonly Block Bin = new BlockStorage();
        static public readonly Block Fluid = new BlockFluid();
        static public readonly Block Stool = new BlockStool();
        static public readonly Block Chair = new BlockChair();
        static public readonly Block Bricks = new BlockBricks();
        static public readonly Block Campfire = new BlockCampfire();
        static public readonly Block Window = new BlockWindow();
        static public readonly Block Roof = new BlockRoof();
        static public readonly Block Stairs = new BlockStairs();
        static public readonly Block Counter = new BlockCounter();
        static public readonly Block Designation = new BlockDesignation();
        static public readonly Block Slab = new BlockSlab();
        static public readonly Block Conveyor = new BlockConveyor();
        static public readonly Block Construction = new BlockConstruction();
        static public readonly Block ShopCounter = new BlockShopCounter();
        static public readonly Block Workbench = new BlockWorkstation("Workbench", typeof(BlockWorkbenchEntity));
        static public readonly Block Kitchen = new BlockWorkstation("Kitchen", typeof(BlockKitchenEntity));
        static public readonly Block PlantProcessingBench = new BlockWorkstation("PlantProcessing", typeof(BlockPlantProcessingEntity));
        static public readonly Block CarpentryBench = new BlockWorkstation("CarpenterBench", typeof(BlockCarpentryEntity));
        static public readonly Block Smeltery = new BlockWorkstation("Smeltery", typeof(BlockSmelteryEntity));
        static BlockDefOf() { }
        internal static void Init() { }

    }
}
