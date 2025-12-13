using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Start_a_Town_
{
    public class PlantProperties : Def
    {
        public struct Graphics
        {
            public string Growing, Grown;

            public Graphics(string textureGrowing, string textureGrown)
            {
                this.Growing = textureGrowing;
                this.Grown = textureGrown;
            }
        }
        public string TextureFruit, TextureSeeds, SeedsName;

        //public string TextureGrowing, TextureGrown;
        [XmlIgnore]
        public string TextureGrown
        {
            get => this.Textures.Growing;
            set => this.Textures.Growing = value;
        }
        [XmlIgnore]
        public string TextureGrowing
        {
            get => this.Textures.Grown;
            set => this.Textures.Grown = value;
        }
        public Graphics Textures;
        public int GrowTicks;
        public int YieldThreshold;
        public int MaxYieldCutDown;
       
        /// <summary>
        /// Ticks per 1 hitpoint recovery.
        /// </summary>
        public int StemHealRate;

        public int PlantingSpacing;
        [XmlIgnore]
        public MaterialDef StemMaterial;
        [XmlIgnore]
        public MaterialDef FruitMaterial;
        [XmlIgnore]
        public ItemDef PlantEntity;
        [XmlIgnore]
        public ItemDef ProductCutDown;

        public int GrowthRate = Ticks.PerGameHour; //ticks per 1 growth
        public GrowthProperties Growth;
        internal ToolUseDef ToolToCut;

        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;


        public PlantProperties()
        {

        }
        public PlantProperties(string name) : base(name)
        {

        }
        [Obsolete]
        public int GetCutDownHitPonts(GameObject plant) => (int)(this.StemMaterial.Density * plant.TotalWeight / 5f);

        

        public Plant CreatePlant()
        {
            var entity = this.PlantEntity.Create() as Plant;
            entity.PlantComponent.PlantProperties = this;
            if (this.PlantEntity == PlantDefOf.Tree)
                entity.SetMaterial(this.StemMaterial);
            else if (this.ProducesFruit)
                entity.Name = entity.Name.Insert(0, $"{this.FruitMaterial.Label} ");
            return entity;
        }

        static public void Init()
        {
            return;
            var ser = new XmlSerializer(typeof(List<PlantProperties>));
            var path = $"{GlobalVars.SaveDir}/{PlantPropertiesDefOf.Berry.Label}.xml";
            //var path = $"{GlobalVars.SaveDir}/Berry.xml";

            //Register(Berry);
            //Register(LightTree);

            System.IO.FileStream file = System.IO.File.Create(path);
            var list = new List<PlantProperties>(GetDefs<PlantProperties>());
            ser.Serialize(file, list);
            file.Close();
        }

        internal GameObject CreateSeeds()
        {
            //var seeds = ItemDefOf.Seeds.Create();
            var seeds = ItemDefOf.Seeds.CreateNew();
            seeds.GetComponent<SeedComponent>().SetPlant(this);
            seeds.Name = $"{this.Label} {this.SeedsName}";
            seeds.Body.Sprite = Sprite.Load(this.TextureSeeds);
            return seeds;
        }

        static T FromXml<T>(XmlNode xmlRoot) where T : new()
        {
            var obj = new T();

            return obj;
        }
    }
    [EnsureStaticCtorCall]
    static class PlantPropertiesDefOf
    {
        static public readonly PlantProperties Berry = new("Berry")
        {
            TextureGrowing = ItemContent.BerryBushGrowing.AssetPath,
            TextureGrown = ItemContent.BerryBushGrown.AssetPath,
            TextureFruit = ItemContent.BerryBushFruit.AssetPath,
            TextureSeeds = ItemContent.SeedsFull.AssetPath,
            SeedsName = "Seeds",
            StemMaterial = MaterialDefOf.ShrubStem,
            FruitMaterial = MaterialDefOf.Berry,
            Growth = new GrowthProperties(ItemDefOf.Fruit, MaterialDefOf.Berry, 5, 6),
            PlantEntity = PlantDefOf.Bush,
        };

        static public readonly PlantProperties LightTree = new("LightTree")
        {
            TextureGrowing = ItemContent.TreeFull.AssetPath,
            TextureGrown = ItemContent.TreeFull.AssetPath,
            TextureSeeds = ItemContent.Sapling.AssetPath,
            SeedsName = "Saplings",
            StemMaterial = MaterialDefOf.LightWood,
            ProductCutDown = RawMaterialDefOf.Logs,
            MaxYieldCutDown = 5,
            GrowTicks = 6 * Ticks.PerSecond,
            PlantEntity = PlantDefOf.Tree,
            ToolToCut = ToolUseDefOf.Chopping,
            StemHealRate = Ticks.FromHours(1),
            PlantingSpacing = 1
        };
        static PlantPropertiesDefOf()
        {
            Def.Register(typeof(PlantPropertiesDefOf));
        }
    }
}
