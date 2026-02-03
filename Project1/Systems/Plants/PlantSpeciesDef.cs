using Project1.Framework.Components.Plants;
using Project1.Framework.Base;
using Project1.Framework.Skills;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Project1.Framework.Materials;

namespace Start_a_Town_
{
    public class PlantSpeciesDef : Def
    {
        internal ToolUseDef ToolToCut;
        [XmlIgnore]
        public MaterialDef FruitMaterial;
        public GrowthProperties Growth;
        public ItemFamilyDef Family;
        public int GrowthRate = Ticks.PerGameHour; //ticks per 1 growth
        public int GrowTicks;
        public int MaxYieldCutDown;
        [XmlIgnore]
        public ItemDef PlantEntity;

        public int PlantingSpacing;
        [XmlIgnore]
        public ItemDef ProductCutDown;

        /// <summary>
        /// Ticks per 1 hitpoint recovery.
        /// </summary>
        public int StemHealRate;
        [XmlIgnore]
        public MaterialDef StemMaterial;
        public string TextureFruit, TextureSeeds;
        public string SeedsName;
        public Graphics Textures;
        public int YieldThreshold;
        public PlantSpeciesDef(string name) : base(name)
        {
            
        }
        static T FromXml<T>(XmlNode xmlRoot) where T : new()
        {
            var obj = new T();

            return obj;
        }

        //internal GameObject CreateSeeds()
        //{
        //    //var seeds = ItemDefOf.Seeds.Create();
        //    var seeds = ItemDefOf.Seeds.CreateBase(this);
        //    seeds.GetComponent<SeedComponent>().SetPlant(this);
        //    seeds.Name = $"{this.Label} {this.SeedsName}";
        //    seeds.Body.Sprite = Sprite.Load(this.TextureSeeds);
        //    return seeds;
        //}

        //public Plant CreatePlant()
        //{
        //    var entity = this.PlantEntity.Create() as Plant;
        //    entity.PlantComponent.Species = this;
        //    if (this.PlantEntity == PlantDefOf.Tree)
        //        entity.SetMaterial(this.StemMaterial);
        //    else if (this.ProducesFruit)
        //        entity.Name = entity.Name.Insert(0, $"{this.FruitMaterial.Label} ");
        //    return entity;
        //}
        [Obsolete]
        public int GetCutDownHitPonts(GameObject plant) => (int)(this.StemMaterial.Density * plant.TotalWeight / 5f);

        static public void Init()
        {
            var ser = new XmlSerializer(typeof(List<PlantSpeciesDef>));
            var path = $"{GlobalVars.SaveDir}/{PlantSpeciesDefOf.Berry.Label}.xml";
            //var path = $"{GlobalVars.SaveDir}/Berry.xml";

            //Register(Berry);
            //Register(LightTree);

            System.IO.FileStream file = System.IO.File.Create(path);
            var list = new List<PlantSpeciesDef>(GetDefs<PlantSpeciesDef>());
            ser.Serialize(file, list);
            file.Close();
        }

        internal Entity Create(PlantStageDef form) => PlantSystem.Create(this, form);

        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;
        [XmlIgnore]
        public string TextureGrowing
        {
            get => this.Textures.Grown;
            set => this.Textures.Grown = value;
        }

        [XmlIgnore]
        public string TextureGrown
        {
            get => this.Textures.Growing;
            set => this.Textures.Growing = value;
        }

        public struct Graphics
        {
            public string Growing, Grown;

            public Graphics(string textureGrowing, string textureGrown)
            {
                this.Growing = textureGrowing;
                this.Grown = textureGrown;
            }
        }

      
    }
    public class PlantSpeciesOld : Def
    {
        internal ToolUseDef ToolToCut;
        [XmlIgnore]
        public MaterialDef FruitMaterial;
        public GrowthProperties Growth;

        public int GrowthRate = Ticks.PerGameHour; //ticks per 1 growth
        public int GrowTicks;
        public int MaxYieldCutDown;
        [XmlIgnore]
        public ItemDef PlantEntity;

        public int PlantingSpacing;
        [XmlIgnore]
        public ItemDef ProductCutDown;

        /// <summary>
        /// Ticks per 1 hitpoint recovery.
        /// </summary>
        public int StemHealRate;
        [XmlIgnore]
        public MaterialDef StemMaterial;
        public string TextureFruit, TextureSeeds, SeedsName;
        public Graphics Textures;
        public int YieldThreshold;

        static T FromXml<T>(XmlNode xmlRoot) where T : new()
        {
            var obj = new T();

            return obj;
        }

        //internal GameObject CreateSeeds()
        //{
        //    //var seeds = ItemDefOf.Seeds.Create();
        //    var seeds = ItemDefOf.Seeds.CreateBase(this);
        //    seeds.GetComponent<SeedComponent>().SetPlant(this);
        //    seeds.Name = $"{this.Label} {this.SeedsName}";
        //    seeds.Body.Sprite = Sprite.Load(this.TextureSeeds);
        //    return seeds;
        //}

        //public Plant CreatePlant()
        //{
        //    var entity = this.PlantEntity.Create() as Plant;
        //    entity.PlantComponent.Species = this;
        //    if (this.PlantEntity == PlantDefOf.Tree)
        //        entity.SetMaterial(this.StemMaterial);
        //    else if (this.ProducesFruit)
        //        entity.Name = entity.Name.Insert(0, $"{this.FruitMaterial.Label} ");
        //    return entity;
        //}
        [Obsolete]
        public int GetCutDownHitPonts(GameObject plant) => (int)(this.StemMaterial.Density * plant.TotalWeight / 5f);

        static public void Init()
        {
            return;
            var ser = new XmlSerializer(typeof(List<PlantSpeciesDef>));
            var path = $"{GlobalVars.SaveDir}/{PlantSpeciesDefOf.Berry.Label}.xml";
            //var path = $"{GlobalVars.SaveDir}/Berry.xml";

            //Register(Berry);
            //Register(LightTree);

            System.IO.FileStream file = System.IO.File.Create(path);
            var list = new List<PlantSpeciesDef>(GetDefs<PlantSpeciesDef>());
            ser.Serialize(file, list);
            file.Close();
        }

        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;
        [XmlIgnore]
        public string TextureGrowing
        {
            get => this.Textures.Grown;
            set => this.Textures.Grown = value;
        }

        [XmlIgnore]
        public string TextureGrown
        {
            get => this.Textures.Growing;
            set => this.Textures.Growing = value;
        }

        public struct Graphics
        {
            public string Growing, Grown;

            public Graphics(string textureGrowing, string textureGrown)
            {
                this.Growing = textureGrowing;
                this.Grown = textureGrown;
            }
        }
    }
}
