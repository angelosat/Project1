namespace Start_a_Town_
{
    /// <summary>
    /// not yet in use
    /// </summary>
    class BlockDefOfNew
    {
        public static readonly BlockDef Air = new("Air")
        {
            BlockType = typeof(BlockAir)
        };
        public static readonly BlockDef Grass = new("Grass")
        {
            BlockType = typeof(BlockGrass)
        };
        public static readonly BlockDef Stone = new("Bedrock")
        {
            BlockType = typeof(BlockBedrock)
        };
        public static readonly BlockDef Farmland = new("Farmland")
        {
            BlockType = typeof(BlockFarmland)
        };
        public static readonly BlockDef Cobblestone = new("Stone")
        {
            BlockType = typeof(BlockStone)
        };
        public static readonly BlockDef Mineral = new("Mineral")
        {
            BlockType = typeof(BlockMineral)
        };
        public static readonly BlockDef Sand = new("Sand")
        {
            BlockType = typeof(BlockSand)
        };
        public static readonly BlockDef WoodenDeck = new("Wooden Deck")
        {
            BlockType = typeof(BlockWoodenDeck)
        };
        public static readonly BlockDef Soil = new("Soil")
        {
            BlockType = typeof(BlockSoil)
        };
        public static readonly BlockDef Door = new("Door")
        {
            BlockType = typeof(BlockDoor)
        };
        public static readonly BlockDef Bed = new("Bed")
        {
            BlockType = typeof(BlockBed)
        };
        public static readonly BlockDef WoodPaneling = new("WoodPaneling")
        {
            BlockType = typeof(BlockWoodPaneling)
        };
        public static readonly BlockDef Chest = new("Chest")
        {
            BlockType = typeof(BlockChest)
        };
        public static readonly BlockDef Bin = new("Bin")
        {
            BlockType = typeof(BlockStorage)
        };
        public static readonly BlockDef Fluid = new("Fluid")
        {
            BlockType = typeof(BlockFluid)
        };
        public static readonly BlockDef Stool = new("Stool")
        {
            BlockType = typeof(BlockStool)
        };
        public static readonly BlockDef Chair = new("Chair")
        {
            BlockType = typeof(BlockChair)
        };
        public static readonly BlockDef Bricks = new("Bricks")
        {
            BlockType = typeof(BlockBricks)
        };
        public static readonly BlockDef Campfire = new("Campfire")
        {
            BlockType = typeof(BlockCampfire)
        };
        public static readonly BlockDef Window = new("Window")
        {
            BlockType = typeof(BlockWindow)
        };
        public static readonly BlockDef Roof = new("Roof")
        {
            BlockType = typeof(BlockRoof)
        };
        public static readonly BlockDef Stairs = new("Stairs")
        {
            BlockType = typeof(BlockStairs)
        };
        public static readonly BlockDef Counter = new("Counter")
        {
            BlockType = typeof(BlockCounter)
        };
        public static readonly BlockDef Designation = new("Designation")
        {
            BlockType = typeof(BlockDesignation)
        };
        public static readonly BlockDef Slab = new("Slab")
        {
            BlockType = typeof(BlockSlab)
        };
        public static readonly BlockDef Conveyor = new("Conveyor")
        {
            BlockType = typeof(BlockConveyor)
        };
        public static readonly BlockDef Prefab = new("Prefab")
        {
            BlockType = typeof(BlockPrefab)
        };
        public static readonly BlockDef Construction = new("Construction")
        {
            BlockType = typeof(BlockConstruction)
        };
        public static readonly BlockDef ShopCounter = new("ShopCounter")
        {
            BlockType = typeof(BlockShopCounter)
        };
        public static readonly BlockDef Workbench = new("Workbench")
        {
            BlockType = typeof(BlockWorkstation),
            BlockEntityType = typeof(BlockWorkbenchEntity)
        };
        public static readonly BlockDef Kitchen = new("Kitchen")
        {
            BlockType = typeof(BlockWorkstation),
            BlockEntityType = typeof(BlockKitchenEntity)
        };
        public static readonly BlockDef PlantProcessingBench = new("PlantProcessingBench")
        {
            BlockType = typeof(BlockWorkstation),
            BlockEntityType = typeof(BlockPlantProcessingEntity)
        };
        public static readonly BlockDef CarpentryBench = new("CarpentryBench")
        {
            BlockType = typeof(BlockWorkstation),
            BlockEntityType = typeof(BlockCarpentryEntity)
        };
        public static readonly BlockDef Smeltery = new("Smeltery")
        {
            BlockType = typeof(BlockWorkstation),
            BlockEntityType = typeof(BlockSmelteryEntity)
        };
        static BlockDefOfNew()
        {
            //Def.Register(typeof(BlockDefOfNew));
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
