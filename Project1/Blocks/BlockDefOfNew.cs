namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class BlockDefOfNew
    {
        static public readonly BlockDef Air = new("Air", typeof(BlockAir));
        static public readonly BlockDef Grass = new("Grass", typeof(BlockGrass));
        static public readonly BlockDef Stone = new("Stone", typeof(BlockBedrock));
        static public readonly BlockDef Farmland = new("Farmland", typeof(BlockFarmland));
        static public readonly BlockDef Cobblestone = new("Cobblestone", typeof(BlockStone));
        static public readonly BlockDef Mineral = new("Mineral", typeof(BlockMineral));
        static public readonly BlockDef Sand = new("Sand", typeof(BlockSand));
        static public readonly BlockDef WoodenDeck = new("WoodenDeck", typeof(BlockWoodenDeck));
        static public readonly BlockDef Soil = new("Soil", typeof(BlockSoil));
        static public readonly BlockDef Door = new("Door", typeof(BlockDoor)); // TODO: different door materials???
        static public readonly BlockDef Bed = new("Bed", typeof(BlockBed));
        static public readonly BlockDef SleepingSpot = new("SleepingSpot", typeof(BlockSleepingSpot));
        static public readonly BlockDef WoodPaneling = new("WoodPaneling", typeof(BlockWoodPaneling));
        static public readonly BlockDef Chest = new("Chest", typeof(BlockChest));
        static public readonly BlockDef Bin = new("Bin", typeof(BlockStorage));
        static public readonly BlockDef Fluid = new("Fluid", typeof(BlockFluid));
        static public readonly BlockDef Stool = new("Stool", typeof(BlockStool));
        static public readonly BlockDef Chair = new("Chair", typeof(BlockChair));
        static public readonly BlockDef Bricks = new("Bricks", typeof(BlockBricks)) { ConstructionProfile = new ConstructionProfile([MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Chunk]) };
        static public readonly BlockDef Campfire = new("Campfire", typeof(BlockCampfire));
        static public readonly BlockDef Window = new("Window", typeof(BlockWindow));
        static public readonly BlockDef Roof = new("Roof", typeof(BlockRoof));
        static public readonly BlockDef Stairs = new("Stairs", typeof(BlockStairs));
        static public readonly BlockDef Counter = new("Counter", typeof(BlockCounter));
        static public readonly BlockDef Designation = new("Designation", typeof(BlockDesignation)) { BlockEntityCompSpecs = [new BlockConstructionComp.Spec()] };
        static public readonly BlockDef Slab = new("Slab", typeof(BlockSlab));
        static public readonly BlockDef Conveyor = new("Conveyor", typeof(BlockConveyor));
        static public readonly BlockDef Construction = new("Construction", typeof(BlockConstruction));
        static public readonly BlockDef ShopCounter = new("ShopCounter", typeof(BlockShopCounter));
        static public readonly BlockDef Workbench = new("Workbench", typeof(BlockWorkstation)) { BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Workbench)] };
        static public readonly BlockDef Smeltery = new("Smeltery", typeof(BlockWorkstation)) { BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Smeltery)] };
        static public readonly BlockDef Kitchen = new("Kitchen", typeof(BlockWorkstation)) { BlockEntityCompSpecs = [new BlockWorkstationComp.Spec(WorkstationDefOf.Kitchen)] };
        //static public readonly BlockDef Kitchen = new BlockDef("Kitchen", typeof(BlockWorkstation("Kitchen", typeof(BlockKitchenEntity));
        //static public readonly BlockDef PlantProcessingBench = new BlockDef("PlantProcessingBench", typeof(BlockWorkstation("PlantProcessing", typeof(BlockPlantProcessingEntity));
        //static public readonly BlockDef CarpentryBench = new BlockDef("CarpentryBench", typeof(BlockWorkstation("CarpenterBench", typeof(BlockCarpentryEntity));
        //static public readonly BlockDef Smeltery = new BlockDef("Smeltery", typeof(BlockWorkstation("Smeltery", typeof(BlockSmelteryEntity));
        static BlockDefOfNew()
        {
            Def.Register(typeof(BlockDefOfNew));
        }

        //[EnsureStaticCtorCall]
        //class BlockDefOfNew
        //{
        //    public static readonly BlockDef Air;
        //    public static readonly BlockDef Grass;
        //    public static readonly BlockDef Stone;
        //    public static readonly BlockDef Farmland;
        //    public static readonly BlockDef Cobblestone;
        //    public static readonly BlockDef Mineral;
        //    public static readonly BlockDef Sand;
        //    public static readonly BlockDef WoodenDeck;
        //    public static readonly BlockDef Soil;
        //    public static readonly BlockDef Door;
        //    public static readonly BlockDef Bed;
        //    public static readonly BlockDef WoodPaneling;
        //    public static readonly BlockDef Chest;
        //    public static readonly BlockDef Bin;
        //    public static readonly BlockDef Fluid;
        //    public static readonly BlockDef Stool;
        //    public static readonly BlockDef Chair;
        //    public static readonly BlockDef Bricks;
        //    public static readonly BlockDef Campfire;
        //    public static readonly BlockDef Window;
        //    public static readonly BlockDef Roof;
        //    public static readonly BlockDef Stairs;
        //    public static readonly BlockDef Counter;
        //    public static readonly BlockDef Designation;
        //    public static readonly BlockDef Slab;
        //    public static readonly BlockDef Conveyor;
        //    public static readonly BlockDef Prefab;
        //    public static readonly BlockDef Construction;
        //    public static readonly BlockDef ShopCounter;
        //    public static readonly BlockDef Workbench;
        //    public static readonly BlockDef Kitchen;
        //    public static readonly BlockDef PlantProcessingBench;
        //    public static readonly BlockDef CarpentryBench;
        //    public static readonly BlockDef Smeltery;

        //    static BlockDefOfNew()
        //    {
        //        var blockDefs = XDocument.Load("Content/Data/Defs/BlockDefs.xml");
        //        var thistype = typeof(BlockDefOfNew);
        //        var defs = typeof(BlockDefOfNew).GetFields().Select(f => f.GetValue(null));
        //        var xserializer = new XmlSerializer(typeof(BlockDef));
        //        foreach(var node in blockDefs.Root.Elements())
        //        {
        //            var name = node.Attribute("name").Value;
        //            using var strreader = node.CreateReader();// new StringReader(node.ToString());
        //            var field = thistype.GetField(name);
        //            var item = xserializer.Deserialize(strreader);
        //            field.SetValue(null, item);
        //        }
        //    }
        //}
    }
}
